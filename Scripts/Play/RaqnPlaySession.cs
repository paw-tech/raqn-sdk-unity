using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using RAQN;
using RAQN.Api;
using RAQN.Play;
using RAQN.Storage;
using FullSerializer;

namespace RAQN.Play
{
    [fsObject]
    public class RaqnPlaySession : RaqnStorable
    {
        public string id;
        public string dix;
        public string start;
        public string end;
        public List<RaqnPlayer> players = new List<RaqnPlayer>();
        public string license = null;
        public Dictionary<string, RaqnData> data = new Dictionary<string, RaqnData>();
        public List<RaqnPlayEvent> events = new List<RaqnPlayEvent>();
        protected float local_engine_start;
        public int expense = 0;
        [fsIgnore]
        protected bool is_synced = true;

        public RaqnPlaySession() : base()
        {
            id = "L$" + System.Guid.NewGuid();
            start = RaqnTime.LocalNow();
            local_engine_start = RaqnTime.LocalEngineNow();
        }

        public float SessionTimeNow()
        {
            return RaqnTime.LocalEngineNow() - local_engine_start;
        }

        public void AddEvent(RaqnPlayEvent _e, string _player = null)
        {
            if (_player == null) { _player = Raqn.Player().id; }
            _e.SetTime(SessionTimeNow());
            _e.SetPlayer(_player);
            events.Add(_e);
            is_synced = false;
        }

        public void Sync(Action OnReady, Action<string> OnError)
        {
            if (IsOffline())
            {
                Raqn.Api.SyncLocalSession(this, (_ps) =>
                {
                    if (_ps == null) { if (OnError != null) { OnError("NULL_RESPONSE"); } return; }
                    this.id = _ps.id;
                    this.dix = _ps.dix;
                    this.start = _ps.start;
                    this.end = _ps.end;
                    this.players = _ps.players;
                    this.events.Clear();
                    this.expense = 0;
                    this.is_synced = true;
                    if (OnReady != null) { OnReady(); }
                }, OnError);
                return;
            }
            else
            {
                Raqn.Api.SyncPlaySession(this, (_ps) =>
                 {
                     if (_ps == null) { if (OnError != null) { OnError("NULL_RESPONSE"); } return; }
                     this.dix = _ps.dix;
                     this.players = _ps.players;
                     this.events.Clear();
                     this.is_synced = true;
                     this.expense = 0;
                     if (OnReady != null) { OnReady(); }
                 }, OnError);
            }
        }

        public void End(Action OnReady = null, Action<string> OnError = null)
        {
            if (IsOffline())
            {
                end = RaqnTime.LocalNow();
                Sync(OnReady, OnError);
            }
            else
            {
                Raqn.Api.OnPlayEndSuccess += () =>
                {
                    Sync(OnReady, OnError);
                };
                Raqn.Api.OnPlayEndError += (_err) =>
                {
                    Sync(OnReady, OnError);
                };
                Raqn.Api.EndPlay();
            }
        }

        public bool IsOffline()
        {
            return this.id.IndexOf("L$") == 0;
        }

        public void InitSessionData(string _sess_id, string _dix_id, string _pkg_id)
        {
            foreach (RaqnPlayer _player in players)
            {
                RaqnData _data = new RaqnData();
                _data.UpdateInfo("RAQN.ApiSession", _sess_id);
                _data.UpdateInfo("RAQN.Dix", _dix_id);
                _data.UpdateInfo("RAQN.Package", _pkg_id);
                data.Add(_player.id, _data);
            }
            if (IsOffline())
            {
                is_synced = false;
            }
        }

        new static protected string DataPath(bool _persistent = false)
        {
            return RaqnStorage.DataPath(_persistent) + "cache/local";
        }
    }

    [fsObject]
    public class RaqnPlayer
    {
        public string id;
        public string avatar;
        public string nickname;
    }
}