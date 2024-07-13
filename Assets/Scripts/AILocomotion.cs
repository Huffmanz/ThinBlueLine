using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AILocomotion : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Animator animator;

    private int speedAnimString = Animator.StringToHash("Speed");

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (navMeshAgent == null)
        {
            Destroy(this);
            return;
        }
        if (navMeshAgent.hasPath)
        {
            animator.SetFloat(speedAnimString, navMeshAgent.velocity.magnitude);
        }
        else
        {
            animator.SetFloat(speedAnimString, 0);
        }
    }
}
