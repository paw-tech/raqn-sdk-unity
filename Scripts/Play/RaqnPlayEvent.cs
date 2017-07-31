using System;
using FullSerializer;
using RAQN.Serialization;
using RAQN.Storage;


namespace RAQN.Play
{
    [fsObject]
    public class RaqnPlayEvent
    {
        [fsProperty]
        protected string name;

        [fsProperty]
        protected float time;

        [fsProperty]
        protected string player;

        [fsProperty]
        protected float value; 

        [fsProperty]
        protected string label;

        public RaqnPlayEvent() : base()
        {
            name = "";
            time = -1;
            value = -1;
            label = "";
            player = "";
        }

        public RaqnPlayEvent(string _name)
        {
            name = _name;
            time = (float)Math.Round(UnityEngine.Time.time, 2);
        }

        public RaqnPlayEvent(string _name, float _value)
        {
            name = _name;
            time = (float)Math.Round(UnityEngine.Time.time,2);
            value = _value;
        }

        public RaqnPlayEvent(string _name, string _label)
        {
            name = _name;
            time = (float)Math.Round(UnityEngine.Time.time, 2);
            label = _label;
        }

        public RaqnPlayEvent(string _name, string _label, float _value)
        {
            name = _name;
            time = (float)Math.Round(UnityEngine.Time.time, 2);
            label = _label;
            value = _value;
        }

        public void SetTime(float _time)
        {
            time = (float)Math.Round(_time,2);
        }

        public void SetPlayer(string _player)
        {
            player = _player;
        }
    }
}