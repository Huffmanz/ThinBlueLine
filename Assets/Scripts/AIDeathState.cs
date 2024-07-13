using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AIDeathState : AIState
{
    public Vector3 direction;
    public Rigidbody rigidbody;

    public AiStateId GetID()
    {
        return AiStateId.Death;
    }

    public void Enter(AIAgent agent)
    {
        GameObject.Destroy(agent.navMeshAgent);
        agent.ragdoll.ActivateRagdoll();
        direction.y = 1;
        agent.currentWeapon.Drop(direction);
        agent.ragdoll.ApplyForce(direction * agent.AgentConfig.RagdollForce, rigidbody);
        agent.skinnedMeshRenderer.updateWhenOffscreen = true;
    }

    public void Exit(AIAgent agent)
    {

    }

    public void Update(AIAgent agent)
    {
    }

}
