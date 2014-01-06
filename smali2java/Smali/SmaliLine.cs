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
            Const,
            Const16,
            ConstHigh16,
            ConstString,
            SputObject,
            SgetObject,
            IputObject,
            IputBoolean,
            InvokeStatic,
            InvokeDirect,
            InvokeVirtual,
            MoveResultObject,
            MoveResult,
            NewInstance,
            ReturnVoid,
            Return,
            Unimplemented,
            Conditional,
            Label
        }

        public enum LineReturnType
        {
            Custom = 1,
            Void,
            Int,
            Boolean,
            Byte,
            Short,
            Char,
            Long,
            Double,
            CustomArray,
            VoidArray,
            IntArray,
            BooleanArray,
            ByteArray,
            ShortArray,
            CharArray,
            LongArray,
            DoubleArray
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

            if (sWords.Length == 0)
                return null;

            String sInst = sWords[0].ToLowerInvariant().Trim();
            sWords[0] = String.Empty;

            if (sInst[0] == '.')
                if (!ParseAsDirective(rv, sInst, ref sWords, ref sRawText))
                    return null;
                else
                    rv.bIsDirective = true;

            else if (sRawText[0] == ':') //We'll go with the idea that a label is still an instruction, but there is no need to waste cycles in the instruction parsing loop.
            {
                rv.Smali = LineSmali.Label; //TODO: add ParseAsLabel
                rv.aName = sRawText;
            }

            else if (sInst[0] != '.')
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
                case "const":
                    rv.Smali = LineSmali.Const;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1]] = String.Empty;
                    rv.aValue = sWords[2];
                    break;
                case "const/16":
                    rv.Smali = LineSmali.Const16;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1]] = String.Empty;
                    rv.aValue = sWords[2];
                    break;
                case "const/high16":
                    rv.Smali = LineSmali.ConstHigh16;
                    sWords[1] = sWords[1].Replace(",", "");
                    rv.lRegisters[sWords[1]] = String.Empty;
                    rv.aValue = sWords[2];
                    break;
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
                    rv.aName = sWords[sWords.Length - 1];
                    sWords[sWords.Length - 1] = String.Empty;
                    String sp = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    if (sp.EndsWith(","))
                        sp = sp.Substring(0, sp.Length - 1);
                    ParseParameters(rv, sp);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = rv.aName;
                    break;
                case "invoke-direct":
                    rv.Smali = LineSmali.InvokeDirect;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
                    rv.aName = sWords[sWords.Length - 1];
                    break;
                case "move-result-object":
                    rv.Smali = LineSmali.MoveResultObject;
                    rv.lRegisters[sWords[1]] = String.Empty;
                    break;
                case "move-result":
                    rv.Smali = LineSmali.MoveResult;
                    rv.lRegisters[sWords[1]] = String.Empty;
                    break;
                case "return":
                case "return-object":
                    rv.Smali = LineSmali.Return;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[1];
                    break;
                case "return-void":
                    rv.Smali = LineSmali.ReturnVoid;
                    break;
                case "sget-object":
                    rv.Smali = LineSmali.SgetObject;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
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
                    sp = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    if (sp.EndsWith(","))
                        sp = sp.Substring(0, sp.Length - 1);
                    ParseParameters(rv, sp);
                    break;
                case "iput-boolean":
                    rv.Smali = LineSmali.IputBoolean;
                    if (sWords[1].EndsWith(","))
                        sWords[1] = sWords[1].Substring(0, sWords[1].Length - 1);
                    ParseParameters(rv, sWords[1]);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = sWords[2];
                    rv.aName = sWords[ sWords.Length - 1]; //Always the last entry.
                    break;
                case "invoke-virtual":
                    rv.Smali = LineSmali.InvokeVirtual;
                    rv.aName = sWords[sWords.Length - 1];
                    sWords[sWords.Length - 1] = String.Empty;
                    sp = String.Join(" ", sWords.Where(x => !String.IsNullOrEmpty(x)).ToArray()).Trim();
                    if (sp.EndsWith(","))
                        sp = sp.Substring(0, sp.Length - 1);
                    ParseParameters(rv, sp);
                    rv.lRegisters[rv.lRegisters.Keys.First()] = rv.aName;
                    break;

                #region Unimplemented Functions

                case "add-double":
                case "add-double/2addr":
                case "add-float":
                case "add-float/2addr":
                case "add-int":
                case "add-int/2addr":
                case "add-int/lit16":
                case "add-int/lit8":
                case "add-long":
                case "add-long/2addr":
                case "aget":
                case "aget-boolean":
                case "aget-byte":
                case "aget-char":
                case "aget-object":
                case "aget-short":
                case "aget-wide":
                case "and-int":
                case "and-int/2addr":
                case "and-int/lit16":
                case "and-int/lit8":
                case "and-long":
                case "and-long/2addr":
                case "aput":
                case "aput-boolean":
                case "aput-byte":
                case "aput-char":
                case "aput-object":
                case "aput-short":
                case "aput-wide":
                case "array-length":
                case "check-cast":
                case "cmpg-double":
                case "cmpg-float":
                case "cmpl-double":
                case "cmpl-float":
                case "cmp-long":
                case "const-class":
                case "const-string/jumbo":
                case "const-wide":
                case "const-wide/16":
                case "const-wide/32":
                case "const-wide/high16":
                case "div-double":
                case "div-float":
                case "div-float/2addr":
                case "div-int":
                case "div-int/2addr":
                case "div-int/lit16":
                case "div-int/lit8":
                case "div-long":
                case "div-long/2addr":
                case "double-to-int":
                case "double-to-long":
                case "execute-inline":
                case "execute-inline/range":
                case "fill-array-data":
                case "filled-new-array":
                case "filled-new-array/range":
                case "float-to-double":
                case "float-to-int":
                case "float-to-long":
                case "iget":
                case "iget-boolean":
                case "iget-byte":
                case "iget-char":
                case "iget-object":
                case "iget-object-quick":
                case "iget-object-volatile":
                case "iget-quick":
                case "iget-short":
                case "iget-volatile":
                case "iget-wide":
                case "iget-wide-quick":
                case "iget-wide-volatile":
                case "instance-of":
                case "int-to-double":
                case "int-to-float":
                case "int-to-long":
                case "invoke-direct/range":
                case "invoke-direct-empty":
                case "invoke-interface":
                case "invoke-interface/range":
                case "invoke-object-init/range":
                case "invoke-static/range":
                case "invoke-super":
                case "invoke-super/range":
                case "invoke-super-quick":
                case "invoke-super-quick/range":
                case "invoke-virtual/range":
                case "invoke-virtual-quick":
                case "invoke-virtual-quick/range":
                case "iput":
                case "iput-byte":
                case "iput-char":
                case "iput-object-quick":
                case "iput-object-volatile":
                case "iput-quick":
                case "iput-short":
                case "iput-volatile":
                case "iput-wide":
                case "iput-wide-quick":
                case "iput-wide-volatile":
                case "long-to-double":
                case "long-to-float":
                case "long-to-int":
                case "move":
                case "move/16":
                case "move/from16":
                case "move-exception":
                case "move-object":
                case "move-object/16":
                case "move-object/from16":
                case "move-result-wide":
                case "move-wide":
                case "move-wide/16":
                case "move-wide/from16":
                case "mul-double":
                case "mul-float":
                case "mul-float/2addr":
                case "mul-int":
                case "mul-int/2addr":
                case "mul-int/lit16":
                case "mul-int/lit8":
                case "mul-long":
                case "mul-long/2addr":
                case "neg-double":
                case "neg-float":
                case "neg-int":
                case "neg-long":
                case "new-array":
                case "nop":
                case "not-int":
                case "not-long":
                case "or-int":
                case "or-int/2addr":
                case "or-int/lit16":
                case "or-long":
                case "or-long/2addr":
                case "packed-switch":
                case "rem-float":
                case "rem-float/2addr":
                case "rem-int":
                case "rem-int/2addr":
                case "rem-int/lit16":
                case "rem-int/lit8":
                case "rem-long":
                case "rem-long/2addr":
                case "return-void-barrier":
                case "return-wide":
                case "rsub-int":
                case "rsub-int/lit8":
                case "sget":
                case "sget-boolean":
                case "sget-byte":
                case "sget-char":
                case "sget-object-volatile":
                case "sget-short":
                case "sget-volatile":
                case "sget-wide":
                case "sget-wide-volatile":
                case "shl-int":
                case "shl-int/2addr":
                case "shl-long":
                case "shl-long/2addr":
                case "shr-int":
                case "shr-int/2addr":
                case "shr-long":
                case "shr-long/2addr":
                case "sparse-switch":
                case "sput":
                case "sput-boolean":
                case "sput-byte":
                case "sput-char":
                case "sput-object-volatile":
                case "sput-short":
                case "sput-volatile":
                case "sput-wide":
                case "sput-wide-volatile":
                case "sub-double":
                case "sub-float":
                case "sub-float/2addr":
                case "sub-int":
                case "sub-int/2addr":
                case "sub-long":
                case "sub-long/2addr":
                case "throw-verification-error":
                case "ushr-int":
                case "ushr-int/2addr":
                case "ushr-long":
                case "ushr-long/2addr":
                case "xor-int":
                case "xor-int/2addr":
                case "xor-long":
                case "xor-long/2addr":
                    rv.Smali = LineSmali.Unimplemented;
                    rv.aName = sRawText;
                    break;
                case "goto":
                case "goto/16":
                case "goto/32":
                case "if-eq":
                case "if-eqz":
                case "if-ge":
                case "if-gez":
                case "if-gt":
                case "if-gtz":
                case "if-le":
                case "if-lez":
                case "if-lt":
                case "if-ltz":
                case "if-ne":
                case "if-nez":
                    rv.Smali = LineSmali.Conditional;
                    rv.aName = sRawText;
                    break;
                default:
                    rv.Smali = LineSmali.Unknown;
                    rv.aName = sRawText;
                    break;
                #endregion
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
