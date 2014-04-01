using Smali2Java_v4.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java_v4
{
    class Program
    {
        public static void Main(string[] args)
        {
            ParserSmali p = new ParserSmali();
            p.Parse("Samples\\NetqinSmsFilter.smali");

            Console.ReadKey();
        }
    }
}
