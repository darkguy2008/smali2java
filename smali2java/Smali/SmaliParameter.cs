using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public class SmaliParameter
    {
        public String Register;
        public String Name;
        public String Type;
        public String Value;

        public String ToJava()
        {
            return SmaliUtils.General.Name2Java(Type) + " " + Name;
        }
    }
}
