using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Find : State
{
    float timer = 0.0f;
    float totalTimer = 0.0f;
    float MoveTimer = 0.0f;

    bool changeSide = false;
    bool random = false;
    public override void Enter(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        EnemyAgent.currentState = EnemyAgent.stateMachine.GetCurrentState();
        //Debug.Log("Name: " + agent.name + " <color=blue>has been entered state</color>: " + EnemyAgent.currentState);

        EnemyAgent.ChangeIndicator();
        totalTimer = 0.0f;
        timer = 0.0f;
        EnemyAgent.transform.Rotate(new Vector3(0.0f, EnemyAgent.transform.rotation.y, 0.0f), Space.World);
        EnemyAgent.EnableDisplaySight();
        float randomNumber = Random.Range(0.0f, 1.0f);
        if (randomNumber <= 0.5f)
            random = true;
        else
            random = false;
    }

    public override void Execute(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        var PlayerUnit = GameObject.FindGameObjectWithTag("Player").gameObject;
        bool found = false;
        if (totalTimer > 7.0f)
        {
            EnemyAgent.stateMachine.ChangeState("Move");
            totalTimer = 0.0f;
        }
        else
        {
            if (timer > 0.2f)
            {
                found = agent.GetComponent<VisualSensor>().ActivatingVisualSensor();
                if (found)
                {
                    EnemyAgent._isFound = true;
                    EnemyAgent.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    EnemyAgent.stateMachine.ChangeState("Move");
                }
                timer = 0.0f;
            }
            else
                timer += Time.deltaTime;

            totalTimer += Time.deltaTime;
        }
        if(random)
        {
            EnemyAgent.gameObject.transform.Rotate(0.0f, Mathf.Cos(Time.deltaTime) / 4.0f * EnemyAgent._speed * MoveTimer, 0.0f);
        }
        else
        {
            EnemyAgent.gameObject.transform.Rotate(0.0f, Mathf.Cos(Time.deltaTime) / 4.0f * -EnemyAgent._speed * MoveTimer, 0.0f);
        }

        if (MoveTimer >= 2.0f)
            changeSide = true;
        if (MoveTimer <= -2.0f)
            changeSide = false;
        if (!changeSide)
            MoveTimer += Time.deltaTime;
        else
            MoveTimer -= Time.deltaTime;
    }

    public override void Exit(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        //Debug.Log("Name: " + agent.name + " <color=blue>has been leaved out state</color>: " + "Find");
        EnemyAgent.EnableDisplaySight();
    }
}
