using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smali2Java
{
    public class SmaliLine
    {
        #region Definitions

        [Flags]
        public enum EAccessMod
        {
            Private = 1,
            Public = 2,
            Protected = 4
        }

        [Flags]
        public enum ENonAccessMod
        {
            Static = 1,
            Final = 2,
            Abstract = 4,
            Synchronized = 8,
            Volatile = 16,
        }

        public static String[] Keywords = new String[] 
        {
            "public",
            "private",
            "protected",
            "static",
            "final",
            "abstract",
            "synchronized",
            "volatile",
            "constructor"
        };

        public enum LineInstruction
        {
            Unknown = -1,
            Class = 1,
            Super,
            Implements,
            Source,
            Field,
            Method,
            Registers,
            Prologue,
            Line,
            EndMethod,
            Catch,
            Parameter
        }

        public enum LineBlock
        {
            None = 1,
            TryStart,
            TryCatch,
            TryEnd,
            IfStart,
            IfEnd
        }

        public enum LineSmali
        {
            Unknown = -1,
            Const4 = 1,
            ConstString,
            SputObject,
            IputObject,
            InvokeStatic,
            InvokeDirect,
            MoveResultObject,
            NewInstance,
            ReturnVoid,
        }

        public enum LineReturnType
        {
            Custom = 1,
            Void,
            Int,
        }
        #endregion

        // Flags & identifiers
        public LineInstruction Instruction;
        public LineBlock Block;
        public LineSmali Smali;
        public EAccessMod AccessModifiers;
        public ENonAccessMod NonAccessModifiers;
        public LineReturnType ReturnType;

        // Simple flags
        public bool bIsDirective = false;
        public bool IsConstructor = false;
        public bool IsDestructor = false;
        
        // Attributes
        public String aType;
        public String aExtra;
        public String aName;
        public String aClassName;
        public String aReturnType;
        public String aValue;
        public Dictionary<String, String> lRegisters = new Dictionary<String, String>();

        public static SmaliLine Parse(String l)
        {
            SmaliLine rv = new SmaliLine();

            l = l.Trim();
            if (l.Length == 0)
                return null;

            if (l[0] == '#')
                return null;

            String sIdentifiers = l;
            String sRawText = String.Empty;
            bool bHasString = l.Contains('"');

            if (bHasString)
                sIdentifiers = SmaliUtils.General.Eat(ref l, '"', false);
            sRawText = l;

            String[] sWords = sIdentifiers.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (sWords.Length < 0)
                return null;

            String sInst = sWords[0].ToLowerInvariant().Trim();
            sWords[0] = String.Empty;

            if (sInst[0] == '.')
                if (!ParseAsDirective(rv, sInst, ref sWords, ref sRawText))
                    return null;
                else
                    rv.bIsDirective = true;

            if(sInst[0] != '.')
                if (!ParseAsInstruction(rv, sInst, ref sWords, ref sRawText))
                    return null;

            return rv;
        }

        #region Directive Parsing

        public static bool ParseAsDirective(SmaliLine rv, String sInst, ref String[] sWords, ref String sRawText)
        {
            switch (sInst)
            {
                case ".class":
                    rv.Instruction = LineInstruction.Class;
                    SetModifiers(rv, ref sWords, 1, sWords.Length - 1);
                    rv.aClassName = sWords[sWords.Length - 1];
                    break;
                case ".super":
                    rv.Instruction = LineInstruction.Super;
                    rv.aType = sWords[1];
                    break;
                case ".implements":
                    rv.Instruction = LineInstruction.Implements;
                    rv.aType = sWords[1];
                    break;
                case ".source":
                    rv.Instruction = LineInstruction.Source;
                    rv.aExtra = sRawText;
                    break;
                case ".field":
                    rv.Instruction = LineInstruction.Field;
                    SetModifiers(rv, ref sWords, 1, sWords.Length - 1);
                    sRawText = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    rv.aName = sRawText.Split(':')[0];
                    rv.aType = sRawText.Split(':')[1];
                    break;
                case ".method":
                    rv.Instruction = LineInstruction.Method;
                    SetModifiers(rv, ref sWords, 1, sWords.Length - 1);
                    sRawText = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    rv.aExtra = sRawText;
                    break;
                case ".prologue":
                    rv.Instruction = LineInstruction.Prologue;
                    break;
                case ".registers":
                    rv.Instruction = LineInstruction.Registers;
                    break;
                case ".line":
                    rv.Instruction = LineInstruction.Line;
                    break;
                case ".end":
                    switch (sRawText)
                    {
                        case ".end method":
                            rv.Instruction = LineInstruction.EndMethod;
                            break;
                    }
                    break;
                case ".param":
                    rv.Instruction = LineInstruction.Parameter;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1].Trim()] = String.Empty;
                    rv.aName = sRawText.Substring(sRawText.IndexOf('"') + 1);
                    rv.aName = rv.aName.Substring(0, rv.aName.IndexOf('"'));
                    rv.aType = sRawText.Substring(sRawText.IndexOf('#') + 1).Trim();
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static void SetModifiers(SmaliLine l, ref String[] ar, int start, int end)
        {
            for (int i = start; i < end; i++)
                if (Keywords.Contains(ar[i].ToLowerInvariant().Trim()))
                {
                    if (!ParseNonAccess(l, ar[i].ToLowerInvariant().Trim()))
                        if (!ParseAccess(l, ar[i].ToLowerInvariant().Trim()))
                            ParseModifier(l, ar[i].ToLowerInvariant().Trim());
                    ar[i] = String.Empty;
                }
        }
        public static bool ParseNonAccess(SmaliLine l, String s)
        {
            switch (s)
            {
                case "static":
                    l.NonAccessModifiers |= ENonAccessMod.Static;
                    break;
                case "final":
                    l.NonAccessModifiers |= ENonAccessMod.Final;
                    break;
                case "abstract":
                    l.NonAccessModifiers |= ENonAccessMod.Abstract;
                    break;
                case "synchronized":
                    l.NonAccessModifiers |= ENonAccessMod.Synchronized;
                    break;
                case "volatile":
                    l.NonAccessModifiers |= ENonAccessMod.Volatile;
                    break;
                default:
                    return false;
            }
            return true;
        }
        public static bool ParseAccess(SmaliLine l, String s)
        {
            switch (s)
            {
                case "public":
                    l.AccessModifiers |= EAccessMod.Public;
                    break;
                case "private":
                    l.AccessModifiers |= EAccessMod.Private;
                    break;
                case "protected":
                    l.AccessModifiers |= EAccessMod.Protected;
                    break;
                default:
                    return false;
            }
            return true;
        }
        public static bool ParseModifier(SmaliLine l, String s)
        {
            switch (s)
            {
                case "constructor":
                    l.IsConstructor = true;
                    break;
                default:
                    return false;
            }
            return true;
        }
        #endregion
        #region Instruction Parsing

        public static bool ParseAsInstruction(SmaliLine rv, String sInst, ref String[] sWords, ref String sRawText)
        {
            switch (sInst)
            {
                case "const/4":
                    rv.Smali = LineSmali.Const4;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1]] = String.Empty;
                    rv.aValue = sWords[2];
                    break;
                case "const-string":
                    rv.Smali = LineSmali.ConstString;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1]] = String.Empty;
                    rv.aValue = sRawText;
                    break;
                case "invoke-static":
                    rv.Smali = LineSmali.InvokeStatic;
                    // TODO: What?
                    //ParseInvocation(rv, ref sWords);
                    break;
                case "invoke-direct":
                    rv.Smali = LineSmali.InvokeDirect;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
                    break;
                case "move-result-object":
                    rv.Smali = LineSmali.MoveResultObject;
                    rv.lRegisters[sWords[1]] = String.Empty;
                    break;
                case "return-void":
                    rv.Smali = LineSmali.ReturnVoid;
                    break;  
                case "sput-object":
                    rv.Smali = LineSmali.SputObject;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
                    break;
                case "new-instance":
                    rv.Smali = LineSmali.NewInstance;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
                    break;
                case "iput-object":
                    rv.Smali = LineSmali.IputObject;
                    rv.aValue = sWords[sWords.Length - 1];
                    sWords[sWords.Length - 1] = String.Empty;
                    String sp = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    if (sp.EndsWith(","))
                        sp = sp.Substring(0, sp.Length - 1);
                    ParseParameters(rv, sp);
                    break;
            }
            return true;
        }

        public static void ParseParameters(SmaliLine rv, String s)
        {
            if (s.EndsWith(","))
                s = s.Substring(0, s.Length - 1);

            if (s.Contains('{'))
                s = s.Substring(1, s.Length - 2);

            if(s.Contains(','))
            {
                String[] sp = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String p in sp)
                    rv.lRegisters[p.Trim()] = String.Empty;
            }
            else
                rv.lRegisters[s] = String.Empty;
        }

        #endregion

    }
}
