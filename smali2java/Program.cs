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

            Console.WriteLine("Decompiling file: " + sFile);
            Console.WriteLine("[");
            SmaliEngine e = new SmaliEngine();
            Console.Write(e.Decompile(sFile));
            Console.WriteLine("]");
            Console.ReadKey();
        }
    }
}
