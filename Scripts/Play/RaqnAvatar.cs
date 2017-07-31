using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using RAQN;
using RAQN.Storage;

namespace RAQN.Play
{

    public class RaqnAvatar : MonoBehaviour
    {
        public event Action OnLoad;

        protected string avatar_name = "RANDOM";
        protected Renderer av_renderer;
        protected Image av_image;

        [SerializeField]
        public int avatar_size = 100;

        public void Awake()
        {
            av_renderer = GetComponent<Renderer>();
            av_image = GetComponent<Image>();
        }

        public void Start()
        {

        }

        public void SetAvatar(string _avatar = null)
        {
            if (_avatar == null)
            {
                RaqnPlayer _player = Raqn.Player();
                if (_player == null)
                {
                    this.avatar_name = "RANDOM";
                }
                else
                {
                    this.avatar_name = _player.avatar;
                }
            }
            else
            {
                this.avatar_name = _avatar;
            }

            bool _local = true;
            Texture2D _tex = GetLocalTexture(avatar_size);
            if (_tex == null)
            {
                _local = false;
                _tex = GetDefaultTexture(avatar_size);
            }

            if (!_local)
            {
                Raqn.Instance.StartCoroutine(LoadRemoteTexture(avatar_name, avatar_size, _tex));
            }
            else
            {
                if (av_renderer)
                {
                    av_renderer.material.mainTexture = _tex;
                }
                else if (av_image)
                {
                    av_image.sprite = GetSprite(_tex);
                }
            }
        }

        protected int SizeToImgSize(int _size)
        {
            if (_size <= 75)
            {
                return 50;
            }
            else if (_size <= 200)
            {
                return 100;
            }
            else
            {
                return 300;
            }
        }

        public Texture2D GetDefaultTexture(int _size = 100)
        {
            string _str_size = SizeToImgSize(_size).ToString();
            Texture2D _tex;
            _tex = new Texture2D(_size, _size, TextureFormat.DXT1, false);
            _tex.LoadImage(RaqnStorage.GetBytes("RAQN/Graphics/Avatars/RANDOM/" + _str_size + "/RANDOM_" + _str_size + ".png"));
            return _tex;
        }

        public Texture2D GetLocalTexture(int _size = 100)
        {
            string _str_size = SizeToImgSize(_size).ToString();
            Texture2D _tex;
            _tex = new Texture2D(_size, _size, TextureFormat.DXT1, false);
            byte[] _img = RaqnStorage.GetBytes("cache/avatars/" + avatar_name + "_" + _str_size + ".png");
            if (_img == null || _img.Length == 0) { return null; }
            _tex.LoadImage(_img);
            return _tex;
        }

        public Sprite GetSprite(Texture2D _tex)
        {
            Sprite _spr = Sprite.Create(_tex, new Rect(0, 0, _tex.width, _tex.height), new Vector2(0, 0), 100.0f);
            return _spr;
        }

        protected string AvatarUrl(string _avatar, int _size)
        {
            _size = SizeToImgSize(_size);
            return "http://arnold.raqn.in/raqn/AVATARS/" + _avatar + '/' + _size.ToString() + '/' + _avatar + '_' + _size.ToString() + ".png";
        }

        public IEnumerator LoadRemoteTexture(string _avatar, int _size, Texture2D _tex)
        {
            string _url = AvatarUrl(_avatar, _size);

            WWW req = new WWW(_url);
            yield return req;
            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.LogError("Error loading remote avatar texture");
                yield break;
            }
            
            if (!Directory.Exists(RaqnStorage.DataPath() + "/cache/avatars"))
            {
                Directory.CreateDirectory(RaqnStorage.DataPath() + "/cache/avatars");
            }
            RaqnStorage.PutBytes("cache/avatars/" + avatar_name + "_" + avatar_size + ".png", req.bytes);

            req.LoadImageIntoTexture(_tex);
            if (av_renderer)
            {
                av_renderer.material.mainTexture = _tex;
            }
            else if (av_image)
            {
                av_image.sprite = GetSprite(_tex);
            }
        }

    }
}