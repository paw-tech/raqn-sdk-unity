using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RAQN;
using RAQN.Play;
using RAQN.Api;
using RAQN.App;
using RAQN.Serialization;
using RAQN.Storage;
using TMPro;
using UnityEngine.EventSystems;

namespace RAQN
{
    class RaqnMenuCtrl : RaqnScene
    {
        static protected RaqnMenuCtrl Instance;

        static public RaqnMenuCtrl Manager
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new RaqnMenuCtrl();
                }
                return Instance;
            }
        }

        [SerializeField]
        protected GameObject gui;

        [SerializeField]
        protected TextMeshProUGUI gui_message;

        [SerializeField]
        public InputField gui_input_user;

        [SerializeField]
        public InputField gui_input_pass;

        [SerializeField]
        public Button gui_button_login;

        [SerializeField]
        public Button gui_button_play;

        [SerializeField]
        public Button gui_button_continue;

        [SerializeField]
        public Button gui_button_change;

        [SerializeField]
        public Button gui_button_offline;

        [SerializeField]
        public GameObject gui_loading;

        [SerializeField]
        public GameObject gui_dialog;

        [SerializeField]
        public Image gui_avatar;

        [SerializeField]
        public TextMeshProUGUI gui_dialog_msg;

        protected RaqnLogin login;

        protected event Action<bool> dialogAction;

        void Awake()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Return))
            {
                if(EventSystem.current.currentSelectedGameObject == gui_input_pass.gameObject || EventSystem.current.currentSelectedGameObject == gui_input_user.gameObject)
                {
                    ButtonLogin();
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            Raqn.OnLogin += State_LoginOK;
            Raqn.OnLoginError += State_LoginFailed;
            Raqn.OnLicenseOK += State_PlayReady;
            Raqn.OnLicenseError += State_LicError;
            Raqn.OnPlay += State_NextScene;
            Raqn.OnPlayError += State_PlayError;
            State_Starting();
        }

        public void State_Starting()
        {
            GUI_Hide();
            gui_loading.gameObject.SetActive(true);
            GUI_SetMessage("Inicializando...");
            login = Raqn.CurrentLogin();
            if (login != null)
            {
                State_ShowLogin();
            }
            else
            {
                State_AskLogin("Usa tu cuenta RAQN");
            }
        }

        public void State_ShowLogin()
        {
            GUI_Hide();
            GUI_SetMessage("");
            gui_avatar.gameObject.SetActive(true);
            gui_avatar.gameObject.GetComponent<RaqnAvatar>().SetAvatar(login.user.profile.avatar);
            gui_avatar.GetComponentInChildren<TextMeshProUGUI>().text = "@" + login.user.profile.nickname;
            gui_button_continue.gameObject.SetActive(true);
            gui_button_change.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gui_button_continue.gameObject); //para poder navegar los botones con el teclado
        }

        public void State_AskLogin(string _msg, bool _append = false)
        {
            GUI_Hide();
            if (_append)
            {
                _msg = "\n" + _msg;
            }
            GUI_SetMessage(_msg, _append);
            gui_input_user.gameObject.SetActive(true);
            gui_input_pass.gameObject.SetActive(true);
            gui_button_login.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gui_input_user.gameObject); //para poder navegar los botones con el teclado

        }

        public void ButtonLogin()
        {
            string _user = gui_input_user.text;
            string _pass = gui_input_pass.text;
            Raqn.TryLogin(_user, _pass);
            gui_loading.gameObject.SetActive(false);
        }

        public void ButtonContinueLogin()
        {
            GUI_Hide();
            Raqn.TryLogin(login.user.id, login.token);
            gui_loading.gameObject.SetActive(true);
        }

        public void ButtonChange()
        {
            State_AskLogin("Ingresa tus credenciales");
        }

        public void State_LoginOK()
        {
            GUI_SetMessage("Login Exitoso!\nVerificando Licencia...");
            GUI_Hide();
            _RestoreOfflineSessions();
            gui_loading.gameObject.SetActive(true);
        }

        public void State_LoginFailed(string _error)
        {
            if (_error == "LOCAL_ERR_CONNECTION")
            {
                if (login != null)
                {
                    if (Raqn.License != null)
                    {
                        if (Raqn.License.HasPermission("OFFLINE"))
                        {
                            State_OfflineSession();
                            return;
                        }
                        State_AskLogin("Tu licencia no permite jugar offline.\nIntenta conectándote a Internet");
                        return;
                    }
                }
                State_AskLogin("Error de Licencia. Chequea tu conexión a internet");
            }
            else
            {
                State_AskLogin("Intento de Login Fallido!\nIntenta Nuevamente");
            }
        }

        public void ButtonPlay()
        {
            GUI_Hide();
            GUI_SetMessage("Cargando...");
            gui_avatar.gameObject.SetActive(true);
            gui_avatar.gameObject.GetComponent<RaqnAvatar>().SetAvatar(login.user.profile.avatar);
            gui_avatar.GetComponentInChildren<TextMeshProUGUI>().text = "@" + login.user.profile.nickname;
            gui_loading.gameObject.SetActive(true);
            Raqn.StartPlay();
        }

        public void ButtonOffline()
        {
            if (Raqn.StartOfflinePlay())
            {
                RaqnApp.App.ChangeScene(1);
            }
        }

        public void State_LicError(string _error)
        {
            if (_error == "LIC_NOT_FOUND")
            {
                GUI_SetMessage("Error de Licencia");
                Raqn.Api.LicenseOptions(Raqn.Dix.Id, _FindFreeLicense);
            }
            else if (_error == "LIC_NOT_RENEWABLE")
            {
                State_AskLogin("La licencia seleccionada no es renovable\nPrueba usando otra cuenta");
            }
            else
            {
                State_AskLogin("Error obteniendo licencia.\nPrueba usando otra cuenta");
            }
        }

        public void State_AcquireLicense(string lic_id)
        {
            GUI_Hide();
            GUI_SetMessage("Obteniendo licencia...");
            gui_loading.gameObject.SetActive(true);
            Raqn.Api.LicenseRequest(Raqn.Dix.Id, lic_id, State_PlayReady, State_LicError);
        }

        public void State_PlayReady()
        {
            GUI_SetMessage("Listo para jugar!");
            GUI_Hide();
            login = Raqn.CurrentLogin();
            gui_avatar.gameObject.GetComponent<RaqnAvatar>().SetAvatar(login.user.profile.avatar);
            gui_avatar.GetComponentInChildren<TextMeshProUGUI>().text = "@" + login.user.profile.nickname;
            gui_avatar.gameObject.SetActive(true);
            gui_button_play.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(gui_button_play.gameObject);
        }

        public void State_PlayError(string _error)
        {
            GUI_Hide();
            if (_error == "LIC_NOT_FOUND")
            {
                GUI_SetMessage("Error de Licencia");
                Raqn.Api.LicenseOptions(Raqn.Dix.Id, _FindFreeLicense);
            }
            else
            {
                State_AskLogin("Error conectando con RAQN.\nIntenta Nuevamente");
            }
        }

        public void State_OfflineSession()
        {
            GUI_Hide();
            GUI_SetMessage("Error de Login. ¿Jugar Offline?");
            gui_avatar.gameObject.SetActive(true);
            gui_button_change.gameObject.SetActive(true);
            gui_button_offline.gameObject.SetActive(true);
            gui_button_offline.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Offline (" + Raqn.License.energy + ")";
        }

        public void State_NextScene()
        {
            RaqnApp.App.ChangeScene(1);
        }

        public void _RestoreOfflineSessions()
        {
            string[] _paths = Directory.GetFiles(RaqnStorage.DataPath() + "cache");
            for (int i = 0; i < _paths.Length; ++i)
            {
                string _dir = Path.GetDirectoryName(_paths[i]);
                string _name = _paths[i].Substring(_dir.Length + 1);
                if (_name.StartsWith(Raqn.Dix.Id + "_L$") && _name.EndsWith(".json"))
                {
                    RaqnPlaySession _pl = RaqnPlaySession.Load<RaqnPlaySession>(_name);
                    if (_pl == null)
                    {
                        File.Delete(_paths[i]);
                    }
                    else
                    {
                        _pl.Sync(() =>
                        {
                            Debug.Log("Playsession synced OK!");
                            File.Delete(_paths[i]);
                        }, (_err) =>
                        {
                            Debug.Log("Error syncing playsession, until next time!");
                        });
                    }
                }
            }
        }


        public void _FindFreeLicense(List<RaqnLicenseTemplate> tlist)
        {
            foreach (RaqnLicenseTemplate lt in tlist)
            {
                if (lt.price.amount <= 0)
                {
                    GUI_SetDialog("No tienes una licencia para este contenido, "
                    + "pero existe una licencia gratuita:\n"
                    + "<b>" + lt.name + "</b>\n"
                    + "¿Deseas acceder a esta licencia?",
                    (option) => { if (option) { State_AcquireLicense(lt.id); } }
                    );
                    break;
                }
            }
        }

        public void GUI_Hide()
        {
            gui_input_user.gameObject.SetActive(false);
            gui_input_pass.gameObject.SetActive(false);
            gui_dialog.gameObject.SetActive(false);
            gui_loading.gameObject.SetActive(false);
            gui_avatar.gameObject.SetActive(false);
            gui_button_change.gameObject.SetActive(false);
            gui_button_continue.gameObject.SetActive(false);
            gui_button_login.gameObject.SetActive(false);
            gui_button_offline.gameObject.SetActive(false);
            gui_button_play.gameObject.SetActive(false);
        }

        override public void GUI_SetMessage(string _message, bool _append = false) //COMENTADO PORQUE TIRABA ERROR DE OVERRIDE
        {
            if (_append)
            {
                gui_message.text += _message;
            }
            else
            {
                gui_message.text = _message;
            }
        }


        public void GUI_SetDialog(string _text, Action<bool> OnResponse)
        {
            gui_dialog.gameObject.SetActive(true);
            gui_dialog_msg.text = _text;
            dialogAction = null;
            dialogAction += OnResponse;
        }

        public void GUI_DialogResponse(bool action)
        {
            if (dialogAction != null)
            {
                dialogAction(action);
            }
            gui_dialog.gameObject.SetActive(false);
        }
    }
}
