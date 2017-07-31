using System.IO;
using FullSerializer;
using UnityEngine;
using RAQN.Serialization;

namespace RAQN.Storage
{
    [fsObject]
    public class RaqnStorable
    {

        static public T Load<T>(string _filename, bool _persistent = false, string _obfuscated = "") where T : RaqnStorable, new()
        {
            return RaqnStorage.LoadJson<T>(SecureDataPath() + "/" + _filename, _persistent, _obfuscated, true);
        }

        static public T LoadOrDie<T>(string _filename, bool _persistent = false, string _obfuscated = "") where T : RaqnStorable, new()
        {
            T _pd = RaqnStorage.LoadJson<T>(SecureDataPath() + "/" + _filename, _persistent, _obfuscated, true);
            if (_pd == null)
            {
                Debug.LogError("Could not load PlayData: " + _filename);
                UnityEngine.Application.Quit();
            }
            return _pd;
        }

        static public T LoadOrCreate<T>(string _filename, bool _persistent = false, string _obfuscated = "") where T : RaqnStorable, new()
        {
            T _pdc;
            _pdc = RaqnStorage.LoadJson<T>(SecureDataPath() + "/" + _filename, _persistent, _obfuscated, true);
            if (_pdc == null)
            {
                _pdc = new T();
                _pdc.Save<T>(_filename);
            }
            return _pdc;
        }

        public bool Save<T>(string _filename, bool _persistent = false, string _obfuscated = "") where T : RaqnStorable, new()
        {
            return RaqnStorage.SaveJson<T>(SecureDataPath() + "/" + _filename, (T)this, _persistent, _obfuscated, true);
        }

        public string ToJson(bool _pretty = false)
        {
            return RaqnSerializer.JsonSerialize(this, _pretty);
        }

        public override string ToString()
        {
            return base.ToString() + " " + ToJson(true);
        }

        static public T FromJson<T>(string _json) where T : RaqnStorable, new()
        {
            return RaqnSerializer.JsonDeserialize<T>(_json);
        }

        static private string SecureDataPath()
        {
            string _path = DataPath();
            if(!Directory.Exists(_path)) { Directory.CreateDirectory(_path); }
            return _path;
        }

        static protected string DataPath(bool _persistent = false)
        {
            return RaqnStorage.DataPath(_persistent) + "cache";
        }
    }
}