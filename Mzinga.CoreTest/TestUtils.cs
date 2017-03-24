﻿// 
// TestUtils.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mzinga.CoreTest
{
    public class TestUtils
    {
        public static void AssertExceptionThrown<T>(Action action) where T : Exception
        {
            bool exceptionThrown = false;

            try
            {
                action();
            }
            catch (T)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown);
        }

        public static void AssertHaveEqualChildren<T>(List<T> expected, List<T> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            foreach (T item in expected)
            {
                Assert.IsTrue(actual.Contains(item));
            }
        }

        public static void AssertCompareToLessThan<T>(T lesser, T greater) where T : IComparable<T>
        {
            Assert.IsTrue(lesser.CompareTo(greater) < 0);
        }

        public static void AssertCompareToGreaterThan<T>(T greater, T lesser) where T : IComparable<T>
        {
            Assert.IsTrue(greater.CompareTo(lesser) > 0);
        }

        public static void AssertCompareToEqualTo<T>(T object1, T object2) where T : IComparable<T>
        {
            Assert.IsTrue(object1.CompareTo(object2) == 0);
        }

        public static string[] NullOrWhiteSpaceStrings = new string[]
        {
            null,
            string.Empty,
            " ",
            "\t",
            "  ",
            "\t\t",
            " \t ",
            "\t \t",
        };
    }
}
