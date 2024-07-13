using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIChasePlayerState : AIState
{
    private float timer = 0.0f;


    public AiStateId GetID()
    {
        return AiStateId.ChasePlayer;
    }

    public void Enter(AIAgent agent)
    {
    }

    public void Exit(AIAgent agent)
    {
        return;
    }


    public void Update(AIAgent agent)
    {
        if (!agent.enabled) return;

        timer -= Time.deltaTime;
        if (!agent.navMeshAgent.hasPath)
        {
            agent.navMeshAgent.destination = agent.playerTransform.position;
        }
        if (timer < 0.0f)
        {
            Vector3 direction = agent.playerTransform.position - agent.navMeshAgent.destination;
            direction.y = 0;
            if (direction.sqrMagnitude > agent.AgentConfig.MaxDistance * agent.AgentConfig.MaxDistance)
            {
                if (agent.navMeshAgent.pathStatus != NavMeshPathStatus.PathPartial)
                {
                    agent.navMeshAgent.destination = agent.playerTransform.position;
                }
            }
            timer = agent.AgentConfig.MaxTime;
        }
    }

}
