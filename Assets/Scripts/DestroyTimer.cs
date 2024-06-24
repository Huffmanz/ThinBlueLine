using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTimer : MonoBehaviour
{
    [SerializeField] float destroyTimer;

    void Update()
    {
        destroyTimer -= Time.deltaTime;
        if (destroyTimer < 0)
        {
            Destroy(gameObject);
        }
    }
}
