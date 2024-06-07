using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpriteAtlasGenerator.Editor.Reader;
using SpriteAtlasGenerator.Editor.Settings;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.U2D;

namespace SpriteAtlasGenerator.Editor
{
    public static class SpriteAtlasGen
    {
        [MenuItem("Tools/SpriteAtlas Generator/Generate")]
        public static void Generate()
        {
            // FIRST: Collect All Rules

            if (!(EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2 || EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2Build))
            {
                Debug.LogError("[SpriteAtlasGenerator] SpriteAtlasV2 is not enabled in Project Settings");
                return;
            }

            var packSettingsDir = SpriteAtlasGeneratorSettings.Instance.packSettingsDirectory;
            var atlasOutputDirectory = SpriteAtlasGeneratorSettings.Instance.atlasOutputDirectory;
            if (!Directory.Exists(packSettingsDir))
            {
                Debug.LogWarning("[SpriteAtlasGenerator] No pack settings found in " + packSettingsDir );
                return;
            }
            
            try
            {
                if (string.IsNullOrEmpty(atlasOutputDirectory))
                {
                    return;
                }
                if (File.Exists(atlasOutputDirectory))
                {
                    Debug.LogError("[SpriteAtlasGenerator] atlasOutputDirectory can't be a file" );
                    return;
                }
                if (!Directory.Exists(atlasOutputDirectory))
                {
                    Directory.CreateDirectory(atlasOutputDirectory);
                }
                EditorUtility.DisplayProgressBar("SpriteAtlasGenerator", "Generating SpriteAtlas...", 0.1f);
                var packSettingsRules = Directory.GetFiles(packSettingsDir, "*.json", SearchOption.AllDirectories);
                for (var index = 0; index < packSettingsRules.Length; index++)
                {
                    var ruleFile = packSettingsRules[index];
                    var ruleReader = new PackRuleReader(ruleFile);
                    if (ruleReader.IsEmpty())
                    {
                        continue;
                    }

                    if (ruleReader.IsFormatError)
                    {
                        Debug.LogError("[SpriteAtlasGenerator] Format error in " + ruleFile + "," +
                                       ruleReader.GetErrorMsg());
                        continue;
                    }

                    float progress = (float)index / (packSettingsRules.Length + 1);
                    Debug.Log($"[SpriteAtlasGenerator] {atlasOutputDirectory}/{ruleReader.AtlasName}");
                    var sAtlasPath = Path.Combine(atlasOutputDirectory, ruleReader.AtlasName);
                    EditorUtility.DisplayProgressBar("SpriteAtlasGenerator", $"Generating {ruleReader.AtlasName}...", progress);
                    var extName = "";
                    if (EditorSettings.spritePackerMode == SpritePackerMode.SpriteAtlasV2)
                    {
                        extName = ".spriteatlasv2";
                    }
                    else
                    {
                        extName = ".spriteatlas";
                    }
                    if (sAtlasPath.EndsWith(extName, StringComparison.InvariantCulture))
                    {
                        sAtlasPath = sAtlasPath.Remove(sAtlasPath.Length - extName.Length);
                    }
                    sAtlasPath += extName;
                    sAtlasPath = sAtlasPath.Replace("\\", "/");
                    SpriteAtlasAsset atlas = SpriteAtlasAsset.Load(sAtlasPath);
                    if (atlas == null)
                    {
                        atlas = CreateSpriteAtlas(ruleReader, sAtlasPath);
                    }
                        
                    else
                    {
                        OverrideAtlasSettings(ruleReader, sAtlasPath);
                    }

                    UnityEngine.Object[] packables;
                    var newPackableList = new HashSet<UnityEngine.Object>();
                    while (true)
                    {
                        packables = atlas.GetMasterAtlas() == null ? null :
                            atlas.GetMasterAtlas().GetPackables();
                        if (packables != null && packables.Length > 0)
                        {
                            atlas.Remove(packables);
                            atlas.GetMasterAtlas().Remove(packables);
                        }
                        else
                        {
                            break;
                        }
                    }
                    EditorUtility.SetDirty(atlas);
                    SpriteAtlasAsset.Save(atlas, sAtlasPath);
                    AssetDatabase.SaveAssets();
                    atlas = SpriteAtlasAsset.Load(sAtlasPath);
                    if (ruleReader.Folders != null)
                    {
                        foreach (var folder in ruleReader.Folders)
                        {
                            bool isFolder = AssetDatabase.IsValidFolder(folder);
                            if (isFolder)
                            {
                                var files = FindImages(folder);
                                //获取目录下所有图片
                                for (int z = 0; z < files?.Count; z++)
                                {
                                    files[z] = files[z].Replace("\\", "/");
                                    AddNewSpriteToAtlas(files[z], newPackableList);
                                }
                            }
                        }
                    }

                    if (ruleReader.Files != null)
                    {
                        var files = ruleReader.Files;
                        for (int z = 0; z < files?.Count; z++)
                        {
                            files[z] = files[z].Replace("\\", "/");
                            AddNewSpriteToAtlas(files[z], newPackableList);
                        }
                    }

                    packables = newPackableList.Where(it => it != null).OrderBy(it => it.name).Distinct().ToArray();
                    atlas.Remove(packables);
                    atlas.Add(packables);
                    EditorUtility.SetDirty(atlas);
                    SpriteAtlasAsset.Save(atlas, sAtlasPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(atlasOutputDirectory, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ImportRecursive);
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        private static List<string> FindImages(string folder)
        {
            var root = Directory.GetParent(Application.dataPath);
            var rootFull = root?.FullName ?? "";
            string[] dFiles = System.IO.Directory.GetFiles(folder, "*.*");
            List<string> result = new List<string>();
            if (dFiles?.Length > 0)
            {
                foreach (var tFile in dFiles)
                {
                    if (!tFile.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) &&
                        !tFile.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) &&
                        !tFile.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) &&
                        !tFile.EndsWith(".bmp", StringComparison.InvariantCultureIgnoreCase) &&
                        !tFile.EndsWith(".tga", StringComparison.InvariantCultureIgnoreCase)
                       )
                    {
                        continue;
                    }
                    var tm = new FileInfo(tFile).FullName.Substring(rootFull.Length + 1);
                    result.Add(tm);
                }
            }
            return result;

        }

        private static void OverrideAtlasSettings(PackRuleReader rule, string atlasPath)
        {
            SpriteAtlasGeneratorSettings.Instance.Validate();
            var path = atlasPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"atlas importer not found {atlasPath}");
                return;
            }
            var importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;
            Assert.IsNotNull(importer);
            UpdateTextureImportSettings(importer, rule);
        }

