using System;
using System.Linq;

namespace WPD
{
    public static class SharedMethods
    {
        public static void ErrorExit(string errorMsg)
        {
            Log.Fatal(errorMsg);
            throw new Exception(errorMsg);
        }

        public const string RecordsList = "!!WPD_Records.txt";

        public static readonly string[] DataSplitChar = [" |-| "];

        private static readonly char[] IllegalCharsArray = ['\\', '/', ':', '*', '?', '"', '<', '>', '|'];

        public static string RemoveIllegalChars(string inputString)
        {
            var processedString = "";
            foreach (var c in inputString)
            {
                if (IllegalCharsArray.Contains(c))
                {
                    processedString += "";
                }
                else
                {
                    processedString += c;
                }
            }

            return processedString;
        }
    }
}