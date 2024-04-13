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
                        atlas = CreateSpriteAtlas(sAtlasPath);
                    UnityEngine.Object[] packables = atlas.GetMasterAtlas() == null ? null :
                        atlas.GetMasterAtlas().GetPackables();
                    List<UnityEngine.Object> newPackableList = new List<UnityEngine.Object>();
                    if (packables != null)
                    {
                        atlas.Remove(packables);
                    }
                    packables = atlas.GetMasterAtlas() == null ? null :
                        atlas.GetMasterAtlas().GetPackables();
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
                                    AddNewSpriteToAtlas(files[z], packables, newPackableList);
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
                            AddNewSpriteToAtlas(files[z], packables, newPackableList);
                        }
                    }

                    packables = newPackableList.OrderBy(it => it.name).Distinct().ToArray();
                    atlas.Add(packables);
                    EditorUtility.SetDirty(atlas);
                    SpriteAtlasAsset.Save(atlas, sAtlasPath);
                    OverrideAtlasSettings(ruleReader, atlas);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
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

        private static void OverrideAtlasSettings(PackRuleReader ruleReader, SpriteAtlasAsset atlas)
        {
            SpriteAtlasGeneratorSettings.Instance.Validate();
            var defaultSettings = SpriteAtlasGeneratorSettings.Instance.defaultAtlasSettings;
            var path = AssetDatabase.GetAssetPath(atlas);
            if (string.IsNullOrEmpty(path)) return;
            var importer = AssetImporter.GetAtPath(path) as SpriteAtlasImporter;
            Assert.IsNotNull(importer);
            if (ruleReader.IncludeInBuild.HasValue)
            {
                importer.includeInBuild = ruleReader.IncludeInBuild.Value;
            }
                  
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = ruleReader.EnableRotation ?? defaultSettings.EnableRotation,
                enableTightPacking = ruleReader.EnableTightPacking ?? defaultSettings.EnableTightPacking,
                padding = ruleReader.Padding ?? defaultSettings.Padding,
            };
            importer.packingSettings = packSetting ;
        }
        
        /// <summary>
        /// 创建图集, 对图集进行统一设置
        /// </summary>
        /// <param name="atlasPath"></param>
        /// <returns></returns>
        private static SpriteAtlasAsset CreateSpriteAtlas(string atlasPath)
        {
            SpriteAtlasGeneratorSettings.Instance.Validate();
            var defaultSettings = SpriteAtlasGeneratorSettings.Instance.defaultAtlasSettings;
            var atlas = new SpriteAtlasAsset();
            SpriteAtlasAsset.Save(atlas, atlasPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(atlasPath) as SpriteAtlasImporter;
            Assert.IsNotNull(importer);
            importer.includeInBuild = defaultSettings.IncludeInBuild;
            SpriteAtlasPackingSettings packSetting = new SpriteAtlasPackingSettings()
            {
                blockOffset = 1,
                enableRotation = defaultSettings.EnableRotation,
                enableTightPacking = defaultSettings.EnableTightPacking,
                padding = defaultSettings.Padding,
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
            int qualityLevel = 70;
            var defaultTexSettings = importer.GetPlatformSettings("DefaultTexturePlatform");
            if (defaultTexSettings == null)
                defaultTexSettings = new TextureImporterPlatformSettings();
            defaultTexSettings.maxTextureSize = maxSize;
            defaultTexSettings.textureCompression = TextureImporterCompression.Compressed;
            defaultTexSettings.compressionQuality = qualityLevel;
            defaultTexSettings.format = TextureImporterFormat.Automatic;
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
            atlasAndSetting.name = "iPhone";
            atlasiOSSetting.maxTextureSize = maxSize;
            atlasiOSSetting.compressionQuality = 0;
            atlasiOSSetting.format = TextureImporterFormat.ASTC_4x4;
            importer.SetPlatformSettings(atlasiOSSetting);
            importer.SaveAndReimport();
            return atlas;
        }

        private static void AddNewSpriteToAtlas(string filePath, UnityEngine.Object[] packables, List<UnityEngine.Object> newPackableList)
        {
            UnityEngine.Object spriteObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
            if (!(spriteObj is Texture2D))
                return;
            var texture = (Texture2D)spriteObj;
            var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
            if (importer == null) return;
            if (texture.width * texture.height > 1280 * 1024)
            {
                return;
            }
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            if (packables == null || packables.Length == 0)
            {
                newPackableList.Add(texture);
                return;
            }
            if (Array.Find(packables, x => x == texture) == null)
                newPackableList.Add(texture);
        }
    }
}