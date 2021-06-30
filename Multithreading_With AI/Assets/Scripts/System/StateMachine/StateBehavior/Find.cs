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
    object box = null;
    public override void Enter(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        EnemyAgent.currentState = EnemyAgent.stateMachine.GetCurrentState();

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
        if (totalTimer > 3.0f)
        {
            bool accept = false;
            Vector3 pos = new Vector3(Random.Range(-Grid.Instance.gridSizeX, Grid.Instance.gridSizeX), 0.0f, Random.Range(-Grid.Instance.gridSizeY, Grid.Instance.gridSizeY));
            while (accept == false)
            {
                pos = new Vector3(Random.Range(-Grid.Instance.gridSizeX, Grid.Instance.gridSizeX), 0.0f, Random.Range(-Grid.Instance.gridSizeY, Grid.Instance.gridSizeY));
                if (Grid.Instance.GetNodeFromWorld(pos) == Grid.Instance.GetNodeFromWorld(EnemyAgent.transform.position))
                {
                    accept = false;
                    continue;
                }
                if (Grid.Instance.GetNodeFromWorld(pos).walkable == TileType.Walkable)
                {
                    accept = true;
                    break;
                }
            }

            box = new PathReqeustInfo(EnemyAgent.id, EnemyAgent.transform.position, pos, EnemyAgent.PathFound);
            if (EnemyAgent.type == ThreadingType.Thread)
                PathThreadManager.RequestInfo(box);
            else if (EnemyAgent.type == ThreadingType.Task)
                PathTaskManager.RequestInfo(box);

            
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

                    Node playerPos = Grid.Instance.GetNodeFromWorld(PlayerUnit.transform.position);
                    EnemyAgent.transform.LookAt(PlayerUnit.transform);
                    box = new PathReqeustInfo(EnemyAgent.id, EnemyAgent.transform.position, playerPos.position, EnemyAgent.PathFound); // main loop
                    if (EnemyAgent.type == ThreadingType.Thread)
                        PathThreadManager.RequestInfo(box);
                    else if (EnemyAgent.type == ThreadingType.Task)
                        PathTaskManager.RequestInfo(box);

                    EnemyAgent.gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                }
                timer = 0.0f;
            }
            else
                timer += Time.deltaTime;

            totalTimer += Time.deltaTime;
        }
        if(random)
            EnemyAgent.gameObject.transform.Rotate(0.0f, Mathf.Cos(Time.deltaTime) / 4.0f * EnemyAgent._speed * MoveTimer, 0.0f);
        else
            EnemyAgent.gameObject.transform.Rotate(0.0f, Mathf.Cos(Time.deltaTime) / 4.0f * -EnemyAgent._speed * MoveTimer, 0.0f);

        if (MoveTimer >= 1.0f)
            changeSide = true;
        if (MoveTimer <= -1.0f)
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
