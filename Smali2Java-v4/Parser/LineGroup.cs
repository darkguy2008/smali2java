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
                            case ELineDirective.Method:
                                sb.Append(ParseAccesors(l.Tokens.Skip(1).Take(l.Tokens.Count - 2)));
                                sb.Append(" " + ParseMethod(l.Tokens.Last()));
                                sb.AppendLine(" {");
                                break;
                            case ELineDirective.EndMethod:
                                sb.Append("}");
                                break;
                        }
                    }
                }

                if (bIsClass)
                    sb.Append(" {");
            }
            return sb.ToString();
        }

        private String ParseMethod(String method)
        {
            StringBuilder sb = new StringBuilder();
            String rType = method.Substring(method.LastIndexOf(')') + 1);
            rType = ParseObject(rType);

            String mName = method.Substring(0, method.IndexOf('('));

            String mArgs = method.Substring(method.IndexOf('(') + 1);
            mArgs = mArgs.Substring(0, mArgs.LastIndexOf(')'));

            sb.Append(rType);
            sb.Append(" " + mName + "(");
            if (mArgs.Length > 0)
            {
                String[] args = mArgs.Split(new String[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                for(int i = 0; i < args.Length; i++)
                    sb.Append(ParseObject(args[i]) + " p" + i.ToString());
            }
            sb.Append(")");

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
