﻿using System.Text.RegularExpressions;

namespace Interface
{
    public static class Extensions
    {
        public static bool IsNumeric(this string value)
        {
            return !Regex.IsMatch(value, @"[a-zA-Z]");
        }
    }
}
