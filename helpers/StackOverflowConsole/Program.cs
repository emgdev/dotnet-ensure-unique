﻿using System;

namespace StackOverflowConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Method(1);
        }

        static void Method(int value)
        {
            Console.WriteLine($"Value: {value}");
            Method(value + 1);
        }
    }
}
