﻿// 
// EngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    public delegate void BoardUpdatedEventHandler(Board board);

    public delegate void EngineTextUpdatedEventHandler(string engineText);

    public delegate void SelectedPieceUpdatedEventHandler(PieceName pieceName);

    public delegate void SelectedTargetPositionUpdatedEventHandler(Position position);

    public class EngineWrapper
    {
        public Board Board
        {
            get
            {
                return _board;
            }
            private set
            {
                _board = value;
                OnBoardUpdate(Board);
            }
        }
        private Board _board = null;

        public MoveSet ValidMoves { get; private set; }


        public bool GameInProgress
        {
            get
            {
                return (null != Board && (Board.BoardState == BoardState.NotStarted || Board.BoardState == BoardState.InProgress));
            }
        }

        public PieceName SelectedPiece
        {
            get
            {
                return _selectedPiece;
            }
            set
            {
                PieceName oldValue = _selectedPiece;

                _selectedPiece = value;

                if (oldValue != value)
                {
                    OnSelectedPieceUpdate(SelectedPiece);
                }
            }
        }
        private PieceName _selectedPiece = PieceName.INVALID;

        public Position SelectedTargetPosition
        {
            get
            {
                return _selectedTargetPosition;
            }
            set
            {
                Position oldValue = _selectedTargetPosition;

                _selectedTargetPosition = value;

                if (oldValue != value)
                {
                    OnSelectedTargetPositionUpdate(SelectedTargetPosition);
                }
            }
        }
        private Position _selectedTargetPosition = null;

        public bool CanPlaySelected
        {
            get
            {
                return (GameInProgress && SelectedPiece != PieceName.INVALID && null != SelectedTargetPosition);
            }
        }

        public bool CanPass
        {
            get
            {
                return (GameInProgress && null != ValidMoves && ValidMoves.Contains(Move.Pass));
            }
        }

        public bool CanUndoLastMove
        {
            get
            {
                return (null != Board && Board.BoardState != BoardState.NotStarted);
            }
        }

        public string EngineText
        {
            get
            {
                return _engineText.ToString();
            }
        }
        private StringBuilder _engineText;

        public event BoardUpdatedEventHandler BoardUpdated;
        public event EngineTextUpdatedEventHandler EngineTextUpdated;

        public event SelectedPieceUpdatedEventHandler SelectedPieceUpdated;
        public event SelectedTargetPositionUpdatedEventHandler SelectedTargetPositionUpdated;

        private Process _process;
        private StreamReader _reader;
        private StreamWriter _writer;

        public EngineWrapper(string engineCommand)
        {
            _engineText = new StringBuilder();
            StartEngine(engineCommand);
        }

        private void StartEngine(string engineName)
        {
            if (String.IsNullOrWhiteSpace(engineName))
            {
                throw new ArgumentNullException("engineName");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = engineName;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            _engineText.Clear();

            _process = Process.Start(startInfo);
            _reader = _process.StandardOutput;
            _writer = _process.StandardInput;

            ReadEngineOutput(EngineCommand.Info);
        }

        public void Close()
        {
            _writer.WriteLine("exit");
            _process.WaitForExit();
            _process.Close();
            _process = null;
        }

        public void NewGame()
        {
            SendCommand("newgame");
        }

        public void PlaySelected()
        {
            if (SelectedPiece == PieceName.INVALID || null == SelectedTargetPosition)
            {
                throw new Exception("Please select a piece and destination first.");
            }

            Move move = new Move(SelectedPiece, SelectedTargetPosition);

            SendCommand("play {0}", move);
        }

        public void Pass()
        {
            SendCommand("pass");
        }

        public void UndoLastMove()
        {
            SendCommand("undo");
        }

        public void PlayBestMove()
        {
            SendCommand("play");
        }

        public void FindBestMove()
        {
            SendCommand("bestmove");
        }

        public void SendCommand(string format, params object[] args)
        {
            SendCommand(String.Format(format, args));
        }

        public void SendCommand(string command)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            EngineCommand cmd = IdentifyCommand(command);

            if (cmd == EngineCommand.Exit)
            {
                throw new Exception("Can't send exit command.");
            }

            EngineTextAppendLine(command);
            _writer.WriteLine(command);
            ReadEngineOutput(cmd);
        }

        private EngineCommand IdentifyCommand(string command)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            string[] split = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string cmd = split[0].ToLower();

            switch (cmd)
            {
                case "info":
                    return EngineCommand.Info;
                case "?":
                case "help":
                    return EngineCommand.Help;
                case "board":
                    return EngineCommand.Board;
                case "newgame":
                    return EngineCommand.NewGame;
                case "play":
                    return EngineCommand.Play;
                case "pass":
                    return EngineCommand.Pass;
                case "validmoves":
                    return EngineCommand.ValidMoves;
                case "bestmove":
                    return EngineCommand.BestMove;
                case "undo":
                    return EngineCommand.Undo;
                case "history":
                    return EngineCommand.History;
                case "exit":
                    return EngineCommand.Exit;
                default:
                    return EngineCommand.Unknown;
            }
        }

        private void ReadEngineOutput(EngineCommand command)
        {
            StringBuilder sb = new StringBuilder();

            string errorMessage = "";
            string invalidMoveMessage = "";

            string line;
            while (null != (line = _reader.ReadLine()))
            {
                EngineTextAppendLine(line);
                if (line == "ok")
                {
                    break;
                }
                else
                {
                    if (line.StartsWith("err"))
                    {
                        errorMessage = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.StartsWith("invalidmove"))
                    {
                        invalidMoveMessage = line.Substring(line.IndexOf(' ') + 1);
                    }
                    sb.AppendLine(line);
                }
            }

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                throw new EngineException(errorMessage);
            }

            if (!String.IsNullOrWhiteSpace(invalidMoveMessage))
            {
                throw new InvalidMoveException(invalidMoveMessage);
            }

            string[] outputSplit = sb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            string firstLine = "";

            if (null != outputSplit && outputSplit.Length > 0)
            {
                firstLine = outputSplit[0];
            }

            // Update other properties
            switch (command)
            {
                case EngineCommand.Board:
                case EngineCommand.NewGame:
                case EngineCommand.Play:
                case EngineCommand.Pass:
                case EngineCommand.Undo:
                    Board = !String.IsNullOrWhiteSpace(firstLine) ? new Board(firstLine) : null;
                    break;
                case EngineCommand.ValidMoves:
                    ValidMoves = !String.IsNullOrWhiteSpace(firstLine) ? new MoveSet(firstLine) : null;
                    break;
                case EngineCommand.BestMove:
                    if (!String.IsNullOrWhiteSpace(firstLine))
                    {
                        Move bestMove = new Move(firstLine);
                        SelectedPiece = bestMove.PieceName;
                        SelectedTargetPosition = bestMove.Position;
                    }
                    break;
                case EngineCommand.History:
                case EngineCommand.Info:
                case EngineCommand.Help:
                default:
                    break;
            }
        }

        private void EngineTextAppendLine(string line)
        {
            _engineText.AppendLine(line);
            OnEngineTextUpdate(EngineText);
        }

        public PieceName GetPieceAt(double cursorX, double cursorY, double hexRadius)
        {
            Position position = Position.FromCursor(cursorX, cursorY, hexRadius);

            Piece topPiece = (null != Board) ? Board.GetPieceOnTop(position) : null;
            return ((null != topPiece) ? topPiece.PieceName : PieceName.INVALID);
        }

        public Position GetTargetPositionAt(double cursorX, double cursorY, double hexRadius)
        {
            Position bottomPosition = Position.FromCursor(cursorX, cursorY, hexRadius);

            Piece topPiece = (null != Board) ? Board.GetPieceOnTop(bottomPosition) : null;

            if (null == topPiece)
            {
                // No piece there, return position at bottom of the stack (stack == 0)
                return bottomPosition;
            }
            else
            {
                // Piece present, return position on top of the piece
                return topPiece.Position.GetShifted(0, 0, 0, 1);
            }
        }

        private void OnBoardUpdate(Board board)
        {
            SelectedPiece = PieceName.INVALID;

            SendCommand("validmoves");

            if (null != BoardUpdated)
            {
                BoardUpdated(board);
            }
        }

        private void OnEngineTextUpdate(string engineText)
        {
            if (null != EngineTextUpdated)
            {
                EngineTextUpdated(engineText);
            }
        }

        private void OnSelectedPieceUpdate(PieceName pieceName)
        {
            SelectedTargetPosition = null;

            if (null != SelectedPieceUpdated)
            {
                SelectedPieceUpdated(pieceName);
            }
        }

        private void OnSelectedTargetPositionUpdate(Position position)
        {
            if (null != SelectedTargetPositionUpdated)
            {
                SelectedTargetPositionUpdated(position);
            }
        }

        private enum EngineCommand
        {
            Unknown = -1,
            Info = 0,
            Help,
            Board,
            NewGame,
            Play,
            Pass,
            ValidMoves,
            BestMove,
            Undo,
            History,
            Exit
        }
    }
}