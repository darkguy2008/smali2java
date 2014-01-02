using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smali2Java
{
    public class SmaliMethod
    {
        public enum EMethodFlags
        {
            ClassInit = 1,
            Constructor = 2,
            p0IsSelf = 4
        }

        public List<String> JavaOutput = new List<String>();
        public List<SmaliLine> Lines = new List<SmaliLine>();

        public SmaliClass ParentClass;
        public SmaliLine.EAccessMod AccessModifiers;
        public SmaliLine.ENonAccessMod NonAccessModifiers;
        public SmaliLine.LineReturnType ReturnType;
        public EMethodFlags MethodFlags = 0;
        public List<SmaliParameter> Parameters = new List<SmaliParameter>();
        public String Name;
        public String SmaliReturnType;

        public bool bIsFirstParam = true;

        public void Process()
        {
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
            return (String.Join("\n", JavaOutput) + "\n");
        }
    }
}
