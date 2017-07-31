using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RAQN.App
{
    public class RaqnApp : MonoBehaviour
    {
        static public RaqnApp App;

        public RaqnScene Scene;
        static public event Action OnAppStart;
        static public event Action OnAppQuit;

        virtual protected void Awake()
        {
            if (App == null)
            {
                DontDestroyOnLoad(gameObject);
                App = this;
                Initialize();
            }
            else if (App != this)
            {
                Destroy(gameObject);
            }
        }

        virtual protected void Start()
        {
            if(OnAppStart != null){ OnAppStart(); }
        }

        protected virtual void Initialize()
        {

        }

        public virtual void ChangeScene(string _sceneName)
        {
            Scene = null;
            UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName);
        }

        public virtual void ChangeScene(int _sceneNum)
        {
            Scene = null;
            UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneNum);
        }

        public virtual void Quit()
        {
            if (OnAppQuit != null) { OnAppQuit(); }
            UnityEngine.Application.Quit();
        }
    }

    public class RaqnScene : MonoBehaviour
    {
        public bool IsPaused = false;

        static public event Action OnSceneEnter;
        static public event Action OnSceneLeave;

        static public event Action OnPauseOn;

        static public event Action OnPauseOff;

        // Use this for initialization
        virtual protected void Start()
        {
            RaqnApp.App.Scene = this;
        }

        virtual public void GUI_SetMessage(string _message, bool _append = false)
        {
            Debug.Log("Set Msg: " + _message);
        }

        virtual public void PauseOn()
        {
            IsPaused = true;
            Time.timeScale = 0;
            if(OnPauseOn != null){ OnPauseOn(); }
        }

        virtual public void PauseOff()
        {
            IsPaused = false;
            Time.timeScale = 1;
            if (OnPauseOff != null) { OnPauseOff(); }
        }

        virtual public void PauseToggle()
        {
            if(IsPaused)
            {
                PauseOff();
            }
            else
            {
                PauseOn();
            }
        }

    }
}
