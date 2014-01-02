using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public class SmaliClass
    {
        public List<SmaliLine> Lines;

        public String PackageName;
        public String ClassName;
        public String SourceFile;
        public String Extends;
        public String Implements;
        public SmaliLine.EAccessMod AccessModifiers;
        public SmaliLine.ENonAccessMod NonAccessModifiers;
        public List<SmaliField> Fields = new List<SmaliField>();
        public List<SmaliMethod> Methods = new List<SmaliMethod>();

        public void LoadAttributes()
        {
            SmaliLine line = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Class).Single();
            ClassName = line.aClassName.Substring(line.aClassName.LastIndexOf('/') + 1).Replace(";", "");
            AccessModifiers = line.AccessModifiers;
            NonAccessModifiers = line.NonAccessModifiers;
            PackageName = line.aClassName.Substring(0, line.aClassName.LastIndexOf('/'));

            line = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Super).SingleOrDefault();
            if (line != null)
                Extends = line.aType;

            line = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Implements).SingleOrDefault();
            if (line != null)
                Implements = line.aType;

            line = Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Source).Single();
            SourceFile = line.aExtra;
        }
        public void LoadFields()
        {
            foreach (SmaliLine l in Lines.Where(x => x.Instruction == SmaliLine.LineInstruction.Field))
                Fields.Add(new SmaliField()
                {
                    AccessModifiers = l.AccessModifiers,
                    NonAccessModifiers = l.NonAccessModifiers,
                    Type = l.aType,
                    Name = l.aName
                });
        }
        public void LoadMethods()
        {
            bool bAdd = false;
            SmaliMethod m = null;

            for (int i = 0; i < Lines.Count; i++)
            {
                SmaliLine l = Lines[i];

                if (bAdd)
                {
                    m.Lines.Add(l);
                    if (l.Instruction == SmaliLine.LineInstruction.EndMethod)
                    {
                        bAdd = false;
                        //if(Methods.Count < 3)
                            Methods.Add(m);
                    }
                    else
                        continue;
                }

                if (l.Instruction == SmaliLine.LineInstruction.Method)
                {
                    bAdd = true;
                    m = new SmaliMethod();
                    m.ParentClass = this;
                    m.Lines.Add(l);
                }
            }

            foreach (SmaliMethod me in Methods)
                me.Process();
        }

        public String ToJava()
        {
            String rv = String.Empty;
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("package {0};\n\n", SmaliUtils.General.Name2Java(PackageName));

            sb.AppendFormat("{0} {1} class {2} {3} {4} {5} {6} {{\n",
                AccessModifiers == 0 ? "" : AccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                NonAccessModifiers == 0 ? "" : NonAccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                SmaliUtils.General.Name2Java(ClassName),
                String.IsNullOrEmpty(Extends) ? "" : "extends",
                String.IsNullOrEmpty(Extends) ? "" : SmaliUtils.General.Name2Java(Extends),
                String.IsNullOrEmpty(Implements) ? "" : "implements",
                String.IsNullOrEmpty(Implements) ? "" : SmaliUtils.General.Name2Java(Implements)
            );

            foreach (SmaliField f in Fields)
                sb.Append(f.ToJava());

            foreach (SmaliMethod m in Methods)
                sb.Append(m.ToJava());

            sb.AppendLine("}");

            rv = sb.ToString();
            return rv;
        }
    }
}
