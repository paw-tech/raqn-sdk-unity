using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using FullSerializer;
using RAQN.Serialization;

namespace RAQN.Api
{

    //public delegate void SuccessDelegate(RaqnResponse _res);
    //public delegate void FailureDelegate(string _error_msg);

    [Serializable]
    public class RaqnRequest
    {
        [SerializeField]
        protected string uri;
        [SerializeField]

        protected Dictionary<string,fsData> body;

        public Action<RaqnResponse> OnSuccess;
        public Action<string> OnError;

        public RaqnRequest(string _uri)
        {
            uri = _uri;
            body = new Dictionary<string,fsData>();
        }

        public void SetAuth(string _auth_string)
        {
            if(body.ContainsKey("_auth")){ return; }
            body.Add("_auth",new fsData(_auth_string));
        }

        public bool SetField<T>(string _field_key, T _field_value)
        {
            RaqnSerializer.JsonSerialize(_field_value);
            fsData _field = RaqnSerializer.TryGetSerializable<T>(_field_value);
            if(_field.IsNull){ return false; }
            body.Add(_field_key,_field);
            return true;
        }

        public string GetUri()
        {
            return this.uri;
        }

        public fsData GetBody()
        {
            return new fsData(this.body);
        }

        public string GetSerializedBody(bool _pretty = false)
        {
            fsData _req = this.GetBody();
            if (!_pretty)
            {
                return fsJsonPrinter.CompressedJson(_req);
            }
            else
            {
                return fsJsonPrinter.PrettyJson(_req);
            }
        }
    }
}