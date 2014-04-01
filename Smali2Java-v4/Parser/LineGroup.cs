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

        public String ToJava()
        {
            StringBuilder sb = new StringBuilder();
            if (Lines.Count > 0)
            {
                foreach (Line l in Lines)
                {
                    if (l.Type == ELineType.Directive)
                    {
                        switch (l.Directive)
                        {
                            case ELineDirective.Class:
                                sb.Append("class " + l.Raw);
                                break;
                            case ELineDirective.Super:
                                sb.Append("extends " + l.Raw);
                                break;
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append(ToJava());
            sb.Append("]");
            return sb.ToString();
        }
    }
}
