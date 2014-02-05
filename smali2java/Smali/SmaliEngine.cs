using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public String Indent(String java)
        {
            int indentation = 0;
            String indenter = "    ";
            StringBuilder sb = new StringBuilder();

            string[] lines = java.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].TrimStart().Contains("}"))
                {
                    indentation--;
                }

                for (int n = 0; n < indentation; n++)
                {
                    sb.Append(indenter);
                }
                sb.AppendFormat("{0}\n", lines[i]);

                if (lines[i].TrimStart().Contains("{"))
                {
                    indentation++;
                }
            }

            return sb.ToString();
        }
    }
}
