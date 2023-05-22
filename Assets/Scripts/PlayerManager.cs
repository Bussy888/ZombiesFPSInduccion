using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public float health = 100f;
    public float healtCap;
    public Slider healthSlider;
    public TextMeshProUGUI healthText;
    public GameManager gameManager;
    public GameObject playerCamera;
    private float shakeTime;
    private float shakeDuration=0.5f;
    private Quaternion playerCameraOriginalRotation;
    public CanvasGroup hitPanel;
    public GameObject weaponHolder;
    private int activeWeaponIndex;
    private GameObject activeWeapon;

    public int totalPoints;
    public TextMeshProUGUI pointsText;
    public PhotonView photonView;
    void Start()
    {
        healtCap = health;
        playerCameraOriginalRotation = playerCamera.transform.localRotation;
        healthSlider.maxValue = health;
        healthSlider.value = health;
        WeaponSwitch(0);
        totalPoints = 0;
        UpdatePoints(0);
    }

    void Update()
    {
        if (PhotonNetwork.InRoom && !photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(false);
            return;
        }
        if (hitPanel.alpha > 0)
        {
            hitPanel.alpha -= Time.deltaTime;
        }
        if (shakeTime < shakeDuration)
        {
            shakeTime += Time.deltaTime;
            CameraShake();
        }else if(playerCamera.transform.localRotation != playerCameraOriginalRotation)
        {
            playerCamera.transform.localRotation = playerCameraOriginalRotation;
        }
        if(Input.GetAxis("Mouse ScrollWheel") !=0)
        {
            WeaponSwitch(activeWeaponIndex + 1);
        }
    }
    public void Hit(float damage)
    {
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("PlayerTakeDamage", RpcTarget.All, damage, photonView.ViewID);

        }
        else
        {
            PlayerTakeDamage(damage, photonView.ViewID);
        }
    }
    [PunRPC]
    public void PlayerTakeDamage(float damage, int viewID)
    {
        if (photonView.ViewID == viewID)
        {
            health -= damage;
            healthSlider.value = health;
            if (health > 25)
            {
                healthText.text = health.ToString();
                healthText.color = Color.white;
                shakeTime = 0;
                hitPanel.alpha = 1;
            }
            else
            {
                healthText.text = health.ToString();
                healthText.color = Color.red;
                shakeTime = 0;
                hitPanel.alpha = 1;
            }
            if (health <= 0)
            {
                gameManager.GameOver();
            }
        }
    }
   
    public void CameraShake()
    {
        playerCamera.transform.localRotation = Quaternion.Euler(Random.Range(-2f, 2f),0,0);
    }

    public void WeaponSwitch(int weaponIndex)
    {
        int index = 0;
        int amountPfWeapons = weaponHolder.transform.childCount;

        if (weaponIndex > amountPfWeapons - 1)
        {
            weaponIndex = 0;
        }
        foreach (Transform child in weaponHolder.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
            if (index == weaponIndex)
            {
                child.gameObject.SetActive(true);
                activeWeapon = child.gameObject;
            }
            index++;
        }
        activeWeaponIndex = weaponIndex;
        if (photonView.IsMine)
        {
            Hashtable hash = new Hashtable
            {
                { "weaponIndex", weaponIndex }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }
    public void UpdatePoints(int pointsToAdd) 
    {
        totalPoints += pointsToAdd;
        pointsText.text = $"${totalPoints}";
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!photonView.IsMine && targetPlayer == photonView.Owner && changedProps["weaponIndex"] != null)
        {
            WeaponSwitch((int)changedProps["weaponIndex"]);
        }
    }
    [PunRPC]
    public void WeaponShootSFX(int viewID)
    {
        activeWeapon.GetComponent<WeaponManager>().ShootVFX(viewID);
    }
}
