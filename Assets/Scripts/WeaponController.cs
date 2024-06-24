using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShootingMode
{
    Single,
    Burst,
    Automatic
}

public class WeaponController : MonoBehaviour
{
    public event Action WeaponFiredEvent;

    [Header("Gun Properties")]
    [SerializeField] private float shootingDelay = 2f;
    [SerializeField] private ShootingMode shootingMode;
    [SerializeField] private int bulletsPerBurst;
    [SerializeField] private float spread;
    [SerializeField] private GameObject muzzleFlash;

    private int burstBulletsLeft;


    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawn;
    [SerializeField] private float bulletVelocity;
    [SerializeField] private float bulletLifeTime = 3f;

    [Header("Sights")]
    [SerializeField] private Transform sightTarget;
    [SerializeField] private float sightOffset;
    [SerializeField] private float aimingSpeed;

    public bool IsAiming { get; set; }
    public bool isShooting { get; private set; }
    public bool readyToShoot { get; private set; }
    bool allowReset = true;


    private Vector3 aimPosition;
    private Vector3 aimVelocity;
    private Vector3 originalPosition;
    private Camera playerCamera;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        originalPosition = transform.localPosition;
    }

    private void Start()
    {
        playerCamera = FirstPersonController.Instance.playerCamera;
    }

    private void Update()
    {
        AimDownSights();
        if (shootingMode == ShootingMode.Automatic)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (shootingMode == ShootingMode.Single || shootingMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }
        if (readyToShoot && isShooting)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }
    }

    private void AimDownSights()
    {
        var targetPosition = originalPosition;
        if (IsAiming)
        {
            targetPosition = playerCamera.transform.position + (transform.position - sightTarget.transform.position) + (playerCamera.transform.forward * sightOffset);
            targetPosition = playerCamera.transform.InverseTransformPoint(targetPosition);
        }

        aimPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref aimVelocity, aimingSpeed);
        transform.localPosition = aimPosition;
    }

    public void FireWeapon()
    {
        readyToShoot = false;
        WeaponFiredEvent?.Invoke();
        Vector3 shootingDirection = CalculateDirectionAndSpready().normalized;
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
        if (shootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            FireWeapon();
        }

    }

    private Vector3 CalculateDirectionAndSpready()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100);
        }

        Vector3 direction = targetPoint - bulletSpawn.position;
        float x = UnityEngine.Random.Range(-spread, spread);
        float y = UnityEngine.Random.Range(-spread, spread);

        return direction + new Vector3(x, y, 0);
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float bulletLifeTime)
    {
        yield return new WaitForSeconds(bulletLifeTime);
        Destroy(bullet);
    }
}
