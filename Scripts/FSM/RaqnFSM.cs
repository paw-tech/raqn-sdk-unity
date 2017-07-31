using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Finite State Machines 
 */
namespace RAQN.FSM
{
    public abstract class MachineBehaviour : MonoBehaviour
    {

        protected Dictionary<string,State> state_list;
        protected State state_current;
        protected Stack<string> state_change_requests;
        protected bool state_debug = false;
        public bool Paused = false;

        protected virtual void MachineAwake()
        {
            if (state_debug)
            {
                Debug.Log("Awaking Machine");
            }
            state_list = new Dictionary<string, State>();
            state_change_requests = new Stack<string>();
        }

        protected virtual void MachineStart()
        {
            if(State != null && !State.IsActive)
            {
                if (state_debug) { Debug.Log("Entering State " + State); }
                State.StateEnter();
            }
        }

        public State State { get { return state_current;  } }

        protected void StateAdd(State _state)
        {
            state_list.Add(_state.Name, _state);
            if (state_debug)
            {
                Debug.Log("Added State " + _state.Name);
            }
        }

        public bool StateHas(string _stateName)
        {
            return state_list.ContainsKey(_stateName);
        }

        protected bool StateSet(string _stateName)
        {
            State _state = StateGet(_stateName);
            if(_state == null)
            {
                Debug.Log("State " + _stateName + " not found!");
                return false;
            }
            state_current = _state;
            if (state_debug) { Debug.Log("Setted State " + state_current); }
            return true;
        }

        public State StateGet(string _stateName)
        {
            if (!this.StateHas(_stateName)) { return null; }
            return state_list[_stateName];
        }

        /*
         * Requests a State Change to the machine, which will be solved at the end of the frame
         */
        public bool StateChange(string _stateName)
        {
            if (!this.StateHas(_stateName)) { return false; }
            state_change_requests.Push(_stateName);
            return true;
        }

        protected virtual void MachineLateUpdate()
        {
            if(state_current == null || Paused)
            {
                return;
            }
            var _count = state_change_requests.Count;
            if (_count == 0) { return; }
            if (_count > 1)
            {
                Debug.Log("More than one change request on state machine " + this.GetType());
            }
            var _reqState = "";
            while (state_change_requests.Count > 0)
            {
                _reqState = state_change_requests.Pop();
                if(_reqState != state_current.Name) { break; }
            }
            state_change_requests.Clear();

            State _newState = this.StateGet(_reqState);
            if(_newState != null)
            {
                if (state_debug) { Debug.Log("Exiting State " + State); }
                State.StateExit();
                StateSet(_reqState);
                if (state_debug) { Debug.Log("Entering State " + State); }
                State.StateEnter();
            }
        }

        // Use this for initialization
        protected virtual void Awake()
        {
            MachineAwake();
        }

        protected virtual void Start()
        {
            MachineStart();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (state_current == null)
            {
                Debug.LogError("Machine does not have a State!");
                return;
            }
            if (Paused) { return; }
            state_current.StateUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (Paused) { return; }
            state_current.StateFixedUpdate();
        }

        protected virtual void LateUpdate()
        {
            if (Paused) { return; }
            MachineLateUpdate();
        }
    }

    public abstract class State
    {
        protected bool is_active;
        public bool IsActive { get { return is_active; } }

        protected string name = "DefaultState";
        public string Name { get { return name; } }

        protected MachineBehaviour machine;
        protected float time_started;
        public float TimeStarted { get { return time_started; } }
        public float TimeElapsed
        {
            get
            {
                if (time_started == -1.0f) { return 0.0f; }
                return Time.time - time_started;
            }
        }

        //protected virtual MachineBehaviour Machine { get { return machine; } }

        protected State(MachineBehaviour _machine, string _name)
        {
            name    = _name;
            machine = _machine;
            StateAwake();
        }

        public override string ToString()
        {
            return name;
        }

        // Use this for initialization
        protected virtual void StateAwake()
        {
            is_active = false;
            time_started = -1.0f;
        }

        public virtual void StateEnter()
        {
            time_started = Time.time;
            is_active = true;
        }

        public virtual void StateUpdate() { }

        public virtual void StateFixedUpdate() { }
        /*
         * Called on LateUpdate when exiting State;
         * Call parent::Exit at the end of extension method
         */
        public virtual void StateExit()
        {
            time_started = -1.0f;
            is_active = false;
        }
    }

}

