using UnityEngine;

public abstract class State
{
    public abstract void Enter(GameObject agent);
    public abstract void Execute(GameObject agent);
    public abstract void Exit(GameObject agent);
}
