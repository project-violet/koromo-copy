/***

   Copyright (C) 2018-2019. dc-koromo. All Rights Reserved.
   
   Author: Koromo Copy Developer

***/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koromo_Copy.LP
{
    /// <summary>
    /// 한글을 영어로 바꾸거나 영어를 한글로 바꾸는 도구 집합입니다.
    /// </summary>
    public class LPKor
    {
        static readonly char[] IndexHangulLetter = { 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ', 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ', 'ㅤ', 'ㅥ', 'ㅦ', 'ㅧ', 'ㅨ', 'ㅩ', 'ㅪ', 'ㅫ', 'ㅬ', 'ㅭ', 'ㅮ', 'ㅯ', 'ㅰ', 'ㅱ', 'ㅲ', 'ㅳ', 'ㅴ', 'ㅵ', 'ㅶ', 'ㅷ', 'ㅸ', 'ㅹ', 'ㅺ', 'ㅻ', 'ㅼ', 'ㅽ', 'ㅾ', 'ㅿ', 'ㆀ', 'ㆁ', 'ㆂ', 'ㆃ', 'ㆄ', 'ㆅ', 'ㆆ', 'ㆇ', 'ㆈ', 'ㆉ', 'ㆊ', 'ㆋ', 'ㆌ', 'ㆍ', 'ㆎ' };
        static readonly string[] IndexHangulLetterCh2 = { "r", "R", "rt", "s", "sw", "sg", "e", "E", "f", "fr", "fa", "fq", "ft", "fe", "fv", "fg", "a", "q", "Q", "qt", "t", "T", "d", "w", "W", "c", "z", "e", "v", "g", "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l", " ", "ss", "se", "st", " ", "frt", "fe", "fqt", " ", "fg", "aq", "at", " ", " ", "qr", "qe", "qtr", "qte", "qw", "qe", " ", " ", "tr", "ts", "te", "tq", "tw", " ", "dd", "d", "dt", " ", " ", "gg", " ", "yi", "yO", "yl", "bu", "bP", "bl" };
        static readonly string[] IndexHangulLetterCh3 = { "k", "kk", "kn", "h", "hl", "hm", "u", "uu", "y", "yk", "yi", "y;", "yn", "y'", "yp", "ym", "i", ";", ";;", ";n", "n", "nn", "j", "l", "ll", "o", "0", "'", "p", "m", "f", "r", "6", "G", "t", "c", "e", "&", "v", "vf", "vr", "vd", "4", "b", "bt", "bc", "bd", "5", "g", "8", "d" };
        
        static readonly char[] IndexHangulInitial = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
        static readonly char[] IndexHangulInitialCh2 = { 'r', 'R', 's', 'e', 'E', 'f', 'a', 'q', 'Q', 't', 'T', 'd', 'w', 'W', 'c', 'z', 'x', 'v', 'g' };
        static readonly string[] IndexHangulInitialCh3 = { "k", "K", "h", "u", "uu", "y", "i", ";", ";;", "n", "nn", "j", "l", "ll", "o", "0", "'", "p", "m" };
        static readonly char[] IndexHangulInitialDu3 = { 'u', ';', 'n', 'l' };

        static readonly char[] IndexHangulMedial = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ', 'ㅢ' };
        static readonly string[] IndexHangulMedialCh2 = { "k", "o", "i", "O", "j", "p", "u", "P", "h", "hk", "ho", "hl", "y", "n", "nj", "np", "nl", "b", "m", "ml", "l" };
        static readonly char[] IndexHangulMedialDu2 = { 'k', 'o', 'l', 'j', 'p' };
        static readonly string[] IndexHangulMedialCh3 = { "f", "r", "6", "G", "t", "c", "e", "7", "v", "vf", "vr", "vd", "4", "b", "bt", "bc", "bd", "5", "g", "gd", "d" };
        static readonly char[] IndexHangulMedialDu3 = { 'f', 'r', 'd', 't', 'c' };

        static readonly char[] IndexHangulFinal = { (char)0, 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
        static readonly string[] IndexHangulFinalCh2 = { "", "r", "R", "rt", "s", "sw", "sg", "e", "f", "fr", "fa", "fq", "ft", "fx", "fv", "fg", "a", "q", "qt", "t", "T", "d", "w", "c", "z", "x", "v", "g" };
        static readonly char[] IndexHangulFinalDu2 = { 't', 'w', 'g', 'r', 'a', 'q', 'x', 'v' };
        static readonly string[] IndexHangulFinalCh3 = { "", "x", "xx", "xq", "s", "E", "S", "A", "w", "wx", "wz", "w3", "wq", "wW", "wQ", "w1", "z", "3", "3q", "q", "2", "a", "#", "Z", "C", "W", "Q", "1" };
        static readonly string[] IndexHangulFinalCh3Do = { "", "x", "!", "V", "s", "E", "S", "A", "w", "@", "F", "D", "T", "%", "$", "R", "z", "3", "X", "q", "qq", "a", "#", "Z", "C", "W", "Q", "1" };
        static readonly char[] IndexHangulFinalDu3 = { 'x', 'q' };

        static readonly char[] IndexNumberic3 = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static readonly char[] IndexNumbericCh3 = { 'H', 'J', 'K', 'L', ':', 'Y', 'U', 'I', 'O', 'P' };
        static readonly char[] IndexSymbol3 = { '*', '=', '"', '"', '\'', '~', ')', '>', ':', '(', '<', ',', '.', '!', '?', '※', ';', '+', '\\', '%', '/', ',', '.' };
        static readonly char[] IndexSymbolCh3 = { '`', '^', '&', '*', '(', ')', '-', '=', '\\', '[', ']', ',', '.', '?', 'B', '~', '_', '+', '|', '{', '}', '<', '>' };

        public static bool SafeLetterExecuative = true;

        public struct Hangul_Initial
        {
            public int initial;
            public int medial;
            public int final;
        }

        const int Hangul_unicode_magic = 0xAC00;

        private static char hangul_combination(Hangul_Initial hi)=>
            (char)(Hangul_unicode_magic + hi.initial * 21 * 28 + hi.medial * 28 + hi.final);
        private static char hangul_comination(int initial, int medial, int final) =>
            (char)(Hangul_unicode_magic + initial * 21 * 28 + medial * 28 + final);

        private static Hangul_Initial hangul_distortion(char hi)
        {
            Hangul_Initial rethi;
            int unis = hi - Hangul_unicode_magic;
            rethi.initial = unis / (21 * 28);
            rethi.medial = (unis % (21 * 28)) / 28;
            rethi.final = (unis % (21 * 28)) % 28;
            return rethi;
        }

        public static string Assembly(string hhh)
        {
            var builder = new StringBuilder(hhh.Length);

            for (int i = 0; i <= hhh.Length - 1; i++)
            {
                int initial = -1;
                int medial = -1;
                int final = -1;
                
                initial = findifexist(IndexHangulInitialCh2, hhh[i]);
                if (initial < 0)
                {
                    if (SafeLetterExecuative)
                    {
                        builder.Append(hhh[i]); continue;
                    }
                    
                    medial = findifexist(IndexHangulMedialCh2, hhh[i].ToString());

                    if (medial < 0)
                        builder.Append(hhh[i]);
                    else
                        builder.Append(IndexHangulMedial[medial]);
                    continue;
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(IndexHangulInitial[initial]);
                    break;
                }
                i += 1;
                
                medial = findifexist(IndexHangulMedialCh2, hhh[i].ToString());
                if (medial < 0)
                {
                    builder.Append(IndexHangulInitial[initial]);
                    i -= 1;
                    continue;
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(hangul_comination(initial, medial, 0));
                    continue;
                }
                else if (hhh.Length - 1 != i)
                {
                    if (IndexHangulMedialDu2.Contains(hhh[i + 1]))
                        jksaveput(new string(new[] { hhh[i], hhh[i + 1] }), IndexHangulMedialCh2, ref medial, ref i);
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(hangul_comination(initial, medial, 0));
                    break;
                }
                i += 1;
                
                bool stop_force = false;
                if (hhh.Length - 1 >= i + 1)
                {
                    if (IndexHangulMedialCh2.Contains(hhh[i + 1].ToString()))
                    {
                        i -= 1;
                        builder.Append(hangul_comination(initial, medial, 0));
                        continue;
                    }
                    else if (hhh.Length - 1 >= i + 2)
                    {
                        if (IndexHangulMedialCh2.Contains(hhh[i + 2].ToString()))
                            stop_force = true;
                    }
                }
                
                final = findifexist(IndexHangulFinalCh2, hhh[i].ToString());
                if (final < 0)
                {
                    builder.Append(hangul_comination(initial, medial, 0));
                    i -= 1;
                    continue;
                }
                if (hhh.Length - 1 >= i + 1 && !stop_force)
                {
                    if (IndexHangulFinalDu2.Contains(hhh[i + 1]))
                        jksaveput(new string(new[] { hhh[i], hhh[i + 1] }), IndexHangulFinalCh2, ref final, ref i);
                }
                builder.Append(hangul_comination(initial, medial, final));
            }

            return builder.ToString();
        }
        
        public static string Assembly3(string hhh)
        {
            var builder = new StringBuilder(hhh.Length);

            for (int i = 0; i <= hhh.Length - 1; i++)
            {
                int initial = -1;
                int medial = -1;
                int final = -1;
                
                initial = findifexist(IndexHangulInitialCh3, hhh[i].ToString());
                if (initial < 0)
                {
                    medial = findifexist(IndexHangulMedialCh3, hhh[i].ToString());
                    if (medial < 0)
                    {
                        int num_sym = -1;
                        num_sym = findifexist(IndexNumbericCh3, hhh[i]);

                        if (num_sym >= 0)
                            builder.Append(IndexNumberic3[num_sym]);
                        else
                        {
                            num_sym = findifexist(IndexSymbolCh3, hhh[i]);
                            if (num_sym >= 0)
                                builder.Append(IndexSymbol3[num_sym]);
                            else
                                builder.Append(hhh[i]);
                        }
                    }
                    else
                    {
                        if (SafeLetterExecuative)
                        {
                            builder.Append(hhh[i]); continue;
                        }

                        builder.Append(IndexHangulMedial[medial]);
                    }
                    continue;
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(IndexHangulInitial[initial]);
                    break;
                }
                else if (hhh.Length - 1 != i)
                {
                    if (IndexHangulInitialDu3.Contains(hhh[i + 1]))
                        jksaveput(new string(new[] { hhh[i], hhh[i + 1] }), IndexHangulInitialCh3, ref initial, ref i);
                }
                i += 1;
                
                var ntx = hhh[i];
                medial = findifexist(IndexHangulMedialCh3, ntx.ToString());
                if (medial < 0)
                {
                    if (hhh[i] == '8')
                        medial = findifexist(IndexHangulMedialCh3, "gd");
                    else if (hhh[i] == '9')
                    {
                        medial = findifexist(IndexHangulMedialCh3, "b");
                        ntx = 'b';
                    }
                    else if (hhh[i] == '/')
                    {
                        medial = findifexist(IndexHangulMedialCh3, "v");
                        ntx = 'v';
                    }
                    else
                    {
                        builder.Append(IndexHangulInitial[initial]);
                        i -= 1;
                        continue;
                    }
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(hangul_comination(initial, medial, 0));
                    continue;
                }
                else if (hhh.Length - 1 != i)
                {
                    if (IndexHangulMedialDu3.Contains(hhh[i + 1]))
                        jksaveput(new string(new[] { hhh[i], hhh[i + 1] }), IndexHangulMedialCh3, ref medial, ref i);
                }
                if (hhh.Length - 1 == i)
                {
                    builder.Append(hangul_comination(initial, medial, 0));
                    break;
                }
                i += 1;
                
                bool stop_force = false;
                if (hhh.Length - 1 >= i + 1)
                {
                    if (IndexHangulMedialCh3.Contains(hhh[i + 1].ToString()))
                    {
                        i -= 1;
                        builder.Append(hangul_comination(initial, medial, 0));
                        continue;
                    }
                    else if (hhh.Length - 1 >= i + 2)
                    {
                        if (IndexHangulMedialCh3.Contains(hhh[i + 2].ToString()))
                            stop_force = true;
                    }
                }
                
                final = findifexist(IndexHangulFinalCh3, hhh[i].ToString());
                if (final < 0)
                {
                    final = findifexist(IndexHangulFinalCh3Do, hhh[i].ToString());
                    if (final < 0)
                    {
                        builder.Append(hangul_comination(initial, medial, 0));
                        i -= 1;
                        continue;
                    }
                }
                if (hhh.Length - 1 >= i + 1 && !stop_force)
                {
                    if (IndexHangulFinalDu3.Contains(hhh[i + 1]))
                        jksaveput(new string(new[] { hhh[i], hhh[i + 1] }), IndexHangulFinalCh3, ref final, ref i);
                }
                builder.Append(hangul_comination(initial, medial, final));
            }

            return builder.ToString();
        }
        
        private static int findifexist<T>(T[] _array, T _item) where T : IComparable<T>
        {
            for (int i = 0; i <= _array.Length - 1; i++)
            {
                if (_array[i].CompareTo(_item) == 0)
                    return i;
            }
            return -1;
        }
        
        private static void jksaveput(string jk_str, string[] _array, ref int _byput, ref int _i)
        {
            int save;
            save = findifexist(_array, jk_str);
            if (save >= 0)
            {
                _byput = save; _i += 1;
            }
        }

        public static string Disassembly(char letter)
        {
            if (IsHangulLetter(letter))
            {
                Hangul_Initial hi = hangul_distortion(letter);
                return IndexHangulInitialCh2[hi.initial] + IndexHangulMedialCh2[hi.medial] + IndexHangulFinalCh2[hi.final];
            }
            else if (IsHangulJamo31(letter))
            {
                int unis = letter;
                return IndexHangulLetterCh2[unis - 0x3131];
            }
            else
            {
                int unis = letter;
                return IndexHangulLetterCh2[unis - 0x1100];
            }
        }

        public static string Disassembly3(char letter)
        {
            if (IsHangulLetter(letter))
            {
                Hangul_Initial hi = hangul_distortion(letter);
                return IndexHangulInitialCh3[hi.initial] + IndexHangulMedialCh3[hi.medial] + IndexHangulFinalCh3[hi.final];
            }
            else if (IsHangulJamo31(letter))
            {
                int unis = letter;
                return IndexHangulLetterCh3[unis - 0x3131];
            }
            else if (IsHangulJamo11(letter))
            {
                int unis = letter;
                return IndexHangulLetterCh3[unis - 0x1100];
            }
            else
            {
                int num_sym = -1;
                for (int i = 0; i <= IndexNumberic3.Length - 1; i++)
                {
                    if (IndexNumberic3[i] == letter)
                    {
                        num_sym = i;
                        break;
                    }
                }
                if (num_sym >= 0)
                    return IndexNumbericCh3[num_sym].ToString();
                for (int i = 0; i <= IndexSymbol3.Length - 1; i++)
                {
                    if (IndexSymbol3[i] == letter)
                    {
                        num_sym = i;
                        break;
                    }
                }
                if (num_sym >= 0)
                    return IndexSymbolCh3[num_sym].ToString();
            }
            return "";
        }

        public static bool IsHangul(char letter)
        {
            int unis = letter;
            if (!SafeLetterExecuative)
            {
                if (0xAC00 <= unis && unis <= 0xD7FB)
                    return true;
                else if (0x3131 <= unis && unis <= 0x3163)
                    return true;
                else if (0x1100 <= unis && unis <= 0x11FF)
                    return true;
            }
            else if (0xAC00 <= unis && unis <= 0xD7FB)
                return true;
            return false;
        }

        public static bool IsHangul3(char letter)
        {
            int unis = letter;
            if (!SafeLetterExecuative)
            {
                if (0xAC00 <= unis && unis <= 0xD7FB)
                    return true;
                else if (0x3131 <= unis && unis <= 0x3163)
                    return true;
                else if (0x1100 <= unis && unis <= 0x11FF)
                    return true;
                else if (IndexNumberic3.Contains(letter) | IndexSymbol3.Contains(letter))
                    return true;
            }
            else if (0xAC00 <= unis && unis <= 0xD7FB)
                return true;
            else if (IndexNumberic3.Contains(letter) | IndexSymbol3.Contains(letter))
                return true;
            return false;
        }

        public static bool IsHangulLetter(char letter)
        {
            int unis = letter;
            if (0xAC00 <= unis && unis <= 0xD7FB)
                return true;
            return false;
        }

        public static bool IsHangulJamo31(char letter)
        {
            int unis = letter;
            if (0x3131 <= unis && unis <= 0x3163)
                return true;
            return false;
        }

        public static bool IsHangulJamo11(char letter)
        {
            int unis = letter;
            if (0x1100 <= unis && unis <= 0x11FF)
                return true;
            return false;
        }
    }
}
