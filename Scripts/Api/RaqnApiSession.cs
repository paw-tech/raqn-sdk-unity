using System;
using System.Collections.Generic;
using FullSerializer;
using RAQN.Serialization;
using RAQN.Play;

namespace RAQN.Api
{

    [fsObject]
    public class RaqnApiSession 
    {
        [fsProperty]
        public string id;

        [fsProperty]
        public string started;
        
        [fsProperty]
        public string expires;

        [fsProperty]
        public List<RaqnLogin> logins = new List<RaqnLogin>();
        
        [fsProperty]
        public string play;

        public RaqnApiSession() : base()
        {
            id = "L$"+ System.Guid.NewGuid();
            started = RaqnTime.LocalNow();
            expires = RaqnTime.FOREVER;
            logins = new List<RaqnLogin>();
        }

        public override string ToString()
        {
            return base.ToString() + " " + RaqnSerializer.JsonSerialize(this,true);
        }

    }
}