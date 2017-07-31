using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using RAQN;
using RAQN.Api;
using RAQN.Serialization;
using RAQN.Storage;
using RAQN.Play;
using FullSerializer;

namespace RAQN
{

    [fsObject]
    public class DixInfo
    {
        public string Id;

        public string Secret;
        public string Package;


        public static DixInfo Load()
        {
            TextAsset _dixtext = Resources.Load("raqn") as TextAsset;
            DixInfo _dix = null;
            if (_dixtext != null)
            {
                _dix = RaqnSerializer.JsonDeserialize<DixInfo>(_dixtext.text);
            }
            if (_dix == null)
            {
                Debug.Log("Cannot Load DixInfo.json!");
                Raqn.Quit();
            }
            return _dix;
        }
    }
}