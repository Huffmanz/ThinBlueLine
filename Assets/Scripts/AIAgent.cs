using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    [SerializeField] AiStateId initialState;
    [SerializeField] AIAgentConfig aIAgentConfig;
    [SerializeField] public AIWeapon currentWeapon;

    public AIAgentConfig AgentConfig => aIAgentConfig;

    public AIStateMachine stateMachine { get; private set; }
    public Transform playerTransform { get; private set; }
    public NavMeshAgent navMeshAgent { get; private set; }
    public Ragdoll ragdoll { get; private set; }
    public SkinnedMeshRenderer skinnedMeshRenderer { get; private set; }
    Health health;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        health = GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += Health_OnDeath;
        }
        ragdoll = GetComponent<Ragdoll>();
        skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    void Start()
    {
        stateMachine = new AIStateMachine(this);
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        stateMachine.RegisterState(new AIChasePlayerState());
        stateMachine.RegisterState(new AIDeathState());
        stateMachine.RegisterState(new AIIdleState());
        stateMachine.RegisterState(new AIAttackPlayerState());
        stateMachine.ChangeState(initialState);
    }

    void Update()
    {
        stateMachine.Update();
    }

    private void Health_OnDeath(Vector3 direction, Rigidbody rigidbody)
    {
        AIDeathState deathState = (AIDeathState)stateMachine.GetState(AiStateId.Death);
        deathState.direction = direction;
        deathState.rigidbody = rigidbody;
        stateMachine.ChangeState(AiStateId.Death);
    }

}
