using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SpriteAtlasGenerator.Editor.Settings
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T s_Instance;
        internal static T Instance
        {
            get
            {
                if (!s_Instance)
                {
                    LoadOrCreate();
                }
                return s_Instance;
            }
        }
        internal static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                s_Instance = arr.Length > 0 ? arr[0] as T : s_Instance ? s_Instance : CreateInstance<T>();
            }
            else
            {
                Debug.LogError($"save location of {nameof(ScriptableSingleton<T>)} is invalid");
            }
            return s_Instance;
        }

        internal static void Save(bool saveAsText = true)
        {
            if (!s_Instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(directoryName)) return;
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                var obj = new Object[] { s_Instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
            }
        }
        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Where(v => v is SettingsFilePathAttribute)
                .Cast<SettingsFilePathAttribute>()
                .FirstOrDefault()
                ?.filepath;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    internal class SettingsFilePathAttribute : Attribute
    {
        internal readonly string filepath;
        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public SettingsFilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }
            if (path.StartsWith('/'))
            {
                path = path.TrimStart('/');
            }
            filepath = "ProjectSettings/" + path;
        }
    }
}