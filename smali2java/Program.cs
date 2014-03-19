using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    class Program
    {
        public static bool Debug = true;
        public static void Main(string[] args)
        {
            if (args.Length == 0)
                Console.WriteLine("ERROR: No Input File! using sample file\n");

            String sFile = args.Length > 0 ? args[0] : "Samples\\AndroidHandler.smali";
            string destFile = args.Length > 1 ? args[1] : null;

            SmaliEngine e = new SmaliEngine();
            string contents = e.Indent(e.Decompile(sFile));
            if (string.IsNullOrEmpty(destFile))
            {
                Console.BufferHeight = 10000;
                Console.WindowWidth = Console.LargestWindowWidth;
                Console.BufferWidth = Console.LargestWindowWidth;

                Console.WriteLine("Decompiling file: " + sFile);
                Console.WriteLine("[");

                Console.Write(contents);
                Console.WriteLine("]");
                Console.ReadKey();
            }
            else
            {
                System.IO.File.WriteAllText(destFile, contents);
            }
        }
    }
}
