using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public static class SmaliUtils
    {
        public static class General
        {
            public static String Eat(ref String s, String until, bool addLength = false)
            {
                String rv = s.Substring(0, s.IndexOf(until));
                s = s.Substring(rv.Length + (addLength ? until.Length : 0));
                return rv;
            }

            public static String Eat(ref String s, char until, bool addLength = false)
            {
                String rv = s.Substring(0, s.IndexOf(until));
                s = s.Substring(rv.Length + (addLength ? 1 : 0));
                return rv;
            }

            public static String Name2Java(String smaliName)
            {
                if (smaliName.StartsWith("L"))
                    smaliName = smaliName.Substring(1);
                if (smaliName.EndsWith(";"))
                    smaliName = smaliName.Remove(smaliName.Length - 1, 1);
                smaliName = smaliName.Replace("/", ".");
                return smaliName;
            }

            public static String Modifiers2Java(SmaliLine.EAccessMod eAccessMod, SmaliLine.ENonAccessMod eNonAccessMod)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(eAccessMod == 0 ? "" : eAccessMod.ToString().ToLowerInvariant().Replace(",", ""));
                if (eAccessMod !=0 && eNonAccessMod !=0) 
                    sb.Append(" ");
                sb.Append(eNonAccessMod == 0 ? "" : eNonAccessMod.ToString().ToLowerInvariant().Replace(",", ""));
                return sb.ToString();
            }

            public static String ReturnType2Java(SmaliLine.LineReturnType rt, String customType)
            {
                if (rt.ToString().EndsWith("Array"))
                {
                    if (rt == SmaliLine.LineReturnType.CustomArray)
                    {
                        if (customType != "")
                            return customType.Substring(1) + "[] ";
                        else
                            return customType.Substring(1) + "[]";
                    }
                    else
                        return Name2Java(rt.ToString().Replace("Array","").ToLowerInvariant().Trim()) + "[] ";
                }
                else
                {
                    if (rt == SmaliLine.LineReturnType.Custom)
                    {
                        if (customType != "")
                            return customType + ' ';
                        else
                            return customType;
                    }
                    else
                        return Name2Java(rt.ToString().ToLowerInvariant().Trim()) + ' ';
                }
            }

            public static SmaliLine.LineReturnType GetReturnType(String s)
            {
                String rt = s.ToLowerInvariant().Trim();
                if (rt.StartsWith("[")) // This is an array
                {
                    rt = rt.Substring(1);
                    switch (rt)
                    {
                        case "v":
                            return SmaliLine.LineReturnType.VoidArray;
                        case "i":
                            return SmaliLine.LineReturnType.IntArray;
                        case "z":
                            return SmaliLine.LineReturnType.BooleanArray;
                        case "b":
                            return SmaliLine.LineReturnType.ByteArray;
                        case "s":
                            return SmaliLine.LineReturnType.ShortArray;
                        case "c":
                            return SmaliLine.LineReturnType.CharArray;
                        case "j":
                            return SmaliLine.LineReturnType.LongArray;
                        case "d":
                            return SmaliLine.LineReturnType.DoubleArray;
                        default:
                            return SmaliLine.LineReturnType.CustomArray;
                    }    
                }
                switch(rt)
                {
                    case "v":
                        return SmaliLine.LineReturnType.Void;
                    case "i":
                        return SmaliLine.LineReturnType.Int;
                    case "z":
                        return SmaliLine.LineReturnType.Boolean;
                    case "b":
                        return SmaliLine.LineReturnType.Byte;
                    case "s":
                        return SmaliLine.LineReturnType.Short;
                    case "c":
                        return SmaliLine.LineReturnType.Char;
                    case "j":
                        return SmaliLine.LineReturnType.Long;
                    case "d":
                        return SmaliLine.LineReturnType.Double;
                    default: 
                        return SmaliLine.LineReturnType.Custom;
                }                
            }
        }
    }
}
