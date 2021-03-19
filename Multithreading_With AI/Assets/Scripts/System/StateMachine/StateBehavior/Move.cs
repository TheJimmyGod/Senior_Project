using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : State
{
    float originalView;
    float originalAngle;
    float timer = 0.0f;
    bool found = false;

    public override void Enter(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        EnemyAgent.currentState = EnemyAgent.stateMachine.GetCurrentState();
        //Debug.Log("Name: " + agent.name + " <color=blue>has been entered state</color>: " + EnemyAgent.currentState);

        EnemyAgent.ChangeIndicator();
        EnemyAgent.GetOrderFromAI();
        EnemyAgent._isStart = false;
        originalView = agent.GetComponent<VisualSensor>().viewRaidus;
        originalAngle = agent.GetComponent<VisualSensor>().viewAngle;
        agent.GetComponent<VisualSensor>().viewRaidus = originalView / 2.0f;
        agent.GetComponent<VisualSensor>().viewAngle = originalAngle / 3.0f;
        EnemyAgent.EnableDisplaySight();
    }

    public override void Execute(GameObject agent)
    {
        var EnemyAgent = agent.GetComponent<Enemy>();
        var PlayerUnit = GameObject.FindGameObjectWithTag("Player").gameObject;
        EnemyAgent.transform.Rotate(new Vector3(0.0f, EnemyAgent.transform.rotation.y, 0.0f), Space.World);
        if (EnemyAgent == null) return;
        object box;
        if (EnemyAgent._isStart == false)
        {
            //Debug.Log("Executing... ");
            EnemyAgent._isStart = true;

            if(EnemyAgent._isFound)
            {
                Node playerPos = Grid.Instance.GetNodeFromWorld(PlayerUnit.transform.position);
                EnemyAgent.transform.LookAt(PlayerUnit.transform);
                box = new PathReqeustInfo(EnemyAgent.id, EnemyAgent.transform.position, playerPos.position, EnemyAgent.PathFound);
                if (EnemyAgent.type == ThreadingType.Thread)
                {
                    PathThreadManager.RequestInfo(box);
                }
                else if (EnemyAgent.type == ThreadingType.Task)
                {
                    PathTaskManager.RequestInfo(box);
                }
            }
            else
            {
                EnemyAgent.lastDistanceRecord.Clear();
                EnemyAgent.lastDistance = 0.0f;
                bool accept = false;
                Vector3 pos = new Vector3(Random.Range(-Grid.Instance.gridSizeX, Grid.Instance.gridSizeX), 0.0f, Random.Range(-Grid.Instance.gridSizeY, Grid.Instance.gridSizeY));
                while(accept == false)
                {
                    pos = new Vector3(Random.Range(-Grid.Instance.gridSizeX, Grid.Instance.gridSizeX), 0.0f, Random.Range(-Grid.Instance.gridSizeY, Grid.Instance.gridSizeY));
                    if (Grid.Instance.GetNodeFromWorld(pos) == Grid.Instance.GetNodeFromWorld(EnemyAgent.transform.position))
                    {
                        accept = false;
                        continue;
                    }
                    if (Grid.Instance.GetNodeFromWorld(pos).walkable != TileType.UnWalkable)
                        accept = true;
                }


                box = new PathReqeustInfo(EnemyAgent.id, EnemyAgent.transform.position, pos, EnemyAgent.PathFound);
                if (EnemyAgent.type == ThreadingType.Thread)
                {
                    PathThreadManager.RequestInfo(box);
                }
                else if (EnemyAgent.type == ThreadingType.Task)
                {
                    PathTaskManager.RequestInfo(box);
                }
            }

        }
        else
        {
            
            if (timer > 1.5f)
            {
                found = agent.GetComponent<VisualSensor>().ActivatingVisualSensor();
                timer = 0.0f;
            }

            if(found == true)
            {
                EnemyAgent.FinalizePathFinding();
                box = new PathReqeustInfo(EnemyAgent.id, EnemyAgent.transform.position, PlayerUnit.transform.position, EnemyAgent.PathFound);

                PathThreadManager.RequestInfo(box);
                EnemyAgent._isFound = true;
                found = false;
                return;
            }

            if (EnemyAgent.path == null || EnemyAgent.path.Length < 1)
            {
                EnemyAgent.stateMachine.ChangeState("Idle");
                return;
            }

            if(EnemyAgent.tiles.Count == 0)
                EnemyAgent.tiles = Grid.Instance.GenerateTile(EnemyAgent.path);

            EnemyAgent.current = EnemyAgent.path[EnemyAgent._previousIndex];

            if (Vector3.Distance(EnemyAgent.transform.position, EnemyAgent.current) > 0.75f)
            {
                Vector3 direction = Vector3.Normalize(EnemyAgent.current - EnemyAgent.transform.position);
                Vector3 newPos = EnemyAgent.transform.position + (direction * EnemyAgent._speed * Time.deltaTime);

                EnemyAgent._rigidbody.MovePosition(new Vector3(newPos.x, 0.0f, newPos.z));

                EnemyAgent.transform.LookAt(EnemyAgent.current);
            }
            else
            {
                EnemyAgent._rigidbody.velocity = Vector3.zero;
                if (EnemyAgent._previousIndex == EnemyAgent.MaximumPath)
                {
                    EnemyAgent.FinalizePathFinding();
                    EnemyAgent.stateMachine.ChangeState("Idle");
                }
                else
                    EnemyAgent._previousIndex++;

            }

            timer += Time.deltaTime;
        }
        
    }

    public override void Exit(GameObject agent)
    {
        //Debug.Log("Name: " + agent.name + " <color=blue>has been leaved out state</color>: " + "Move");
        var EnemyAgent = agent.GetComponent<Enemy>();

        EnemyAgent.FinalizePathFinding();
        agent.GetComponent<VisualSensor>().viewRaidus = originalView;
        agent.GetComponent<VisualSensor>().viewAngle = originalAngle;
        EnemyAgent.EnableDisplaySight();
        timer = 0.0f;
        found = false;
    }
}
