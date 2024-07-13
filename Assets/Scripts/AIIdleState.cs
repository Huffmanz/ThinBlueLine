using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIIdleState : AIState
{
    public AiStateId GetID()
    {
        return AiStateId.Idle;
    }

    public void Enter(AIAgent agent)
    {

    }

    public void Exit(AIAgent agent)
    {

    }


    public void Update(AIAgent agent)
    {
        Vector3 playerDirection = agent.playerTransform.position - agent.transform.position;
        if (playerDirection.magnitude > agent.AgentConfig.MaxSightDistance)
        {
            return;
        }

        Vector3 agentDirection = agent.transform.forward;
        playerDirection.Normalize();
        float dotProduct = Vector3.Dot(playerDirection, agentDirection);
        if (dotProduct > 0.0f)
        {
            agent.stateMachine.ChangeState(AiStateId.AttackPlayer);
        }

    }


}
