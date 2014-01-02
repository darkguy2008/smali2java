using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smali2Java
{
    public class SmaliMethod
    {
        public SmaliClass ParentClass;
        public SmaliVM vm = new SmaliVM();
        public StringBuilder Buffer = new StringBuilder();
        public StringBuilder JavaBuffer = new StringBuilder();

        public List<String> Java = new List<String>();
        public List<SmaliLine> Lines = new List<SmaliLine>();
        public SmaliLine.EAccessMod AccessModifiers;
        public SmaliLine.ENonAccessMod NonAccessModifiers;
        public SmaliLine.LineReturnType ReturnType;
        public List<SmaliParameter> Parameters = new List<SmaliParameter>();
        public String Name;
        public String SmaliReturnType;
        public bool IsClinit = false;
        public bool IsConstructor = false;
        public bool IsFirstParam = true;
        public bool Isp0self = false;

        // MAIN TRANSLATION METHOD, FAKE VIRTUAL MACHINE
        public void Process()
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                SmaliLine l = Lines[i];
                if (l.bIsDirective)
                {
                    #region Directives
                    switch (l.Instruction)
                    {
                        case SmaliLine.LineInstruction.Method:
                            #region Method
                            AccessModifiers = l.AccessModifiers;
                            NonAccessModifiers = l.NonAccessModifiers;
                            Name = l.aName;
                            ReturnType = l.ReturnType;
                            SmaliReturnType = l.aReturnType;
                            IsConstructor = l.IsConstructor;
                            #endregion
                            break;
                        case SmaliLine.LineInstruction.Parameter:
                            #region Parameter
                            if (l.aName != "p0" && IsFirstParam)
                            {
                                IsFirstParam = false;
                                Isp0self = true;
                                Parameters.Add(new SmaliParameter()
                                {
                                    Name = "this",
                                    Register = "p0",
                                    Type = ParentClass.ClassName
                                });
                            }

                            l.aName = char.ToUpper(l.aName[0]) + l.aName.Substring(1);
                            Parameters.Add(new SmaliParameter()
                            {
                                Name = "param" + l.aName,
                                Register = l.lRegisters.Keys.First(),
                                Type = l.aType
                            });
                            #endregion
                            break;
                        case SmaliLine.LineInstruction.Prologue:
                            #region Prologue (method declaration)
                            StringBuilder sb = new StringBuilder();

                            if (Name == "<clinit>")
                                IsClinit = true;

                            if (IsClinit)
                            {
                                Name = "";
                                ReturnType = SmaliLine.LineReturnType.Custom;
                                SmaliReturnType = "";
                            }
                            else if (Name == "<init>")
                            {
                                Name = ParentClass.ClassName.Replace(";", "");
                                SmaliReturnType = "";
                                ReturnType = SmaliLine.LineReturnType.Custom;
                            }
                            
                            sb.AppendFormat("{0} {1} {2} {3}",
                                AccessModifiers == 0 ? "" : AccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                                NonAccessModifiers == 0 ? "" : NonAccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                                ReturnType == SmaliLine.LineReturnType.Custom ? SmaliUtils.General.Name2Java(SmaliReturnType) : ReturnType.ToString().ToLowerInvariant(),
                                Name
                            );

                            if (!IsClinit)
                                sb.Append(" (");

                            if (Parameters.Count > 0)
                            {
                                for(int j = Isp0self ? 1 : 0; j < Parameters.Count; j++)
                                    sb.Append(Parameters[j].ToJava() + ", ");                                    
                                sb.Remove(sb.Length - 2, 2);

                                if (Isp0self)
                                    vm.Put("p0", "this");
                            }

                            if(!IsClinit)
                                sb.Append(") ");

                            sb.Append("{");

                            Buffer.Append(sb.ToString());
                            #endregion
                            break;
                        case SmaliLine.LineInstruction.EndMethod:
                            Java.Add("}");
                            break;
                        case SmaliLine.LineInstruction.Line:
                            if (!String.IsNullOrEmpty(Buffer.ToString()))
                            {
                                JavaBuffer.Append(Buffer.ToString());
                                if (!String.IsNullOrEmpty(JavaBuffer.ToString()))
                                {
                                    Java.Add(JavaBuffer.ToString());
                                    Buffer = new StringBuilder();
                                    JavaBuffer = new StringBuilder();
                                }
                            }
                            break;
                    }
                    #endregion
                }
                else
                {
                    #region Smali instructions
                    String sReg;
                    String sSrcValue;
                    String sDstValue;
                    switch (l.Smali)
                    {
                        case SmaliLine.LineSmali.Const4:
                        case SmaliLine.LineSmali.ConstString:
                            vm.Put(l.lRegisters.Keys.First(), l.aValue);
                            break;

                        case SmaliLine.LineSmali.SputObject:
                            sReg = l.lRegisters.Keys.First();

                            if (!vm.vmStack.ContainsKey(sReg))
                            {
                                // SKIP!
                                break;
                            }

                            sSrcValue = vm.Get(sReg);
                            sDstValue = l.lRegisters[sReg];

                            Dictionary<String, String> args = new Dictionary<String, String>();
                            args[sReg] = sSrcValue;

                            Buffer.Append(ParseSmali(sDstValue, args));
                            break;

                        case SmaliLine.LineSmali.InvokeStatic:
                        //case SmaliLine.LineSmali.InvokeDirect:

                            args = new Dictionary<string,string>();
                            foreach (KeyValuePair<String, String> kv in l.lRegisters)
                            {
                                if (!vm.vmStack.ContainsKey(kv.Key))
                                {
                                    // SKIP!
                                    break;
                                }
                                args[kv.Key] = vm.Get(kv.Key);
                            }

                            Buffer.Append(SmaliUtils.General.Name2Java(l.aClassName));
                            Buffer.Append("." + l.aName + "(");

                            if (args.Count > 0)
                            {
                                foreach (KeyValuePair<String, String> kv in args)
                                    Buffer.Append(kv.Value + ", ");
                                Buffer.Remove(Buffer.Length - 2, 2);
                            }

                            Buffer.Append(")");

                            break;

                        case SmaliLine.LineSmali.MoveResultObject:

                            if (Buffer.Length > 0)
                            {
                                sReg = l.lRegisters.Keys.First();
                                vm.Put(sReg, Buffer.ToString());
                                Buffer = new StringBuilder();
                            }

                            break;
                        case SmaliLine.LineSmali.ReturnVoid:                            
                            Buffer.Append("return");
                            break;
                    }
                    #endregion
                }
            }
        }

        public string ParseSmali(string smaliLine, Dictionary<String, String> args)
        {            
            if (smaliLine.Contains('('))
            {
                // It's a function
            }
            else
            {
                // It's an assignment
                String sClass = SmaliUtils.General.Eat(ref smaliLine, "->", true);
                String sName = SmaliUtils.General.Eat(ref smaliLine, ":", true);
                String sType = smaliLine;

                sClass = sClass.Replace(ParentClass.PackageName, "");
                sClass = sClass.Substring(1);

                if (sClass == ParentClass.ClassName)
                    sClass = "";
                else
                    sClass = SmaliUtils.General.Name2Java(sClass) + ".";

                return sClass + sName + " = " + args[args.Keys.First()] + ";";
            }

            return ":D";
        }

        public String ToJava()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in Java)
                sb.AppendLine(s);
            return sb.ToString();
        }
    }
}
