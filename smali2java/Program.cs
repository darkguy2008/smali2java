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
            //String sFile = @"PUT YOUR SMALI FILE PATH HERE";
            String sFile = @"C:\users\dragon\desktop\helloworld.txt";

            Console.WriteLine("[");
            SmaliEngine e = new SmaliEngine();
            Console.Write(e.Decompile(sFile));
            Console.WriteLine("]");
            Console.ReadKey();

        }
    }
}
