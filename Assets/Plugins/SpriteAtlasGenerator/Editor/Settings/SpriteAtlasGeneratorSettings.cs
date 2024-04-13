using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpriteAtlasGenerator.Editor.Settings
{
    [SettingsFilePath("SpriteAtlasGeneratorSettings.asset")]
    public class SpriteAtlasGeneratorSettings : ScriptableSingleton<SpriteAtlasGeneratorSettings>
    {
        [Header("Pack JSON Rules Directory (*.json)")]
        public string packSettingsDirectory = "Assets/Art/SpriteAtlas/PackRules";
        [Header("Atlas Output")]
        public string atlasOutputDirectory = "Assets/Art/SpriteAtlas/Generated";
        [Header("Default Atlas Settings")]
        public AtlasSettings defaultAtlasSettings = new AtlasSettings();

        public void Validate()
        {
            if (defaultAtlasSettings == null)
            {
                defaultAtlasSettings = new AtlasSettings();
            }
        }

        [System.Serializable]
        public class AtlasSettings
        {
            [Header("Include In Build")]
            [Tooltip("If false, the atlas will not be included in the build.")]
            [FormerlySerializedAs("includeInBuild")]
            public bool IncludeInBuild = true;
            [Header("Padding")]
            [FormerlySerializedAs("padding")]
            public int Padding = 8;
            [Header("Enable Rotation")]
            [FormerlySerializedAs("enableRotation")]
            public bool EnableRotation = false;
            [Header("Tight Packing")]
            [FormerlySerializedAs("enableTightPacking")]
            public bool EnableTightPacking = false;
            [Header("Alpha Dilation")]
            [FormerlySerializedAs("enableAlphaDilation")]
            public bool EnableAlphaDilation = true;
        }
    }
}