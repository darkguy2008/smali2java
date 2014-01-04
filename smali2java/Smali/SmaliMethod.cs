using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public class SmaliMethod
    {
        [Flags]
        public enum EMethodFlags
        {
            p0IsSelf = 4
        }

        public List<String> JavaOutput = new List<String>();
        public List<SmaliLine> Lines = new List<SmaliLine>();

        public SmaliClass ParentClass;
        public SmaliLine.EAccessMod AccessModifiers;
        public SmaliLine.ENonAccessMod NonAccessModifiers;
        public EMethodFlags MethodFlags = 0;
        public SmaliCall MethodCall;

        public bool bIsFirstParam = true;
        public bool bHasParameters= true;
        public bool bHasPrologue = true;
        public void Process()
        {
            bHasPrologue = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Prologue).Count() > 0;
            bHasParameters = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Parameter).Count() > 0;
            for (int i = 0; i < Lines.Count; i++)
            {
                SmaliLine l = Lines[i];
                if (l.bIsDirective)
                    SmaliEngine.VM.ProcessDirective(this, l);
                else
                    SmaliEngine.VM.ProcessInstruction(this, l);

                if (!String.IsNullOrEmpty(SmaliEngine.VM.Java))
                {
                    JavaOutput.Add(SmaliEngine.VM.Java);
                    SmaliEngine.VM.Java = String.Empty;
                }
            }
        }

        public String ToJava()
        {
            return (String.Join("\n", JavaOutput.ToArray()) + "\n");
        }
    }
}
