using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using RAQN.Serialization;
using RAQN.Storage;
using FullSerializer;

namespace RAQN.Api
{
    
    [fsObject]
    public class RaqnLicense
    {
        public string id;
        public string expires;
        public List<string> perms = new List<string>();

        public int energy = 0;
        public int per_session = 0;
        public int per_minute = 0;

        public bool HasPermission(string _permission)
        {
            return perms.IndexOf(_permission) >= 0;
        }

        public bool IsExpired()
        {
            return RaqnTime.ServerNow().CompareTo(expires) > 0;
        }

        public bool IsDepleted() 
        {
            return energy <= 0;
        }

    }

    public class RaqnLicenseTemplate
    {
        public string id;
        public string type;
        public string name;
        public string description;
        public string[] platforms;
        public int uses;
        public string[] perms;
        public int coins;
        public Money price;
    }

    public class Money
    {
        public string currency;
        public float amount;
    }
}