﻿// 
// EnumMatchToBooleanConverter.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
#elif WINDOWS_WPF
using System.Globalization;
using System.Windows.Data;
#endif

// Class adapted from http://wpftutorial.net/RadioButton.html

namespace Mzinga.SharedUX
{
    public class EnumMatchToBooleanConverter : IValueConverter
    {
#if WINDOWS_UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            if (null == value || null == parameter)
            {
                return false;
            }

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();

            return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

#if WINDOWS_UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            if (null == value || null == parameter)
            {
                return null;
            }

            bool? useValue = value as bool?;

            if (useValue.HasValue && useValue.Value)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }

            return null;
        }
    } 
}
