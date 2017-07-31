using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Text;
using FullSerializer;
using RAQN;
using RAQN.Serialization;
using RAQN.Play;

namespace RAQN.Api
{

    public class RaqnApi
    {
        //static string endpoint = "http://localhost:8081";
        static string endpoint = "https://api.raqn.in";
        static bool debug = false;

        protected string client_id;
        protected string client_key;

        protected string auth;
        protected RaqnApiSession session;
        protected RaqnPlaySession playSession;

        public event Action OnStartSuccess;
        public event Action<string> OnStartError;

        public event Action OnLoginSuccess;
        public event Action<string> OnLoginError;

        public event Action OnPlaySuccess;
        public event Action<string> OnPlayError;

        public event Action OnPlayEndSuccess;
        public event Action<string> OnPlayEndError;

        public RaqnApi(string _client_id, string _client_key)
        {
            this.client_id = _client_id;
            this.client_key = _client_key;
        }

        protected string MakeAuthString(string _user_id = "", string _user_key = "")
        {
            if (_user_id == "" || _user_key == "")
            {
                return "C:" + this.client_id + ":" + this.client_key;
            }
            return "U:" + this.client_id + ":" + this.client_key + ":" + _user_id + ":" + _user_key;
        }

        static void ResponseFailure(RaqnRequest _req, string _error)
        {
            if (_req.OnError != null)
            {
                _req.OnError(_error);
            }
        }

