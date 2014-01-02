using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smali2Java
{
    public class SmaliVM
    {
        #region VM Stack (variable holder)
        public Dictionary<String, Object> vmStack = new Dictionary<String, Object>();

        public void Put(String register, String value)
        {
            vmStack[register] = value;
        }

        public String Get(String register)
        {
            return vmStack[register].ToString();
        }
        #endregion

        public Directives smaliDirectives = new Directives();
        public Instructions smaliInstructions = new Instructions();
        public String Java;
        public StringBuilder Buf = new StringBuilder();
        public void FlushBuffer()
        {
            Java = Buf.ToString();
            Buf = new StringBuilder("");
        }

        public void ProcessDirective(SmaliMethod m, SmaliLine l)
        {
            smaliDirectives.m = m;
            smaliDirectives.l = l;
            switch (l.Instruction)
            {
                case SmaliLine.LineInstruction.Method:
                    smaliDirectives.Method();
                    break;
                case SmaliLine.LineInstruction.Parameter:
                    smaliDirectives.Parameter();
                    break;
                case SmaliLine.LineInstruction.Prologue:
                    smaliDirectives.Prologue();
                    break;
                case SmaliLine.LineInstruction.Line:
                    smaliDirectives.Line();
                    break;
                case SmaliLine.LineInstruction.EndMethod:
                    smaliDirectives.EndMethod();
                    break;
            }
        }
        public void ProcessInstruction(SmaliMethod m, SmaliLine l)
        {
            smaliInstructions.m = m;
            smaliInstructions.l = l;
            switch (l.Smali)
            {
                case SmaliLine.LineSmali.Const4:
                case SmaliLine.LineSmali.ConstString:
                    smaliInstructions.Const();
                    break;
            }
        }

        public class Directives
        {
            public SmaliMethod m;
            public SmaliLine l;

            public void Method()
            {
                m.AccessModifiers = l.AccessModifiers;
                m.NonAccessModifiers = l.NonAccessModifiers;
                m.Name = l.aName;
                m.ReturnType = l.ReturnType;
                m.SmaliReturnType = l.aReturnType;
            }
            public void Parameter()
            {
                if (l.aName != "p0" && m.bIsFirstParam)
                {
                    m.bIsFirstParam = false;
                    m.MethodFlags |= SmaliMethod.EMethodFlags.p0IsSelf;
                    m.Parameters.Add(new SmaliParameter()
                    {
                        Name = "this",
                        Register = "p0",
                        Type = m.ParentClass.ClassName
                    });
                }

                l.aName = char.ToUpper(l.aName[0]) + l.aName.Substring(1);
                m.Parameters.Add(new SmaliParameter()
                {
                    Name = "param" + l.aName,
                    Register = l.lRegisters.Keys.First(),
                    Type = l.aType
                });
            }
            public void Prologue()
            {
                if (m.Name == "<clinit>")
                    m.MethodFlags |= SmaliMethod.EMethodFlags.ClassInit;
                if (m.Name == "<init>")
                    m.MethodFlags |= SmaliMethod.EMethodFlags.Constructor;

                if (m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.ClassInit))
                {
                    m.Name = "";
                    m.ReturnType = SmaliLine.LineReturnType.Custom;
                    m.SmaliReturnType = "";
                }
                else if (m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.Constructor))
                {
                    m.Name = m.ParentClass.ClassName.Replace(";", "");
                    m.SmaliReturnType = "";
                    m.ReturnType = SmaliLine.LineReturnType.Custom;
                }

                SmaliEngine.VM.Buf.AppendFormat("{0} {1} {2}",
                    SmaliUtils.General.Modifiers2Java(m.AccessModifiers, m.NonAccessModifiers),
                    m.ReturnType == SmaliLine.LineReturnType.Custom ? SmaliUtils.General.Name2Java(m.SmaliReturnType) : m.ReturnType.ToString().ToLowerInvariant(),
                    m.Name
                );

                if (!m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.ClassInit))
                    SmaliEngine.VM.Buf.Append(" (");
                
                if (m.Parameters.Count > 0)
                {
                    for (int j = m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.p0IsSelf) ? 1 : 0; j < m.Parameters.Count; j++)
                        SmaliEngine.VM.Buf.Append(m.Parameters[j].ToJava() + ", ");
                    SmaliEngine.VM.Buf.Remove(SmaliEngine.VM.Buf.Length - 2, 2);

                    if (m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.p0IsSelf))
                        SmaliEngine.VM.Put("p0", "this");
                }

                if (!m.MethodFlags.HasFlag(SmaliMethod.EMethodFlags.ClassInit))
                    SmaliEngine.VM.Buf.Append(") ");

                SmaliEngine.VM.Buf.Append("{");
                SmaliEngine.VM.FlushBuffer();
            }
            public void Line()
            {
                SmaliEngine.VM.FlushBuffer();
            }
            public void EndMethod()
            {
                SmaliEngine.VM.Buf.Append("}");
                SmaliEngine.VM.FlushBuffer();
            }
        }
        public class Instructions
        {
            public SmaliMethod m;
            public SmaliLine l;

            public void Const()
            {
                SmaliEngine.VM.Put(l.lRegisters.Keys.First(), l.aValue);
            }

        }
    }
}
