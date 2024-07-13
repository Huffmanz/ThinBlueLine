using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AIAgentConfig : ScriptableObject
{
    public float MaxTime = 1f;
    public float MaxDistance = 1f;
    public float MaxSightDistance = 5.0f;
    public float RagdollForce = 2.0f;
}
