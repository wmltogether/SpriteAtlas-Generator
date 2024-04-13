using System.Collections.Generic;

namespace SpriteAtlasGenerator.Editor.SimpleJSON
{
    internal static class SimpleJSONHelper
    {
        public static bool TryGetBool(this JSONNode node, string key, out bool value)
        {
            if (node.HasKey(key))
            {
                if (node[key].IsBoolean)
                {
                    value = node[key].AsBool;
                    return true;
                }
                else if (node[key].IsNumber)
                {
                    value = node[key].AsInt >= 1;
                    return true;
                }
                value = false;
            }
            value = false;
            return false;
        }
        
        public static bool TryGetInt(this JSONNode node, string key, out int value)
        {
            if (node.HasKey(key))
            {
                if (node[key].IsNumber)
                {
                    value = node[key].AsInt;
                    return true;
                }
                else if (node[key].IsBoolean)
                {
                    value = node[key].AsBool ? 1 : 0;
                    return true;
                }
                value = 0;
            }
            value = 0;
            return false;
        }
        
        public static bool TryGetString(this JSONNode node, string key, out string value)
        {
            if (node.HasKey(key))
            {
                value = node[key];
                return true;
            }
            value = default;
            return false;
        }
        
        public static bool TryGetStringList(this JSONNode node, string key, out List<string> value)
        {
            if (node.HasKey(key))
            {
                if (node[key].IsArray)
                {
                    value = node[key].AsArray.AsStringList;
                }
                else
                {
                    value = new List<string>();
                }
                return true;
            }
            value = default;
            return false;
        }
        
        public static bool TryGetLong(this JSONNode node, string key, out long value)
        {
            if (node.HasKey(key))
            {
                if (node[key].IsNumber)
                {
                    value = node[key].AsLong;
                    return true;
                }
                else if (node[key].IsBoolean)
                {
                    value = node[key].AsBool ? 1 : 0;
                    return true;
                }
                value = 0;
            }
            value = 0;
            return false;
        }
    }
}