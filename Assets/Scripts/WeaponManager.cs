using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public enum ShootType{Manual, Automatic}
public class WeaponManager : MonoBehaviour
{
    public string nameWeapon;
    public GameObject playerCam;
    public float range = 100f;
    public float damage = 25f;
    public Animator playerAnimator;

    public ParticleSystem flashParticleSystem;
    public GameObject bloodParticleSystem;
    public GameObject concreteParticleSystem;

    public AudioClip shootClip;
    public AudioSource weaponAudioSource;
    public WeaponSway weaponSway;
    public float swaySensitivity;

    public GameObject crosshair;

    public float currentAmmo;
    public float maxAmmo;
    public float reloadTime;
    public bool isReloading;
    public float reserveAmmo;

    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI reserveAmmoText;
    public TextMeshProUGUI currentWeaponText;
    public float fireRate;
    public float fireRateTimer;
    public ShootType type;
    public string weaponType;
    public float reserveAmmoCap;
    public GameManager gameManager;
    public PhotonView photonView;
    // Start is called before the first frame update
    void Start()
    {

        weaponAudioSource = GetComponent<AudioSource>();
        swaySensitivity = weaponSway.swaySensibility;
        UpdateAmmoText();
        reserveAmmoCap = reserveAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            return;
        }
        if (!gameManager.isPaused && !gameManager.isGameOver)
        {
            if (playerAnimator.GetBool("IsShooting"))
            {
                playerAnimator.SetBool("IsShooting", false);
            }
            if (reserveAmmo <= 0 && currentAmmo <= 0)
            {
                Debug.Log("Te quedaste sin balas");
                return;
            }
            if (currentAmmo <= 0 && !isReloading)
            {
                Debug.Log("No tienes balas");
                StartCoroutine(Reload(reloadTime));
                return;
            }
            if (isReloading)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.R) && reserveAmmo > 0 && currentAmmo != maxAmmo)
            {
                Debug.Log("Recarga Manual de las balas");
                StartCoroutine(Reload(reloadTime));
                return;
            }

            if (fireRateTimer > 0)
            {
                fireRateTimer -= Time.deltaTime;
            }

            if (type == ShootType.Manual)
            {
                if (Input.GetButtonDown("Fire1") && fireRateTimer <= 0)
                {
                    Shoot();
                    fireRateTimer = 1 / fireRate;
                }
            }
            else if (type == ShootType.Automatic)
            {
                if (Input.GetButton("Fire1") && fireRateTimer <= 0)
                {
                    Shoot();
                    fireRateTimer = 1 / fireRate;
                }
            }

        }
    }

    private void OnEnable()
    {
        playerAnimator.SetTrigger(weaponType);
        UpdateAmmoText();
    }
    private void OnDisable()
    {
        playerAnimator.SetBool("IsReloading", false);
        isReloading = false;
    }
    void Shoot()
    {
        currentAmmo--;
        UpdateAmmoText();
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("WeaponShootSFX", RpcTarget.All, photonView.ViewID);

        }
        else
        {
            ShootVFX(photonView.ViewID);
        }
        playerAnimator.SetBool("IsShooting", true);

        RaycastHit hit;
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range))
        {
            EnemyManager enemyManager = hit.transform.GetComponent<EnemyManager>();
            if(enemyManager != null )
            {
                enemyManager.Hit(damage);
                GameObject particleInstance = Instantiate(bloodParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
                particleInstance.transform.parent = hit.transform;
            }
            else
            {
                Instantiate(concreteParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
    void Aim()
    {
        playerAnimator.SetBool("IsAiming", true);
        weaponSway.swaySensibility = swaySensitivity / 3;
        crosshair.SetActive(false);
    }
    public IEnumerator Reload(float rt)
    {
        isReloading = true;
        playerAnimator.SetBool("IsReloading", true);
        crosshair.SetActive(false);
        yield return new WaitForSeconds(rt);
        playerAnimator.SetBool("IsReloading", false);
        if (!playerAnimator.GetBool("IsAiming"))
        {
            crosshair.SetActive(true);

        }
        float missingAmmo = maxAmmo - currentAmmo;
        if(reserveAmmo >= missingAmmo)
        {
            currentAmmo += missingAmmo;
            reserveAmmo -= missingAmmo;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }
        if (gameObject.activeSelf) {
            UpdateAmmoText();
        }
        

        isReloading = false;
    }
    public void ShootVFX(int viewID)
    {
        if(photonView.ViewID == viewID)
        {

            flashParticleSystem.Play();
            weaponAudioSource.PlayOneShot(shootClip, 1);
        }
    }
    public void UpdateAmmoText()
    {
        currentAmmoText.text = currentAmmo.ToString();
        reserveAmmoText.text = reserveAmmo.ToString();
        currentWeaponText.text = nameWeapon;
    }
}
