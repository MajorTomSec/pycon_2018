// 
// This file is licensed under the terms of the Simple Non Code License (SNCL) 2.3.0.
// The full license text can be found in the file named LICENSE.txt.
// Written originally by Alexandre Quoniou in 2018.
//

using System;
using System.Text;

namespace Nepgya
{
    public static class StaticRandom
    {
        private static readonly Random Random = new Random();
        private static readonly object Synchronized = new object();

        public static int Next()
        {
            lock (Synchronized)
            {
                return Random.Next();
            }
        }

        public static int Next(int maxValue)
        {
            lock (Synchronized)
            {
                return Random.Next(maxValue);
            }
        }

        public static int Next(int minValue, int maxValue)
        {
            lock (Synchronized)
            {
                return Random.Next(minValue, maxValue);
            }
        }

        public static double NextDouble()
        {
            lock (Synchronized)
            {
                return Random.NextDouble();
            }
        }

        public static void NextBytes(byte[] buffer)
        {
            lock (Synchronized)
            {
                Random.NextBytes(buffer);
            }
        }

        public static string NextString(int length, string charset)
        {
            var builder = new StringBuilder();
            while (length-- > 0)
            {
                builder.Append(charset[Next(charset.Length)]);
            }

            return builder.ToString();
        }
    }
}