using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public int _idxParam = 0;

        public void ProcessDirective(SmaliMethod m, SmaliLine l)
        {
            smaliDirectives.m = m;
            smaliDirectives.l = l;
            switch (l.Instruction)
            {
                case SmaliLine.LineInstruction.Method:
                    smaliDirectives.Method();
                    if (!m.hasParams)
                    {
                        smaliDirectives.noParameter();
                        if (!m.hasProlouge)
                            smaliDirectives.Prologue();
                    }
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
                case SmaliLine.LineSmali.SputObject:
                    smaliInstructions.SputObject();
                    break;
                case SmaliLine.LineSmali.NewInstance:
                    smaliInstructions.NewInstance();
                    break;
                case SmaliLine.LineSmali.InvokeDirect:
                    smaliInstructions.InvokeDirect();
                    break;
                case SmaliLine.LineSmali.IputBoolean:
                    smaliInstructions.IputBoolean();
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
                m.MethodCall = SmaliCall.Parse(l.aExtra);
                SmaliEngine.VM._idxParam = 0;
            }
            public void noParameter()
            {
                m.bIsFirstParam = false;
                m.MethodFlags |= SmaliMethod.EMethodFlags.p0IsSelf;
                m.MethodCall.Parameters.Insert(0, new SmaliParameter()
                {
                    Name = "this",
                    Register = "p0",
                    Type = m.ParentClass.ClassName
                });
                SmaliEngine.VM._idxParam = 1;
                for (; SmaliEngine.VM._idxParam < m.MethodCall.Parameters.Count; SmaliEngine.VM._idxParam++)
                {
                    m.MethodCall.Parameters[SmaliEngine.VM._idxParam].Name =
                        m.MethodCall.Parameters[SmaliEngine.VM._idxParam].Register = "p" + SmaliEngine.VM._idxParam;
                }
            }
            public void Parameter()
            {
                if (l.lRegisters.Keys.First() != "p0" && m.bIsFirstParam)
                {
                    m.bIsFirstParam = false;
                    m.MethodFlags |= SmaliMethod.EMethodFlags.p0IsSelf;
                    m.MethodCall.Parameters.Insert(0, new SmaliParameter()
                    {
                        Name = "this",
                        Register = "p0",
                        Type = m.ParentClass.ClassName
                    });
                    SmaliEngine.VM._idxParam = 1;
                }

                l.aName = char.ToUpper(l.aName[0]) + l.aName.Substring(1);

                // TODO: Check if this algorithm is right?
                m.MethodCall.Parameters[SmaliEngine.VM._idxParam].Name = "param" + l.aName;
                m.MethodCall.Parameters[SmaliEngine.VM._idxParam].Register = l.lRegisters.Keys.First();
                SmaliEngine.VM._idxParam++;
            }
            public void Prologue()
            {
                // TODO: Create extension method HasFlag because .NET 3.5 doesn't have it?
                if((m.MethodCall.CallFlags & SmaliCall.ECallFlags.Constructor) == SmaliCall.ECallFlags.Constructor)
                    m.MethodCall.Method = m.ParentClass.ClassName.Replace(";", "");

                SmaliEngine.VM.Buf.AppendFormat("{0} {1}{2}",
                    SmaliUtils.General.Modifiers2Java(m.AccessModifiers, m.NonAccessModifiers),
                    SmaliUtils.General.ReturnType2Java(m.MethodCall.SmaliReturnType, m.MethodCall.Return),
                    m.MethodCall.Method                    
                );

                if (((m.MethodCall.CallFlags & SmaliCall.ECallFlags.ClassInit) == SmaliCall.ECallFlags.ClassInit) == false)
                    SmaliEngine.VM.Buf.Append(" (");
                
                if (m.MethodCall.Parameters.Count > 0)
                {
                    // Let's save a couple of cycles and only calculate this once in this if block.
                    bool p0isSelf = (m.MethodFlags & SmaliMethod.EMethodFlags.p0IsSelf) == SmaliMethod.EMethodFlags.p0IsSelf;

                    for (int j = p0isSelf ? 1 : 0; j < m.MethodCall.Parameters.Count; j++)
                        SmaliEngine.VM.Buf.Append(m.MethodCall.Parameters[j].ToJava() + ", ");

                    // Only remove things from the buffer if we printed any args to remove.
                    if (m.MethodCall.Parameters.Count > (p0isSelf ? 1 : 0) )
                        SmaliEngine.VM.Buf.Remove(SmaliEngine.VM.Buf.Length - 2, 2);

                    if (p0isSelf)
                        SmaliEngine.VM.Put("p0", "this");
                }

                if (((m.MethodCall.CallFlags & SmaliCall.ECallFlags.ClassInit) == SmaliCall.ECallFlags.ClassInit) == false)
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
            public void SputObject()
            {
                String sReg = l.lRegisters.Keys.First();

                // SKIP! TODO: Should not skip, actually. If it skips, something IS wrong
                if (!SmaliEngine.VM.vmStack.ContainsKey(sReg))
                    return;

                String sSrcValue = SmaliEngine.VM.Get(sReg);
                String sDstValue = l.lRegisters[sReg];

                Dictionary<String, String> args = new Dictionary<String, String>();
                args[sReg] = sSrcValue;

                SmaliCall c = SmaliCall.Parse(sDstValue);

                SmaliEngine.VM.Buf = new StringBuilder();

                SmaliEngine.VM.Buf.AppendFormat("{0}{1}{2} = {3};\n",
                    c.Variable,
                    m.ParentClass.PackageName == c.ClassName ? "" : (c.ClassName + "."),
                    m.ParentClass.ClassName == c.Method ? "" : (c.Method + "."),
                    sSrcValue
                );

                //TODO: Well... todo. Lol.
                //Buffer.Append(ParseSmali(sDstValue, args));
            }
            public void IputBoolean()
            {
                String sReg = l.lRegisters.Keys.First();

                // SKIP! TODO: Should not skip, actually. If it skips, something IS wrong
                if (!SmaliEngine.VM.vmStack.ContainsKey(sReg))
                    return;

                String sSrcValue = SmaliEngine.VM.Get(sReg);
                String sDstValue = l.aName;

                Dictionary<String, String> args = new Dictionary<String, String>();
                args[sReg] = sSrcValue;

                SmaliCall c = SmaliCall.Parse(sDstValue);

                SmaliEngine.VM.Buf = new StringBuilder();

                SmaliEngine.VM.Buf.AppendFormat("{0}{1} = {2};\n",
                    (m.ParentClass.PackageName == c.ClassName && m.ParentClass.ClassName == c.Method ? 
                        "this." : 
                        (SmaliUtils.General.Name2Java(c.ClassName) + "." + c.Method + ".")),
                    c.Variable,
                    (sSrcValue == "0x1" ? "true" : "false")
                );

                //TODO: Well... todo. Lol.
                //Buffer.Append(ParseSmali(sDstValue, args));
            }

            public void NewInstance()
            {
                SmaliCall c = SmaliCall.Parse(l.lRegisters[l.lRegisters.Keys.First()]);
                StringBuilder sb = new StringBuilder();
                sb.Append("new " + SmaliUtils.General.Name2Java(c.ClassName));
                sb.Append("." + c.Method + "()");
                SmaliEngine.VM.Put(l.lRegisters.Keys.First(), sb.ToString());
            }
            public void InvokeDirect()
            {
                String sReg = l.lRegisters.Keys.First();
                // SKIP! TODO: Should not skip, actually. If it skips, something IS wrong
                if (!SmaliEngine.VM.vmStack.ContainsKey(sReg))
                    return;

                SmaliCall c = SmaliCall.Parse(l.lRegisters[l.lRegisters.Keys.First()]);
                
                // It's a constructor, skip method name
                if ((c.CallFlags & SmaliCall.ECallFlags.Constructor) == SmaliCall.ECallFlags.Constructor)
                    SmaliEngine.VM.Buf.Append(SmaliEngine.VM.Get(sReg));

                // TODO: I think this needs a bit more work :/
            }
        }
    }
}
