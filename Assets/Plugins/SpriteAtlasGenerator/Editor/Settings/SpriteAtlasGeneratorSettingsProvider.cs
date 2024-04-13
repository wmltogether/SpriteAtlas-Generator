using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace SpriteAtlasGenerator.Editor.Settings
{
    public class SpriteAtlasGeneratorSettingsProvider: SettingsProvider
    {
        private const string SettingPath = "Project/Sprite Atlas Generator";
        private SerializedObject _serializedObject;

        private SerializedProperty _packSettingsDirectory;
        private SerializedProperty _atlasOutputDirectory;
        private SerializedProperty _defaultAtlasSettings;
        
        private static SpriteAtlasGeneratorSettingsProvider provider;
        [SettingsProvider]
        public static SettingsProvider CreateSettingProvider()
        {
            if (SpriteAtlasGeneratorSettings.Instance && provider == null)
            {
                provider = new SpriteAtlasGeneratorSettingsProvider();
                using (var so = new SerializedObject(SpriteAtlasGeneratorSettings.Instance))
                {
                    provider.keywords = GetSearchKeywordsFromSerializedObject(so);
                }
            }
            return provider;
        }

        public SpriteAtlasGeneratorSettingsProvider() : base(SettingPath, SettingsScope.Project)
        {
            
        }
        public SpriteAtlasGeneratorSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords) : base(path, scopes, keywords)
        {
            
        }
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            OnCreate();
        }

        private void OnCreate()
        {
            var setting = SpriteAtlasGeneratorSettings.LoadOrCreate();
            setting.Validate();
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);
            _packSettingsDirectory = _serializedObject.FindProperty("packSettingsDirectory");
            _atlasOutputDirectory = _serializedObject.FindProperty("atlasOutputDirectory");
            _defaultAtlasSettings = _serializedObject.FindProperty("defaultAtlasSettings");
            
        }

        public override void OnGUI(string searchContext)
        {
            using (CreateSettingsWindowGUIScope())
            {
                if (_serializedObject == null||!_serializedObject.targetObject)
                {
                    OnCreate();
                }

                if (_serializedObject == null) return;
                _serializedObject.Update();
                EditorGUILayout.HelpBox(new GUIContent("Generator Settings"));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_packSettingsDirectory);
                EditorGUILayout.PropertyField(_atlasOutputDirectory);
                EditorGUILayout.HelpBox(new GUIContent("Atlas Settings"));
                EditorGUILayout.PropertyField(_defaultAtlasSettings);
                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    Save();
                }
            }
        }
        
        private IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }
        
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Save();
        }

        private void Save()
        {
            SpriteAtlasGeneratorSettings.Save();
        }
    }
}