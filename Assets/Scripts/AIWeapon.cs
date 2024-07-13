using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class AIWeapon : MonoBehaviour
{
    [SerializeField] public WeaponIK weaponIK;

    [Header("Gun Properties")]
    [SerializeField] private float shootingDelay = 2f;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private float reloadTime;
    [SerializeField] private int magazineSize = 10;
    [SerializeField] private int numMagazine = 2;
    [SerializeField] private int bulletsPerBurst;

    private bool isReloading = false;
    private float reloadTimer;
    private int burstBulletsLeft;
    private int currentAmmo;
    private int currentMagazines;
    public bool IsAiming { get; set; }
    public bool isShooting { get; private set; }
    public bool readyToShoot { get; private set; }
    bool allowReset = true;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletVelocity;
    [SerializeField] private float bulletLifeTime = 3f;


    private Transform currentTarget;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        currentAmmo = magazineSize;
        currentMagazines = numMagazine;
    }

    private void Update()
    {
        if (readyToShoot && isShooting)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }
        HandleReload();
    }

    public void Drop(Vector3 force)
    {
        Destroy(weaponIK);
        transform.parent = null;
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.AddForce(force, ForceMode.VelocityChange);
        }
        if (TryGetComponent<BoxCollider>(out BoxCollider boxCollider))
        {
            boxCollider.enabled = true;
        }

    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
        weaponIK.SetTargetTransform(currentTarget);
    }

    public void FireWeapon(bool enabled)
    {
        readyToShoot = enabled;
        isShooting = enabled;
    }

    private void FireWeapon()
    {
        if (isReloading) return;
        if (currentAmmo == 0) return;

        currentAmmo -= 1;
        readyToShoot = false;
        Vector3 shootingDirection = GetShootDirection().normalized;
        GameObject muzzleFlashInst = Instantiate(muzzleFlash, bulletSpawn.position, Quaternion.identity);
        muzzleFlashInst.transform.forward = shootingDirection;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletLifeTime));
        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }
    }

    private Vector3 GetShootDirection()
    {
        return currentTarget.position - bulletSpawn.position;
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private void HandleReload()
    {
        if (!isReloading && currentMagazines > 0 && currentAmmo == 0)
        {
            currentMagazines--;
            currentAmmo = magazineSize;
            isReloading = transform;
            reloadTimer = 0;
        }
        if (isReloading)
        {
            isReloading = reloadTimer < reloadTime;
            reloadTimer += Time.deltaTime;
        }
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletLifeTime)
    {
        yield return new WaitForSeconds(bulletLifeTime);
        Destroy(bullet);
    }
}
