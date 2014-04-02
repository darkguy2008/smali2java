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
                bool bIsClass = false;
                foreach (Line l in Lines)
                {
                    if (l.Type == ELineType.Directive)
                    {
                        switch (l.Directive)
                        {
                            case ELineDirective.Class:
                                bIsClass = true;
                                sb.Append(ParseAccesors(l.Tokens.Skip(1).Take(l.Tokens.Count - 2)));
                                sb.Append(" class ");
                                sb.Append(ParseObject(l.Tokens.Last()));
                                break;
                            case ELineDirective.Super:
                                sb.Append(" extends ");
                                sb.Append(ParseObject(l.Tokens.Last()));
                                break;
                            case ELineDirective.Field:
                                sb.Append(ParseAccesors(l.Tokens.Skip(1).Take(l.Tokens.Count - 2)));
                                sb.Append(" " + ParseObject(l.Tokens.Last()) + ";");
                                break;
                        }
                    }
                }

                if (bIsClass)
                    sb.Append(" {");
            }
            return sb.ToString();
        }

        private String ParseObject(string obj)
        {
            obj = obj.Trim();
            if(!String.IsNullOrEmpty(obj)) {
                if (obj.Contains(":"))
                {
                    String[] split = obj.Split(':');
                    split[1] = ParseObject(split[1]);
                    return split[1] + " " + split[0];
                }
                else
                {
                    if (obj[0] == 'L')
                        obj = obj.Substring(1);
                }
                obj = obj.Replace('/', '.');
                obj = obj.Replace(";", "");
            }
            return obj;
        }

        public String ParseAccesors(IEnumerable<String> acc)
        {
            return String.Join(" ", acc.ToArray());
        }

        public override string ToString()
        {
            return ToJava();
        }
    }
}
