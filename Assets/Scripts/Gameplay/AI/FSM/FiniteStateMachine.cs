using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFarm.AI.FSM
{
    // This is the base class for our finite state machine. It will manage the states and transitions for an AI character (e.g. an NPC).
    // Each state will be a separate class that inherits from a base State class, and the FSM will handle switching between these states based on certain conditions (e.g. time of day, player proximity, etc.).
    // public class Blackboard
    // {
    //     // This is a simple data structure that can be used to store information that states can read and write to. For example, it can store the current time of day, the player's position, etc.
    //     // This allows different states to share information without needing to directly reference each other.
    //     private Dictionary<string, object> data = new Dictionary<string, object>();

    //     public void SetValue(string key, object value)
    //     {
    //         data[key] = value;
    //     }

    //     public T GetValue<T>(string key)
    //     {
    //         if (data.TryGetValue(key, out object value) && value is T)
    //         {
    //             return (T)value;
    //         }
    //         return default(T);
    //     }

    //     public Blackboard(List<string> keys)
    //     {
    //         foreach (var key in keys)
    //         {
    //             data[key] = null; // Initialize all keys with null value
    //         }
    //     }
    // }
    public interface IState
    {
        void OnEnter();
        void OnExit();
        void OnUpdate();
    }
    public class FiniteStateMachine
    {
        private IState currentState;

        private void ChangeState(IState newState)
        {
            if(newState == currentState)
            {
                return; // No need to change state if it's the same state
            }
            if (currentState != null)
            {
                currentState.OnExit();
            }
            currentState = newState;
            if (currentState != null)
            {
                currentState.OnEnter();
            }
        }
        
        public void Update()
        {
            if (currentState != null)
            {
                currentState.OnUpdate();
            }
        }


    }
}
