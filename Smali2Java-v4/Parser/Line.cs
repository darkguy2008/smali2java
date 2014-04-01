using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java_v4.Parser
{
    public enum ELineType
    {
        Directive = 1,
        Instruction = 2,
        Label = 3,
        Comment = 4
    }

    public enum ELineDirective
    {
        Class = 1,
        Super,
        Source,
        Field,
        Method,
        EndMethod,
        Registers,
        Prologue,
        Line,
        Param,

    }

    public class Line
    {
        public ELineType Type;
        public ELineDirective Directive;
        public List<String> Tokens = new List<String>();
        public bool NewGroup = false;
        public String Raw;

        public Line(string s)
        {
            s = s.Trim();
            if(!String.IsNullOrEmpty(s))
                if (s.Length > 0)
                {
                    Raw = s;

                    switch (s[0])
                    {
                        case '.':
                            Type = ELineType.Directive;
                            break;
                        case '#':
                            Type = ELineType.Comment;
                            break;
                        case ':':
                            Type = ELineType.Label;
                            break;
                        default:
                            Type = ELineType.Instruction;
                            break;
                    }
                    if (s.Contains(' '))
                    {
                        Tokens.AddRange(s.Split(new String[] { " " }, StringSplitOptions.RemoveEmptyEntries));
                        if (Tokens.Count > 0)
                        {
                            switch (Tokens.First().ToLowerInvariant().Trim())
                            {
                                case ".class":
                                    Directive = ELineDirective.Class;
                                    break;
                                case ".super":
                                    Directive = ELineDirective.Super;
                                    break;
                                case ".source":
                                    Directive = ELineDirective.Source;
                                    break;
                                case ".field":
                                    Directive = ELineDirective.Field;
                                    NewGroup = true;
                                    break;
                                case ".method":
                                    Directive = ELineDirective.Method;
                                    NewGroup = true;
                                    break;
                            }
                        }
                    }
                }
        }
    }
}
