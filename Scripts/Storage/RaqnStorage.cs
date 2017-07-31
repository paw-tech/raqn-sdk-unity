using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using FullSerializer;
using RAQN.Serialization;

namespace RAQN.Storage
{
    public static class RaqnStorage
    {
        private static string[] _persistentDataPaths;

        private static bool IsWritableDir(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return false;
                string file = Path.Combine(path, Path.GetRandomFileName());
                using (FileStream fs = File.Create(file, 1)) { }
                File.Delete(file);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetPersistentDataPath(params string[] components)
        {
            try
            {
                string path = Path.DirectorySeparatorChar + string.Join("" + Path.DirectorySeparatorChar, components);
                if (!Directory.GetParent(path).Exists) return null;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!IsWritableDir(path))
                {
                    return null;
                }
                return path;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return null;
            }
        }

        private static string persistentDataPathInternal
        {
            #if UNITY_ANDROID
                get
                {
                    if (Application.isEditor || !Application.isPlaying) return Application.persistentDataPath;
                    string path = null;
                    if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "emulated", "0", "Android", "data", Application.identifier, "files");
                    if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("data", "data", Application.identifier, "files");
                    return path;
                }
            #else
                get { return Application.persistentDataPath; }
            #endif
        }

        public static string persistentDataPathExternal
        {
#if UNITY_ANDROID
            get
            {
                if (Application.isEditor || !Application.isPlaying) return null;
                string path = null;
                if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "sdcard0", "Android", "data", Application.identifier, "files");
                if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("storage", "sdcard1", "Android", "data", Application.identifier, "files");
                if (string.IsNullOrEmpty(path)) path = GetPersistentDataPath("mnt", "sdcard", "Android", "data", Application.identifier, "files");
                return path;
            }
#else
	get { return null; }
#endif
        }

        private static string[] persistentDataPaths
        {
            get
            {
                if (_persistentDataPaths == null)
                {
                    List<string> paths = new List<string>();
                    if (!string.IsNullOrEmpty(persistentDataPathInternal)) paths.Add(persistentDataPathInternal);
                    if (!string.IsNullOrEmpty(persistentDataPathExternal)) paths.Add(persistentDataPathExternal);
                    if (!string.IsNullOrEmpty(Application.persistentDataPath) && !paths.Contains(Application.persistentDataPath)) paths.Add(Application.persistentDataPath);
                    _persistentDataPaths = paths.ToArray();
                }
                return _persistentDataPaths;
            }
        }

        // returns best persistent data path
        public static string persistentDataPath
        {
            get { return persistentDataPaths.Length > 0 ? System.IO.Path.GetDirectoryName(persistentDataPaths[0]) : null; }
        }

        public static string DataPath(bool _persistent = false, bool _absolute = false)
        {
            string _path = "";
            if (_absolute) { return _path; }
            if (_persistent)
            { 
                _path = persistentDataPath;
            }
            else
            {
                //_path = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
                _path = UnityEngine.Application.temporaryCachePath;
                Debug.Log(_path);
            }
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
            return _path + "/";
        }

        public static byte[] GetBytes(string _filename, bool _persistent = false, bool _absolute = false)
        {
            string _file = DataPath(_persistent, _absolute) + _filename;
            byte[] _bytes = new byte[0];
            if (!File.Exists(_file))
            {
                Debug.LogError("Could not find file " + _file);
                return _bytes;
            }
            _bytes = File.ReadAllBytes(_file);
            return _bytes;
        }

        public static bool PutBytes(string _filename, byte[] _bytes, bool _persistent = false, bool _absolute = false)
        {
            string _file = DataPath(_persistent, _absolute) + _filename;
            File.WriteAllBytes(_file, _bytes);
            return true;
        }

        public static T LoadJson<T>(string _filename, bool _persistent = false, string _obfuscated = "", bool _absolute = false)
        {
            string _file = DataPath(_persistent, _absolute) + _filename;
            string _data;
            try
            {
                if (!File.Exists(_file))
                {
                    return default(T);
                }
                _data = File.ReadAllText(_file);
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
                return default(T);
            }
            if (_obfuscated != "")
            {
                _data = RaqnSerializer.ObfuscateXOR(_obfuscated, _data);
            }
            return RaqnSerializer.JsonDeserialize<T>(_data);
        }

        public static bool SaveJson<T>(string _filename, T content, bool _persistent = false, string _obfuscated = "", bool _absolute = false)
        {
            string _file = DataPath(_persistent, _absolute) + _filename;
            try
            {
                if (!File.Exists(_file))
                {
                    FileStream _fs = File.Create(_file);
                    _fs.Close();
                }
                string _data = RaqnSerializer.JsonSerialize(content);
                if (_obfuscated != "")
                {
                    _data = RaqnSerializer.ObfuscateXOR(_obfuscated, _data);
                }
                File.WriteAllText(_file, _data);
            }
            catch (Exception _e)
            {
                Debug.LogError(_e.Message);
                return false;
            }
            return true;
        }
    }
}