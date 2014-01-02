using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smali2Java
{
    public class SmaliEngine
    {
        public static SmaliVM VM = new SmaliVM();
        private List<SmaliLine> Lines = new List<SmaliLine>();

        public String Decompile(String sFilename)
        {
            String rv = String.Empty;

            foreach (String s in File.ReadAllLines(sFilename).Where(x => !String.IsNullOrEmpty(x)))
            {
                SmaliLine l = SmaliLine.Parse(s);
                if (l != null)
                    Lines.Add(l);
            }
            
            SmaliClass c = new SmaliClass();
            c.Lines = Lines;
            c.LoadAttributes();
            c.LoadFields();
            c.LoadMethods();

            StringBuilder sb = new StringBuilder();
            
            sb.Append(c.ToJava());
            
            rv = sb.ToString();            
            return rv;
        }

    }
}
