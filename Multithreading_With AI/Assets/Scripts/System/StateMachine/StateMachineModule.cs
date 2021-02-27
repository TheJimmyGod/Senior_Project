using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public class StateMachineModule : MonoBehaviour
{
    // Start is called before the first frame update
    public State currentState;
    public Dictionary<string, object> states = new Dictionary<string, object>();
    public GameObject agent;

    private void Awake()
    {
        agent = this.gameObject;
    }
    
    public void AddState<T>(T newState, string stateName) where T : class
    {
        if(states.ContainsKey(stateName))
        {
            Debug.Log("<color=red>Warning!</color> The state already exists " + stateName);
            return;
        }
        else if(states.ContainsValue(newState))
        {
            Debug.Log("<color=red>Warning!</color> The state already exists " + newState.ToString());
            return;
        }
        else
        {
            states.Add(stateName,newState);
            if (currentState == null)
                currentState = states[stateName] as State;
        }
    }

    public void ChangeState(string stateName)
    {
        State _state = null;
        if (states.ContainsKey(stateName))
        {
            _state = states[stateName] as State;
        }
        else
        {
            Debug.Log("<color=red>Warning!</color> The state does not exist on his behavior");
        }

        if (currentState != null)
            currentState.Exit(agent);
        currentState = _state;
        currentState.Enter(agent);
    }

    public string GetCurrentState()
    {
        return currentState.ToString();
    }

    public void ActivateState()
    {
        if (agent == null)
            return;
        if (currentState != null)
            currentState.Execute(agent);
    }
}
