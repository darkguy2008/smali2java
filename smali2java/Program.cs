using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ERROR: No Input File!");
                Environment.Exit(1);
            }
            String sFile = args[0]; // You can also put @"C:\users\youruser\desktop\smalifile.smali"

            Console.WriteLine("[");
            SmaliEngine e = new SmaliEngine();
            Console.Write(e.Decompile(sFile));
            Console.WriteLine("]");
            Console.ReadKey();
        }
    }
}
