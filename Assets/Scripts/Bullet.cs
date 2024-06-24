using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private void OnCollisionEnter(Collision hitObject)
    {
        if (hitObject.gameObject.CompareTag("Target"))
        {
            print($"hit {hitObject.gameObject.name}!");
            CreateBulletImpactEffect(hitObject);
            Destroy(gameObject);
        }
        if (hitObject.gameObject.CompareTag("Wall"))
        {
            CreateBulletImpactEffect(hitObject);
            Destroy(gameObject);
        }
    }

    private void CreateBulletImpactEffect(Collision hitObject)
    {
        ContactPoint contact = hitObject.contacts[0];

        GameObject hole = Instantiate(GlobalReferences.Instance.bulletImpactEffect, contact.point, Quaternion.LookRotation(contact.normal));
        hole.transform.SetParent(hitObject.transform);
    }
}