        /// <summary>
        /// 创建图集, 对图集进行统一设置
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <returns></returns>
        /// <returns></returns>
        private static SpriteAtlasAsset CreateSpriteAtlas(PackRuleReader rule, string atlasPath)
        {
           
            var atlas = new SpriteAtlasAsset();
            SpriteAtlasAsset.Save(atlas, atlasPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(atlasPath) as SpriteAtlasImporter;
            Assert.IsNotNull(importer);
            UpdateTextureImportSettings(importer, rule);
            return atlas;
        }

        private static void UpdateTextureImportSettings(SpriteAtlasImporter importer, PackRuleReader rule)
        {
            SpriteAtlasGeneratorSettings.Instance.Validate();
            var defaultSettings = SpriteAtlasGeneratorSettings.Instance.defaultAtlasSettings;
            if (rule.IncludeInBuild.HasValue)
            {
                importer.includeInBuild = rule.IncludeInBuild.Value;
            }
            else
            {
                importer.includeInBuild = defaultSettings.IncludeInBuild;
            }
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = rule.EnableRotation ?? defaultSettings.EnableRotation,
                enableTightPacking = rule.EnableTightPacking ?? defaultSettings.EnableTightPacking,
                padding = rule.Padding ?? defaultSettings.Padding,
                enableAlphaDilation = rule.EnableAlphaDilation ?? true,
            };
            importer.packingSettings = (packSetting);
            SpriteAtlasTextureSettings textureSetting = new SpriteAtlasTextureSettings()
            {
                readable = false,
                generateMipMaps = false,
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            };
            importer.textureSettings = textureSetting;
            int maxSize = 2048; //最大尺寸
            if (rule.MaxSize.HasValue)
            {
                maxSize = Mathf.Clamp(rule.MaxSize.Value, 256, 4096);
            }
            int qualityLevel = 70;
            var defaultTexSettings = importer.GetPlatformSettings("DefaultTexturePlatform");
            if (defaultTexSettings == null)
                defaultTexSettings = new TextureImporterPlatformSettings();
            defaultTexSettings.name = "DefaultTexturePlatform";
            defaultTexSettings.maxTextureSize = maxSize;
            defaultTexSettings.textureCompression = TextureImporterCompression.Compressed;
            defaultTexSettings.compressionQuality = qualityLevel;
            defaultTexSettings.format = TextureImporterFormat.Automatic;
            defaultTexSettings.overridden = true;
            importer.SetPlatformSettings(defaultTexSettings);
            var atlasAndSetting = importer.GetPlatformSettings("Android");
            if (atlasAndSetting == null)
                atlasAndSetting = new TextureImporterPlatformSettings();
            atlasAndSetting.overridden = true;
            atlasAndSetting.name = "Android";
            atlasAndSetting.maxTextureSize = maxSize;
            atlasAndSetting.compressionQuality = 0;
            atlasAndSetting.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformSettings(atlasAndSetting);
            var atlasiOSSetting = importer.GetPlatformSettings("iPhone");
            if (atlasiOSSetting == null)
                atlasiOSSetting = new TextureImporterPlatformSettings();
            atlasiOSSetting.overridden = true;
            atlasiOSSetting.name = "iPhone";
            atlasiOSSetting.maxTextureSize = maxSize;
            atlasiOSSetting.compressionQuality = 0;
            atlasiOSSetting.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformSettings(atlasiOSSetting);
            importer.SaveAndReimport();
        }

        private static void AddNewSpriteToAtlas(string filePath, HashSet<UnityEngine.Object> newPackableList)
        {
            UnityEngine.Object spriteObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
            if (!(spriteObj is Texture2D))
                return;
            var texture = (Texture2D)spriteObj;
            var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                return;
            }
            if (importer.spriteImportMode == SpriteImportMode.None)
            {
                return;
            }
            if (importer.spriteImportMode == SpriteImportMode.Multiple)
            {
                return;
            }
            if (texture.width * texture.height > 1280 * 1024)
            {
                return;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            if (!newPackableList.Contains(texture))
                newPackableList.Add(texture);
        }
    }
}