using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java_v4.Parser
{
    public class LineGroup
    {
        public List<Line> Lines = new List<Line>();

        public void Add(Line l)
        {
            Lines.Add(l);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            foreach (Line l in Lines)
                sb.AppendLine(l.Raw);
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}
