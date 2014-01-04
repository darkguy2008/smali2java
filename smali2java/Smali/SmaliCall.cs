using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public class SmaliCall
    {
        [Flags]
        public enum ECallFlags
        {
            ClassInit = 1,
            Constructor = 2
        }

        public String ClassName;
        public String Method;
        public String Variable;
        public String Return;
        public List<SmaliParameter> Parameters = new List<SmaliParameter>();
        public SmaliLine.LineReturnType SmaliReturnType;
        public ECallFlags CallFlags;

        public static SmaliCall Parse(String l)
        {
            SmaliCall rv = new SmaliCall();
            rv.Method = l;

            if (l.Contains("("))
            {
                // Function call
                rv.Method = rv.Method.Substring(0, rv.Method.IndexOf("("));
                rv.Return = l.Substring(l.LastIndexOf(")") + 1);
                if (rv.Return.Length > 0)
                    rv.SmaliReturnType = SmaliUtils.General.GetReturnType(rv.Return);

                if(rv.Method.Contains("->"))
                {
                    rv.ClassName = rv.Method.Substring(0, rv.Method.IndexOf(";"));
                    rv.Method = rv.Method.Substring(rv.Method.IndexOf("->") + 2);
                }
            }
            else
            {
                if (l.Contains("/"))
                {
                    rv.ClassName = l;
                    if (l.Contains("->"))
                        rv.ClassName = l.Substring(0, l.IndexOf("->"));

                    // Get class name out
                    rv.ClassName = rv.ClassName.Substring(0, rv.ClassName.LastIndexOf('/'));
                    rv.Method = l.Substring(rv.ClassName.Length + 1);
                    if (l.Contains(":"))
                    {
                        rv.Variable = rv.Method.Substring(rv.Method.IndexOf("->") + 2);
                        rv.Variable = rv.Variable.Substring(0, rv.Variable.IndexOf(":"));
                    }
                    rv.Method = rv.Method.Substring(0, rv.Method.IndexOf(";"));
                }
            }

            if (rv.Method.EndsWith(";"))
                rv.Method = rv.Method.Remove(rv.Method.Length - 1, 1);

            if(l.Contains("("))
            {
                // Get parameters
                String sParameters = l.Substring(l.IndexOf("(") + 1);
                sParameters = sParameters.Substring(0, sParameters.IndexOf(")"));
                if (sParameters.Length > 0)
                {
                    String[] aParm = sParameters.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (String p in aParm)
                        if (p.StartsWith("L"))
                        {
                            rv.Parameters.Add(new SmaliParameter()
                            {
                                Type = p,
                            });
                        }
                        else
                        {
                            for (int i = 0 ; i < p.Length; i++)
                            {
                                if (!p[i].Equals('L'))
                                {
                                    rv.Parameters.Add(new SmaliParameter()
                                    {
                                        Type = String.Empty + p[i],
                                    });
                                }
                                else
                                {
                                    rv.Parameters.Add(new SmaliParameter()
                                    {
                                        Type = p.Substring(i),
                                    });
                                    break;
                                }
                            }
                        }
                }
            }

            // Set flags
            if (rv.Method == "<clinit>")
            {
                rv.Method = String.Empty;
                rv.Return = String.Empty;
                rv.SmaliReturnType = SmaliLine.LineReturnType.Custom;
                rv.CallFlags |= ECallFlags.ClassInit;
            }
            if (rv.Method == "<init>")
            {
                rv.Return = String.Empty;
                rv.SmaliReturnType = SmaliLine.LineReturnType.Custom;
                rv.CallFlags |= ECallFlags.Constructor;
            }

            return rv;
        }
    }
}
