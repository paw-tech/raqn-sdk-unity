using UnityEngine;
using System;
using System.Collections;
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
    public class Raqn : MonoBehaviour
    {
        public static Raqn Instance;
        public static DixInfo Dix;
        public static RaqnApi Api;
        protected static RaqnApiSession ApiSession;
        protected static RaqnPlaySession PlaySession;
        protected static RaqnLicense local_license;
        protected static RaqnLogin local_login;

        public static RaqnData GameData;
        public static RaqnData GameState;

        static public event Action OnLogin;
        static public event Action<string> OnLoginError;
        static public event Action OnLicenseOK;
        static public event Action<string> OnLicenseError;
        static public event Action OnPlay;
        static public event Action<string> OnPlayError;
        static public event Action OnSync;
        static public event Action<string> OnSyncError;
        static public event Action OnBeforeQuit;
        static public event Action OnQuitReady;

        static protected int CurrentPlayer = 0;

        static private bool quit_ready = false;

        public static RaqnUser ApiUser
        {
            get
            {
                if (ApiSession == null) { return null; }
                if (ApiSession.logins.Count == 0) { return null; }
                return ApiSession.logins[0].user;
            }
        }
        public static RaqnLicense License
        {
            get
            {
                return local_license;
            }
        }

        public static int PlayerCount
        {
            get
            {
                if (PlaySession == null) { return 0; }
                return PlaySession.players.Count;
            }
        }

        public static RaqnPlayer Player(int _index = -1)
        {
            if (PlaySession == null) { return null; }
            if (PlaySession.players.Count == 0) { return null; };
            if (_index == -1) { _index = CurrentPlayer; }
            if (PlaySession.players.Count < _index) { return null; }
            return PlaySession.players[_index];
        }

        public static RaqnPlayDataController PlayData(int _player_index = -1)
        {
            if (PlaySession == null) { return new RaqnPlayDataController(); }
            if (Player(_player_index) == null) { return new RaqnPlayDataController(); };
            return new RaqnPlayDataController(Player().id, PlaySession);
        }

        void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
                Initialize();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected void Initialize()
        {
            Dix = DixInfo.Load();
            try
            {
                GameData = RaqnData.LoadOrCreate<RaqnData>(Dix.Id + ".json", true);
                //GeneralStats = StatContainer.LoadOrDie("General");
            }
            catch (Exception _ex)
            {
                Debug.LogException(_ex);
                Debug.LogError("Could not load DixData. Quitting app! (Exception :" + _ex.Message + ")");
                RealQuit();
            }
            Api = new RaqnApi(Dix.Id, Dix.Secret);
            Api.OnStartSuccess += OnLogin_Success;
            Api.OnStartError += OnLogin_Error;
            Api.OnLoginSuccess += OnLogin_Success;
            Api.OnLoginError += OnLogin_Error;
            Api.OnPlaySuccess += OnPlay_Success;
            Api.OnPlayError += OnPlay_Error;

            ApiSession = new RaqnApiSession();

            local_login = RaqnStorage.LoadJson<RaqnLogin>("cache/login.dat", false, Dix.Secret);
            local_license = RaqnStorage.LoadJson<RaqnLicense>("cache/license.dat", false, Dix.Secret);
        }

        static public void TryLogin(string _username, string _password)
        {
            Api.Start(_username, _password);
        }

        protected void OnLogin_Success()
        {
            ApiSession = Api.GetSession();
            if (ApiUser == null || ApiSession == null)
            {
                OnLogin_Error(ResponseStatus.ERR_FORMATTING);
                return;
            }
            RaqnTime.SetSyncDelta(ApiSession.started);
            local_login = ApiSession.logins[0];
            RaqnStorage.SaveJson<RaqnLogin>("cache/login.dat", local_login, false, Dix.Secret);
            Api.LicenseCurrent(Dix.Id, (lic) =>
            {
                OnLicense_Success(lic);
            }, (_err) =>
            {
                OnLicense_Error(_err);
            });
            if (OnLogin != null) { OnLogin(); }
        }

        protected void OnLogin_Error(string _error)
        {
            if (OnLoginError != null) { OnLoginError(_error); }
        }

        protected void OnLicense_Success(RaqnLicense lic)
        {
            local_license = lic;
            if (local_license.IsDepleted())
            {
                OnLicense_Error("LIC_DEPLETED");
            }
            else if (local_license.IsExpired())
            {
                OnLicense_Error("LIC_EXPIRED");
            }
            else
            {
                if (OnLicenseOK != null)
                {
                    OnLicenseOK();
                }
            }
        }

        protected void OnLicense_Error(string _error)
        {
            if (OnLicenseError != null)
            {
                OnLicenseError(_error);
            }
        }

        protected void OnPlay_Success()
        {
            //ApiSession = Api.GetSession();
            PlaySession = Api.GetPlaySession();
            if (PlaySession == null)
            {
                OnPlay_Error(ResponseStatus.ERR_FORMATTING);
            }
            //init session data for all players
            PlaySession.InitSessionData(ApiSession.id, Dix.Id, Dix.Package);
            RaqnStorage.SaveJson<RaqnLicense>("cache/license.dat", local_license, false, Dix.Secret);

            ExpendEnergy(License.per_session);
            GameData.UpdateStatDelta("RAQN.TotalUses", 1);
            OnSync += () =>
            {
                Api.LicenseCurrent(Dix.Id, (lic) =>
                {
                    local_license = lic;
                });
            };
            SyncJob();
            if (OnPlay != null) { OnPlay(); }
        }

        protected void OnPlay_Error(string _error)
        {
            if (OnPlayError != null) { OnPlayError(_error); }
        }

        static public RaqnLogin CurrentLogin()
        {
            return local_login;
        }

        static public bool StartPlay()
        {
            Api.StartPlay(Dix.Id, Dix.Package);
            return true;
        }

        static public bool StartOfflinePlay()
        {
            if (PlaySession != null || local_login == null || local_license == null || local_license.energy <= local_license.per_session)
            {
                return false;
            }
            if (ApiSession == null)
            {
                ApiSession = new RaqnApiSession();
                ApiSession.logins.Add(local_login);
            }
            if (PlaySession == null)
            {
                PlaySession = new RaqnPlaySession();
                PlaySession.dix = Dix.Id;
                PlaySession.license = local_license.id;
                RaqnPlayer _p = new RaqnPlayer();
                _p.id = local_login.user.id;
                _p.nickname = local_login.user.profile.nickname;
                _p.avatar = local_login.user.profile.avatar;
                PlaySession.players.Add(_p);
                PlaySession.InitSessionData(ApiSession.id, Dix.Id, Dix.Package);
                ApiSession.play = PlaySession.id;
            }
            ExpendEnergy(License.per_session);
            RaqnStorage.SaveJson<RaqnLicense>("cache/license.dat", local_license, false, Dix.Secret);
            return true;
        }

        static public bool ExpendEnergy(int _energy)
        {
            if (License == null) { return false; }
            if (License.energy >= _energy)
            {
                License.energy -= _energy;
                PlaySession.expense += _energy;
                return true;
            }
            return false;
        }

        void OnApplicationQuit()
        {
            if (!quit_ready)
            {
                UnityEngine.Application.CancelQuit();
                Quit();
            }
        }

        static public void Quit()
        {
            if (OnBeforeQuit != null) { OnBeforeQuit(); }
            GameData.UpdateStatDelta("RAQN.TotalPlaytime", UnityEngine.Time.time);
            GameData.Save<RaqnData>(Dix.Id + ".json");
            if (PlaySession != null)
            {
                PlaySession.End(() =>
                {
                    if (OnQuitReady != null)
                    {
                        quit_ready = true;
                        OnQuitReady();
                    }
                    else { RealQuit(); }
                }, (_err) =>
                 {
                     PlaySession.Save<RaqnPlaySession>(Dix.Id + "_" + PlaySession.id + ".json");
                     RealQuit();
                 });
            }
            else
            {
                RealQuit();
            }
            //SendSessionData();
        }

        static public void SyncNow()
        {
            if (PlaySession == null) { return; }
            PlaySession.Sync(OnSync, OnSyncError);
        }

        static private void RealQuit()
        {
            quit_ready = true;
            UnityEngine.Application.Quit();
        }

        protected IEnumerator SyncJob()
        {
            while (true)
            {
                yield return new WaitForSeconds(60);
                SyncNow();
            }
        }

        void DebugPrintVars()
        {
            //Este metodo debuggea las variables de sesión
            Debug.Log("Printing Raqn Vars");
            Debug.Log("DixInfo = " + RaqnSerializer.JsonSerialize(Dix, true));
            Debug.Log("ApiUser = " + RaqnSerializer.JsonSerialize(ApiUser, true));
            Debug.Log("ApiSession = " + RaqnSerializer.JsonSerialize(ApiSession, true));
            Debug.Log("GameData = " + RaqnSerializer.JsonSerialize(GameData, true));
            if (License != null) { Debug.Log("License = " + RaqnSerializer.JsonSerialize(License, true)); }
        }
    }


    public class RaqnPlayDataController
    {
        string player_id;
        RaqnPlaySession playsession;

        public RaqnPlayDataController()
        {
            player_id = null;
            playsession = null;
        }

        public RaqnPlayDataController(string _player_id, RaqnPlaySession _playsession)
        {
            player_id = _player_id;
            playsession = _playsession;
        }

        protected bool IsValid()
        {
            return player_id != null && playsession != null;
        }

        public bool Event(string _event_name)
        {
            if (!IsValid()) { return false; }
            RaqnPlayEvent _ev = new RaqnPlayEvent(_event_name);
            playsession.AddEvent(_ev, player_id);
            return true;
        }

        public bool Event(string _event_name, float _value)
        {
            if (!IsValid()) { return false; }
            RaqnPlayEvent _ev = new RaqnPlayEvent(_event_name, _value);
            playsession.AddEvent(_ev, player_id);
            return true;
        }

        public bool Event(string _event_name, string _label)
        {
            if (!IsValid()) { return false; }
            RaqnPlayEvent _ev = new RaqnPlayEvent(_event_name, _label);
            playsession.AddEvent(_ev, player_id);
            return true;
        }

        public bool Event(string _event_name, string _label, float _value)
        {
            if (!IsValid()) { return false; }
            RaqnPlayEvent _ev = new RaqnPlayEvent(_event_name, _label, _value);
            playsession.AddEvent(_ev, player_id);
            return true;
        }

        bool FlagSet(string _key, bool _value)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateFlag(_key, _value);
        }

        public bool FlagToggle(string _key)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateFlagToggle(_key);
        }

        public bool InfoUpdate(string _key, string _value)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateInfo(_key, _value);
        }

        public bool StatSet(string _key, double _value, int _round = 2)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateStat(_key, _value);
        }

        public bool StatUpdateDelta(string _key, double _value)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateStatDelta(_key, _value);
        }

        public bool StatUpdateMin(string _key, double _value)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateStatMin(_key, _value);
        }

        public bool StatUpdateMax(string _key, double _value)
        {
            if (!IsValid()) { return false; }
            return playsession.data[player_id].UpdateStatMax(_key, _value);
        }
    }
}