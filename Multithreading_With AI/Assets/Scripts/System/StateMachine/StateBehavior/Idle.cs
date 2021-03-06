using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : State
{
    float Timer = 0.0f;
    public override void Enter(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        EnemyAgent.currentState = EnemyAgent.stateMachine.GetCurrentState();
        //Debug.Log("Name: " + agent.name + " <color=blue>has been entered state</color>: " + EnemyAgent.currentState);
        Timer = 0.0f;
        float random = Random.Range(0.0f, 180.0f);
        EnemyAgent.gameObject.transform.rotation = Quaternion.Euler(0.0f, random, 0.0f);
        EnemyAgent._rigidbody.velocity = Vector3.zero;

        EnemyAgent.ChangeIndicator();
    }

    public override void Execute(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        if(EnemyAgent._isFound)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player").gameObject;
            EnemyAgent.transform.LookAt(player.transform.position);
            EnemyAgent.stateMachine.ChangeState("Find");
        }
        if (Timer> 2.5f)
            EnemyAgent.stateMachine.ChangeState("Find");
        else
            Timer += Time.deltaTime;

    }

    public override void Exit(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        //Debug.Log("Name: " + agent.name + " <color=blue>has been leaved out state</color>: " + "Idle");
        EnemyAgent._isFound = false;
    }
}
