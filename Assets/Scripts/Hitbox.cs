using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] bool InstantKill;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Hit(Vector3 direction)
    {
        if (InstantKill)
        {
            health.TakeDamage(health.currentHealth, direction, rb);
        }
        else
        {
            health.TakeDamage(1, direction, rb);
        }
    }
}
