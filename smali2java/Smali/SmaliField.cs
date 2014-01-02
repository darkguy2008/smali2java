using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smali2Java
{
    public class SmaliField
    {
        public SmaliLine.EAccessMod AccessModifiers;
        public SmaliLine.ENonAccessMod NonAccessModifiers;
        public String Name;
        public String Type;

        public String ToJava()
        {
            return String.Format("{0} {1} {2} {3};\n",
                AccessModifiers == 0 ? "" : AccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                NonAccessModifiers == 0 ? "" : NonAccessModifiers.ToString().ToLowerInvariant().Replace(",", ""),
                SmaliUtils.General.Name2Java(Type),
                Name
            );
        }
    }
}
