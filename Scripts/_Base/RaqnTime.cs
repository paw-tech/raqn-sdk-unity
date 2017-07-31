using System;
using UnityEngine;

namespace RAQN
{
    public class RaqnTime
    {
        public const string NEVER = "0000-01-01T00:00:00.000Z";
        public const string FOREVER = "9999-12-31T00:00:00.000Z";
        //Local Time - Server Time
        public static float sync_delta = 0.0f;

        public static DateTime FromIsoString(string _iso)
        {
            return DateTime.Parse(_iso);
            //return DateTime.Parse(_iso, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        public static string ToIsoString(DateTime _dt)
        {
            return _dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static string LocalNow()
        {
            return ToIsoString(DateTime.UtcNow);
        }

        public static string ServerNow()
        {
            DateTime _local_now = RaqnTime.FromIsoString(RaqnTime.LocalNow());
            return RaqnTime.ToIsoString(_local_now.AddSeconds(-sync_delta));
        }

        static public float LocalEngineNow()
        {
            return (float)Math.Round(UnityEngine.Time.time, 2);
        }

        static public float ServerEngineNow()
        {
            return LocalEngineNow() - sync_delta;
        }

        public static void SetSyncDelta(string _server_now)
        {
            DateTime _serv_now = RaqnTime.FromIsoString(_server_now);
            DateTime _local_now = RaqnTime.FromIsoString(RaqnTime.LocalNow());

            TimeSpan _diff = _local_now.Subtract(_serv_now);
            float _delta = (float)Math.Round(_diff.TotalSeconds, 2);
            SetSyncDelta(_delta);
        }
        public static void SetSyncDelta(float _delta)
        {
            sync_delta = _delta;
        }

        public static bool IsNever(string _time)
        {
            return _time == NEVER;
        }

        public static bool IsForever(string _time)
        {
            return _time == FOREVER;
        }
    }
}