        public IEnumerator SendRequest(RaqnRequest _req)
        {
            if (auth != null)
            {
                _req.SetAuth(auth);
            }
            //_req.OnSuccess += this.UpdateFromResponse;
            string _url = endpoint + _req.GetUri();
            if (debug)
            {
                Debug.Log("Making Api Request to Url = " + _url + "\n" + _req.GetSerializedBody(true));
            }
            string _str = _req.GetSerializedBody();
            byte[] _body = Encoding.UTF8.GetBytes(_str);
            UnityWebRequest _web_req = new UnityWebRequest(_url, UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw _uh = new UploadHandlerRaw(_body);
            _uh.contentType = "application/json";
            _web_req.uploadHandler = _uh;
            _web_req.downloadHandler = new DownloadHandlerBuffer();
            _web_req.SetRequestHeader("Content-Type", "application/json");

            yield return _web_req.Send();

            if (_web_req.isError)
            {
                ResponseFailure(_req, "LOCAL_ERR_CONNECTION");
                yield break;
            }

            string _json = _web_req.downloadHandler.text;
            if (debug) { Debug.Log("Received response. Returned " + _json); }

            RaqnResponse _res = RaqnResponse.FromJson(_json);
            if (_res == null)
            {
                ResponseFailure(_req, "LOCAL_ERR_PARSING");
                yield break;
            }
            
            _UpdateFromResponse(_res);
            if (ResponseStatus.IsError(_res.status))
            {
                if (_req.OnError != null)
                {
                    string _err_code = _res.status;
                    if (_res.GetMessageCode() != "")
                    {
                        _err_code = _res.GetMessageCode();
                    }
                    _req.OnError(_err_code);
                }
            }
            else if (_req.OnSuccess != null)
            {
                _req.OnSuccess(_res);
            }
        }

        public Coroutine Start(string _username = "", string _password = "")
        {
            RaqnRequest _req = new RaqnRequest("/start");
            _req.SetAuth(MakeAuthString(_username, _password));
            _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnStartSuccess);
            _req.OnError += OnStartError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine LoginUser(string _username, string _password)
        {
            RaqnRequest _req = new RaqnRequest("/user/login");
            if (session == null)
            {
                _req.SetAuth(MakeAuthString(_username, _password));
            }
            _req.SetField("user", MakeAuthString(_username, _password));
            _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnLoginSuccess);
            _req.OnError += OnLoginError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine StartPlay(string _dix_id, string _pkg_id)
        {
            RaqnRequest _req = new RaqnRequest("/play/start");
            _req.SetField("dix", _dix_id);
            _req.SetField("package", _pkg_id);
            _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnPlaySuccess);
            _req.OnError += OnPlayError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine EndPlay()
        {
            RaqnRequest _req = new RaqnRequest("/play/end");
            _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnPlayEndSuccess);
            _req.OnError += OnPlayEndError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine LicenseCurrent(string _dix_id, Action<RaqnLicense> OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/dix/" + _dix_id + "/license/play");
            _req.OnSuccess += RaqnUtils.ComposeAction<RaqnResponse,RaqnLicense>(OnReady,_ResponseParser<RaqnLicense>("license"));
            _req.OnError += OnError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine LicenseOptions(string _dix_id, Action<List<RaqnLicenseTemplate>> OnReady)
        {
            RaqnRequest _req = new RaqnRequest("/dix/" + _dix_id + "/license/options");
            //_req.SetField("dix", _dix_id);
            _req.OnSuccess += RaqnUtils.ComposeAction<RaqnResponse, List<RaqnLicenseTemplate>>(OnReady, _ResponseParser<List<RaqnLicenseTemplate>>("licenses"));
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine LicenseRequest(string _dix_id, string _lic_id, Action OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/dix/" + _dix_id + "/license/request/" + _lic_id);
            _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnReady);
            _req.OnError += OnError;
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public RaqnApiSession GetSession()
        {
            return this.session;
        }

        public RaqnPlaySession GetPlaySession()
        {
            return this.playSession;
        }

        public Coroutine SendPlayData(RaqnData _pd, string _user, Action OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/play/data/update");
            _req.SetField<string>("user", _user);
            _req.SetField<string>("format", "RAW");
            _req.SetField<RaqnData>("data", _pd);
            if (OnReady != null)
            {
                _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnReady);
            }
            if (OnError != null)
            {
                _req.OnError += OnError;
            }
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine SendPlayEvents(List<RaqnPlayEvent> _el, string _user, Action OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/play/events/update");
            _req.SetField<string>("user", _user);
            _req.SetField<string>("format", "RAW");
            _req.SetField<List<RaqnPlayEvent>>("events", _el);
            if (OnReady != null)
            {
                _req.OnSuccess += RaqnUtils.ActionWrapper<RaqnResponse>(OnReady);
            }
            if (OnError != null)
            {
                _req.OnError += OnError;
            }
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine SyncLocalSession(RaqnPlaySession _ps, Action<RaqnPlaySession> OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/play/sync/local");
            _req.SetField<RaqnPlaySession>("playsession", _ps);
            _req.SetField<string>("time", RaqnTime.LocalNow());
            if (OnReady != null)
            {
                _req.OnSuccess += RaqnUtils.ComposeAction<RaqnResponse, RaqnPlaySession>(OnReady, _ResponseParser<RaqnPlaySession>("_playsession"));
            }
            if (OnError != null)
            {
                _req.OnError += OnError;
            }
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public Coroutine SyncPlaySession(RaqnPlaySession _ps, Action<RaqnPlaySession> OnReady = null, Action<string> OnError = null)
        {
            RaqnRequest _req = new RaqnRequest("/play/sync");
            _req.SetField<RaqnPlaySession>("playsession", _ps);
            _req.SetField<string>("time",RaqnTime.LocalNow());
            if (OnReady != null)
            {
                _req.OnSuccess += RaqnUtils.ComposeAction<RaqnResponse, RaqnPlaySession>(OnReady, _ResponseParser<RaqnPlaySession>("_playsession"));
            }
            if (OnError != null)
            {
                _req.OnError += OnError;
            }
            return Raqn.Instance.StartCoroutine(SendRequest(_req));
        }

        public RaqnUser GetUser(int _index = 0)
        {
            if (this.session == null) { return null; }
            if (this.session.logins.Count <= _index) { return null; }
            return this.session.logins[_index].user;
        }

        protected void _UpdateFromResponse(RaqnResponse _res)
        {
            string _auth;
            RaqnApiSession _session;
            RaqnPlaySession _playSession;
            if (_res.TryGetData<string>("_auth", out _auth))
            {
                this.auth = _auth;
            }
            if (_res.TryGetData<RaqnApiSession>("_session", out _session))
            {
                this.session = _session;
            }
            if (_res.TryGetData<RaqnPlaySession>("_playsession", out _playSession))
            {
                this.playSession = _playSession;
            }
        }

        protected Func<RaqnResponse, T> _ResponseParser<T>(string _key)
        {
            return (_res => _res.TryGetValue<T>(_key));
        }

        /*protected List<RaqnLicenseTemplate> _ParseLicenseTemplates(RaqnResponse _res)
        {
            List<RaqnLicenseTemplate> _lt;
            _res.TryGetData<List<RaqnLicenseTemplate>>("licenses", out _lt);
            return _lt;
        }*/

    }
}