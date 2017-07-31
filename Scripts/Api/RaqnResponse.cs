using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using FullSerializer;
using RAQN.Serialization;

namespace RAQN.Api
{
    [Serializable]
    public class RaqnResponse
    {
        [fsProperty]
        public string status;

        [fsProperty]
        public List<RaqnMsg> msg;

        [fsProperty]
        protected Dictionary<string,fsData> data;

        public RaqnResponse(string _status, List<RaqnMsg> _msg, Dictionary<string,fsData> _data)
        {
            status = _status;
            msg = _msg;
            data = _data;
        }

        public RaqnResponse() : base()
        {
            status = "OK";
            msg = new List<RaqnMsg>();
            data = new Dictionary<string,fsData>();
        }

        public bool IsError()
        {
            return ResponseStatus.IsError(status);
        }

        public bool TryGetData<T>(string _key, out T _value)
        {
            _value = default(T);
            if(!data.ContainsKey(_key)){ return false; }
            fsData _aux = data[_key];
            object _obj = null;
            fsResult _res = Serialization.RaqnSerializer.Instance.TryDeserialize(_aux, typeof(T), ref _obj).AssertSuccess();
            if (!_res.Failed)
            {
                _value = (T)_obj;
                return true;
            }
            return false;
        }

        public T TryGetValue<T>(string _key)
        {
            T _value = default(T);
            TryGetData<T>(_key,out _value);
            return _value;
        }

        static public RaqnResponse FromJson(string _json)
        {
            fsData _res_fsdata = fsJsonParser.Parse(_json);
            if (!_res_fsdata.IsDictionary)
            {
                return null;
            }

            Dictionary<string, fsData> _dict = _res_fsdata.AsDictionary;

            if (!_dict.ContainsKey("status") || !_dict.ContainsKey("msg") || !_dict.ContainsKey("data"))
            {
                return null;
            }

            string _res_status = _dict["status"].AsString;

            List<RaqnMsg> _res_msg = new List<RaqnMsg>();
            if (_dict["msg"].IsList)
            {
                foreach (fsData _fsmsg in _dict["msg"].AsList)
                {
                    RaqnMsg _msg = null;
                    RaqnSerializer.Instance.TryDeserialize<RaqnMsg>(_fsmsg, ref _msg);
                    if (_msg != null)
                    {
                        _res_msg.Add(_msg);
                    }
                }
            }

            Dictionary<string,fsData> _res_data = new Dictionary<string,fsData>();
            if(_dict["data"].IsDictionary)
            {
                _res_data = _dict["data"].AsDictionary;
            }

            RaqnResponse _res = new RaqnResponse(_res_status, _res_msg, _res_data);

            return _res;
        }


        public string ToJson(bool _pretty = false)
        {
            fsData _fsres = RaqnSerializer.TryGetSerializable<RaqnResponse>(this);
            //return _fsres.ToString();
            if (_pretty)
            {
                return fsJsonPrinter.PrettyJson(_fsres);
            }
            return fsJsonPrinter.CompressedJson(_fsres);
        }

        public string GetMessageText(int _i = 0)
        {
            if(_i < msg.Count) { return msg[_i].message; }
            return "";
        }

        public string GetMessageCode(int _i = 0)
        {
            if (_i < msg.Count) { return msg[_i].code; }
            return "";
        }

    }

    [fsObject]
    public class RaqnMsg
    {
        [fsProperty]
        public string code;

        [fsProperty]
        public string message;

        public RaqnMsg() : base() { }

        public RaqnMsg(string _code, string _text)
        {
            code = _code;
            message = _text;
        }
    }


    static class ResponseStatus
    {
        public const string OK = "OK";
        public const string ERROR = "ERROR";
        public const string REDIR = "REDIR";
        public const string REDIR_SOFT = "REDIR_SOFT";
        public const string ERR_REQUEST = "WRONG_REQUEST_FORMAT";
        public const string ERR_WRONG_METHOD = "WRONG_METHOD";
        public const string ERR_NOT_FOUND = "NOT_FOUND";
        public const string ERR_SESS_MISSING = "SESSION_MISSING";
        public const string ERR_SESS_EXPIRED = "SESSION_EXPIRED";
        public const string ERR_SESS_INVALID = "SESSION_INVALID";
        public const string ERR_LOGIN_FAILED = "LOGIN_FAILED";
        public const string ERR_NOT_AUTHORIZED = "NOT_AUTHORIZED";
        public const string ERR_PROCESSING = "ERROR_PROCESSING";
        public const string ERR_FORMATTING = "ERROR_FORMATTING";

        static public bool IsError(string _status)
        {
            return _status != "OK" && _status.Substring(0, 5) != "REDIR";
        }
    }
}