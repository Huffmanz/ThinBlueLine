using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackPlayerState : AIState
{
    public AiStateId GetID()
    {
        return AiStateId.AttackPlayer;
    }

    public void Enter(AIAgent agent)
    {
        agent.currentWeapon.SetTarget(agent.playerTransform);
        agent.navMeshAgent.stoppingDistance = 5.0f;
        agent.currentWeapon.FireWeapon(true);
    }

    public void Update(AIAgent agent)
    {
        agent.navMeshAgent.destination = agent.playerTransform.position;
    }

    public void Exit(AIAgent agent)
    {
        agent.navMeshAgent.stoppingDistance = 0.0f;
        agent.currentWeapon.FireWeapon(false);
    }



}
