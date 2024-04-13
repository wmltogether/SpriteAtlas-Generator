using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpriteAtlasGenerator.Editor.SimpleJSON;

namespace SpriteAtlasGenerator.Editor.Reader
{
    internal class PackRuleReader
    {
        public string AtlasName { get; protected set; }
        public bool? IncludeInBuild { get; protected set; }
        public int? Padding { get; protected set; }
        public bool? EnableRotation { get; protected set; }
        public bool? EnableTightPacking { get; protected set; }
        public bool? EnableAlphaDilation { get; protected set; }

        public List<string> Files { get; protected set; }
        public List<string> Folders { get; protected set; }
        
        private bool _isFormatError;
        private readonly StringBuilder _errorBuilder;

        public bool IsFormatError => _isFormatError;

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(AtlasName);
        }

        public string GetErrorMsg()
        {
            return _errorBuilder?.ToString();
        }
        const int maxCharsToCheck = 512;
        private bool IsJsonFile(byte[] content)
        {
            
            int numCharsToCheck = Math.Min(content.Length, maxCharsToCheck);
            int i = 0;
            if (content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF) // skip potential BOM
                i = 3;
            bool openingBraceFound = false;
            for (; i < numCharsToCheck; ++i) {
                char c = (char)content[i];
                if (char.IsWhiteSpace(c))
                    continue;
                if (!openingBraceFound) {
                    if (c == '{' || c == '[') openingBraceFound = true;
                    else return false;
                } else if (c == '{' || c == '[' || c == ']' || c == '}' || c == ',')
                    continue;
                else
                    return c == '"';
            }
            return true;
        }
        public PackRuleReader(string iniPath)
        {
            try
            {
                _errorBuilder = new StringBuilder();
                if (!File.Exists(iniPath))
                {
                    _isFormatError = true;
                    _errorBuilder.AppendLine("Could not find ini file at " + iniPath);
                    return;
                }

                var fs = File.OpenRead(iniPath);
                var chkChunk = new byte[maxCharsToCheck];
                _ = fs.Read(chkChunk, 0, (int)(Math.Max(chkChunk.Length, fs.Length)));
                if (!IsJsonFile(chkChunk))
                {
                    return;
                }

                fs.Seek(0, SeekOrigin.Begin);
                var content = new byte[fs.Length];
                _ = fs.Read(content, 0, content.Length);
                var jsonData = SimpleJSON.SimpleJSON.Parse(new UTF8Encoding(false).GetString(content));
                if (jsonData.HasKey("AtlasName"))
                {
                    var settingsSection = jsonData;
                    if (TryReadString(settingsSection, "AtlasName", out var atlasName, out var errAtlasName))
                    {
                        AtlasName = atlasName;
                        _errorBuilder.AppendLine(errAtlasName);
                    }
                    if (TryReadBool(settingsSection, "IncludeInBuild", out var includeInBuild, out var errIncludeInBuild))
                    {
                        IncludeInBuild = includeInBuild;
                        _errorBuilder.AppendLine(errIncludeInBuild);
                    }
                    if (TryReadInt(settingsSection, "Padding", out var padding, out var errPadding))
                    {
                        Padding = padding;
                        _errorBuilder.AppendLine(errPadding);
                    }
                    if (TryReadBool(settingsSection, "EnableRotation", out var enableRotation, out var errEnableRotation))
                    {
                        EnableRotation = enableRotation;
                        _errorBuilder.AppendLine(errEnableRotation);
                    }
                    if (TryReadBool(settingsSection, "EnableTightPacking", out var enableTightPacking, out var errEnableTightPacking))
                    {
                        EnableTightPacking = enableTightPacking;
                        _errorBuilder.AppendLine(errEnableTightPacking);
                    }
                    if (TryReadBool(settingsSection, "EnableAlphaDilation", out var enableAlphaDilation, out var errEnableAlphaDilation))
                    {
                        EnableAlphaDilation = enableAlphaDilation;
                        _errorBuilder.AppendLine(errEnableAlphaDilation);
                    }
                    if (TryReadStringList(settingsSection, "Files", out var files, out var errFiles))
                    {
                        Files = files;
                        _errorBuilder.AppendLine(errFiles);
                    }
                    if (TryReadStringList(settingsSection, "Folders", out var folders, out var errFolders))
                    {
                        Folders = folders;
                        _errorBuilder.AppendLine(errFolders);
                    }
                }
            }
            catch (Exception ex)
            {
                _isFormatError = true;
                _errorBuilder.AppendLine(ex.ToString());
            }
        }

        private bool TryReadBool(JSONNode node, string key, out bool result, out string onError)
        {
            result = default;
            onError = default;
            if (node.HasKey(key))
            {
                if (node.TryGetBool(key, out var v))
                {
                    result = v;
                    return true;
                }
                else
                {
                    onError = ($"{key} is not a valid boolean value.");
                }
            }

            return false;
        }
        
        private bool TryReadInt(JSONNode node, string key, out int result, out string onError)
        {
            result = default;
            onError = default;
            if (node.HasKey(key))
            {
                if (node.TryGetInt(key, out int v))
                {
                    result = v;
                    return true;
                }
                else
                {
                    onError = ($"{key} is not a valid int value.");
                }
            }

            return false;
        }
        
        private bool TryReadString(JSONNode node, string key, out string result, out string onError)
        {
            result = default;
            onError = default;
            if (node.HasKey(key))
            {
                if (node.TryGetString(key, out string v))
                {
                    result = v;
                    return true;
                }
                else
                {
                    onError = ($"{key} is not a valid string value.");
                }
            }
            return false;
        }
        
        private bool TryReadStringList(JSONNode node, string key, out List<string> result, out string onError)
        {
            result = default;
            onError = default;
            if (node.HasKey(key))
            {
                if (node.TryGetStringList(key, out List<string> v))
                {
                    result = v;
                    return true;

                }
                else
                {
                    onError = ($"{key} is not a valid string list.");
                }
            }
            return false;
        }
    }
}