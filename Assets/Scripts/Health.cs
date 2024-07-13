using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void DeathEventHandler(Vector3 direction, Rigidbody rb);
    public event DeathEventHandler OnDeath;
    [SerializeField] float maxHealth;

    public float currentHealth { get; private set; }
    private bool dead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector3 direction = new Vector3(), Rigidbody hitBody = null)
    {
        if (dead) return;
        currentHealth -= amount;
        Debug.Log(currentHealth);
        if (currentHealth <= 0.0f)
        {
            dead = true;
            OnDeath?.Invoke(direction, hitBody);
        }
    }

}
