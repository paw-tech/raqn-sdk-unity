using System;
using System.Collections.Generic;
using FullSerializer;

namespace RAQN.Serialization
{
    static class RaqnSerializer
    {
        static private fsSerializer serializer;
        static public fsSerializer Instance { get {
                if (serializer == null)
                {
                    serializer = new fsSerializer();
                    //serializer.AddConverter(new ApiRequestConverter());
                }
                return serializer;
            } }

        static public fsData TryGetSerializable<T>(T _obj)
        {
            fsData _data;
            Instance.TrySerialize<T>(_obj, out _data);
            return _data;
        }

        static public string JsonSerialize(object _obj, bool _pretty = false)
        {
            fsData _data;
            Instance.TrySerialize(_obj.GetType(), _obj, out _data).AssertSuccessWithoutWarnings();

            if (_pretty)
            {
                return fsJsonPrinter.PrettyJson(_data);
            }
            return fsJsonPrinter.CompressedJson(_data);
        }

        static public T JsonDeserialize<T>(string _json)
        {
            if(_json.Trim() == ""){ return default(T); }
            // step 1: parse the JSON data
            fsData _data = fsJsonParser.Parse(_json);

            // step 2: deserialize the data
            object deserialized = null;
            Instance.TryDeserialize(_data, typeof(T), ref deserialized).AssertSuccess();
            
            return (T)deserialized;
        }

        public static string ObfuscateXOR(string _codeword, String _text)
        {
            string _res = "";
            for (int i = 0; i < _text.Length; i++)
            {
                _res += (char)(_text[i] ^ _codeword[i % _codeword.Length]);
            }
            return _res;
        }
    }
}

