using System;
using System.Diagnostics;

namespace SpriteAtlasGenerator.Editor.Reader
{
    internal static class SafeConvert
    {

        public static string ToString(object s)
        {
            if (s == null)
            {
                return string.Empty;
            }

            if (s is string dst)
            {
                return dst;
            }
            return s.ToString();
        }

        private static readonly char[] TrimTags = { '\t', '\x20', '\r', '\n', '\uFFFE', '\uFEFF' };
        public static bool ToBoolean(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            var result = input.Trim(TrimTags).ToLowerInvariant();
            if (result.Equals("true") || result.Equals("1") || result.Equals("yes"))
            {
                return true;
            }
            Debug.Fail($"{input} is not a valid boolean string");
            return false;
        }
        public static int ToInt32(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var raw = input.Trim(TrimTags).ToLowerInvariant();
            if (raw.Equals("true") || raw.Equals("1") || raw.Equals("yes"))
            {
                return 1;
            }
            else if (raw.Equals("false") || raw.Equals("0") || raw.Equals("no"))
            {
                return 0;
            }
            else if (raw.StartsWith("0x"))
            {
                try
                {
                    int hexValue = Convert.ToInt32(raw, 16);
                    return hexValue;
                }
                catch (Exception)
                {
                    Debug.Fail($"{input} is not a valid hex string");
                    return 0;
                }
                
            }
            if (int.TryParse(raw, out var result))
            {
                return result;
            }
            Debug.Fail($"{input} is not a valid int32 string");

            return 0;
        }
        
        public static long ToLong(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var raw = input.Trim(TrimTags).ToLowerInvariant();
            if (raw.Equals("true") || raw.Equals("1") || raw.Equals("yes"))
            {
                return 1;
            }
            else if (raw.Equals("false") || raw.Equals("0") || raw.Equals("no"))
            {
                return 0;
            }
            else if (raw.StartsWith("0x"))
            {
                try
                {
                    var hexValue = Convert.ToInt64(raw, 16);
                    return hexValue;
                }
                catch (Exception)
                {
                    Debug.Fail($"{input} is not a valid hex string");
                    return 0;
                }
                
            }
            if (long.TryParse(raw, out var result))
            {
                return result;
            }
            Debug.Fail($"{input} is not a valid int32 string");

            return 0;
        }
        
        public static float ToSingle(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }

            if (float.TryParse(input.Trim(TrimTags), out var result))
            {
                return result;
            }
            Debug.Fail($"{input} is not a valid float string");
            return 0;
        }
    }

}