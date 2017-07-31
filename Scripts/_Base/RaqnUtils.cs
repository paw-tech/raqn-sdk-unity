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

    static class RaqnUtils
    {
        static public string RandomString(int _length)
        {
            var _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var _stringChars = new char[_length];
            var _random = new System.Random();

            for (int i = 0; i < _stringChars.Length; i++)
            {
                _stringChars[i] = _chars[_random.Next(_chars.Length)];
            }
            return new String(_stringChars);
        }

        static public Func<Tin, Tout> ComposeFunc<Tin, T, Tout>(Func<T, Tout> f, Func<Tin, T> g)
        {
            return (x => f(g(x)));
        }

        static public Action<Tin> ComposeAction<Tin, T>(Action<T> f, Func<Tin, T> g)
        {
            return (x => f(g(x)));
        }

        static public Action<Tin> ActionWrapper<Tin>(Action f)
        {
            return x => { if (f != null) { f(); } };
        }

    }
}