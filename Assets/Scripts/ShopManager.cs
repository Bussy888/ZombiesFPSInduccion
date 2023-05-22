using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public int price = 500;
    public TextMeshProUGUI priceNumber;
    public TextMeshProUGUI priceText;

    private PlayerManager playerManager;
    private bool playerIsReach;

    public bool isHealthShop;
    public bool isAmmoShop;

    // Start is called before the first frame update
    void Start()
    {
        priceText.text = price.ToString();
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E)) 
        {
            BuyShop();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIsReach = true;
            priceText.gameObject.SetActive(true);
            priceNumber.gameObject.SetActive(true);
            playerManager=other.GetComponent<PlayerManager>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerIsReach = false;
            priceText.gameObject.SetActive(false);
            priceNumber.gameObject.SetActive(false);
        }
    }
    public void BuyShop()
    {
        if (playerIsReach)
        {
            if (playerManager.totalPoints >= price)
            {
                playerManager.UpdatePoints(-price);
                if (isHealthShop)
                {
                    playerManager.health = playerManager.healtCap;
                    playerManager.healthText.text = playerManager.health.ToString();
                    playerManager.healthText.color = Color.white;
                    playerManager.healthSlider.value = playerManager.health;
                }
                if (isAmmoShop)
                {
                    foreach(Transform child in playerManager.weaponHolder.transform)
                    {
                        WeaponManager weaponManager = child.GetComponent<WeaponManager>();
                        weaponManager.currentAmmo = weaponManager.maxAmmo;
                        weaponManager.reserveAmmo = weaponManager.reserveAmmoCap;
                        StartCoroutine(weaponManager.Reload(weaponManager.reloadTime));
                  

                    }
                }
            }
        }
    }
}
