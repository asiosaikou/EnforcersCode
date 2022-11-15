using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    // Gun stats
    public int GunID;
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    // Bools
    bool shooting, readyToShoot, reloading;

    // Reference
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    // Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;
    public TextMeshProUGUI text;

    // Recoil
    private Recoil Recoil_Script;

    public void Start()
    {
        bulletsLeft = magazineSize < Globals.ReserveAmmoCount[GunID] ?
            magazineSize :
            Globals.ReserveAmmoCount[GunID];
        readyToShoot = true;

        Recoil_Script = GameObject.Find("CameraRot/CameraRecoil").GetComponent<Recoil>();
    }

    public void Update()
    {
        MyInput();

        //SetText
        text.SetText(bulletsLeft + " / " + Globals.ReserveAmmoCount[GunID]);
    }

    private void MyInput()
    {
        if (allowButtonHold)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        // Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
            Recoil_Script.RecoilFire();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 20;

        // RayCast
        Debug.Log("I was shot!");
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);
            Debug.DrawRay(transform.position, forward, Color.green);

            if (rayHit.collider.CompareTag("Enemy"))
            {
                rayHit.collider.GetComponent<EnemyHPDamage>().takeDamage(damage);
            }
        }

        // Graphics
        GameObject t_newHole = Instantiate(bulletHoleGraphic, rayHit.point + rayHit.normal * 0.001f, Quaternion.identity) as GameObject;
        t_newHole.transform.LookAt(rayHit.point + rayHit.normal);
        Destroy(t_newHole, 5f);
        GameObject t_newMuzzle = Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity) as GameObject;
        Destroy(t_newMuzzle, 0.5f);
        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        int bulletsThatNeedToBeAdded = magazineSize - bulletsLeft;
        int bulletsThatCanBeAdded = bulletsThatNeedToBeAdded < Globals.ReserveAmmoCount[GunID] ?
            bulletsThatNeedToBeAdded :
            Globals.ReserveAmmoCount[GunID];
        bulletsLeft += bulletsThatCanBeAdded;
        Globals.ReserveAmmoCount[GunID] -= bulletsThatCanBeAdded;
        reloading = false;
    }
}