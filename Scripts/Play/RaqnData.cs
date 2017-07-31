using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using FullSerializer;
using RAQN.Serialization;
using RAQN.Storage;

namespace RAQN.Play
{

    [fsObject]
    public class RaqnData : RaqnStorable
    {
        [SerializeField]
        protected Dictionary<string, string> INFO;
        [SerializeField]
        protected Dictionary<string, bool> FLAGS;
        [SerializeField]
        protected Dictionary<string, double> STATS;

        public RaqnData() : base()
        {
            INFO = new Dictionary<string, string>();
            FLAGS = new Dictionary<string, bool>();
            STATS = new Dictionary<string, double>();
        }

        public bool UpdateFlag(string _key, bool _value)
        {
            FLAGS[_key] = _value;
            return true;
        }

        public bool UpdateFlagToggle(string _key)
        {
            if (!FLAGS.ContainsKey(_key))
            {
                return UpdateFlag(_key, false);
            }
            FLAGS[_key] = !FLAGS[_key];
            return true;
        }

        public bool UpdateInfo(string _key, string _value)
        {
            INFO[_key] = _value;
            return true;
        }

        public bool UpdateStat(string _key, double _value, int _round = 2)
        {
            STATS[_key] = Math.Round(_value, _round);
            return true;
        }

        public bool UpdateStatDelta(string _key, double _value)
        {
            if (!STATS.ContainsKey(_key))
            {
                return UpdateStat(_key, _value);
            }
            return UpdateStat(_key, STATS[_key] + _value);
        }

        public bool UpdateStatMin(string _key, double _value)
        {
            if (!STATS.ContainsKey(_key))
            {
                return UpdateStat(_key, _value);
            }
            return UpdateStat(_key, Math.Min(STATS[_key], _value));
        }

        public bool UpdateStatMax(string _key, double _value)
        {
            if (!STATS.ContainsKey(_key))
            {
                return UpdateStat(_key, _value);
            }
            return UpdateStat(_key, Math.Max(STATS[_key], _value));
        }

        public double GetStat(string _key)
        {
            double _value = default(double);
            if (!STATS.ContainsKey(_key))
            {
                return _value;
            }
            return STATS[_key];
        }

        public int GetStatInt(string _key)
        {
            return (int)GetStat(_key);
        }

        public string GetInfo(string _key)
        {
            string _value = default(string);
            if (!INFO.ContainsKey(_key))
            {
                return _value;
            }
            return INFO[_key];
        }

        public bool GetFlag(string _key)
        {
            bool _value = default(bool);
            if (!FLAGS.ContainsKey(_key))
            {
                return _value;
            }
            return FLAGS[_key];
        }
    }
}
