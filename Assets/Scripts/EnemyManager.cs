using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

public class EnemyManager : MonoBehaviour
{
    public GameObject player;
    private GameObject[] playersInScene;
    public Animator enemyAnimator;
    //public float damage = 20f;
    public float health = 100f;

    public GameManager gameManager;
    public Slider healthBar;
    public bool playerInReach;
    public float attackDelayTimer;
    public float howMuchEarlierStartAttackAnim;
    public float delayBetweenAttacks;

    public AudioSource enemyAudioSource;
    public AudioClip[] growlAudioClip;

    public PhotonView photonView;
    void Start()
    {
        playersInScene = GameObject.FindGameObjectsWithTag("Player");
        enemyAudioSource = GetComponent<AudioSource>();
        healthBar.maxValue = health;
        healthBar.value = health;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!enemyAudioSource.isPlaying)
        {
            enemyAudioSource.clip = growlAudioClip[Random.Range(0,growlAudioClip.Length)];
            enemyAudioSource.Play();
        }

        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            return;
        }
        GetClosestPlayer();
        if (player != null)
        {
            GetComponent<NavMeshAgent>().destination = player.transform.position;

            healthBar.transform.LookAt(player.transform);
        }
        if (GetComponent<NavMeshAgent>().velocity.magnitude > 1)
        {
            enemyAnimator.SetBool("IsRunning", true);
        }
        else
        {
            enemyAnimator.SetBool("IsRunning", false);
        }
        
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == player)
        {
            playerInReach = true;
        }
    }
    private void OnCollisionStay(Collision other)
    {
        if (playerInReach)
        {
            attackDelayTimer += Time.deltaTime;

            if(attackDelayTimer >= delayBetweenAttacks-howMuchEarlierStartAttackAnim && attackDelayTimer <= delayBetweenAttacks)
            {
                enemyAnimator.SetTrigger("IsAttacking");
            }

            if(attackDelayTimer >= delayBetweenAttacks) {
                player.GetComponent<PlayerManager>().Hit(Random.Range(5, 20));
                attackDelayTimer = 0;
            }
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if(other.gameObject == player)
        {
            playerInReach = false;
            attackDelayTimer = 0;
        }
    }
    public void Hit(float damage)
    {
        
        if (PhotonNetwork.InRoom)
        {
            photonView.RPC("TakeDamage", RpcTarget.All, damage, photonView.ViewID);
        }
        else
        {
            TakeDamage(damage, photonView.ViewID);
        }
    }

    [PunRPC]
    public void TakeDamage(float damage, int viewID)
    {
        if (photonView.ViewID == viewID)
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0)
            {

                enemyAnimator.SetTrigger("IsDead");
                transform.position = new Vector3(transform.localPosition.x, transform.localPosition.y - 3.0f, transform.localPosition.z);
                if (!PhotonNetwork.InRoom || (PhotonNetwork.IsMasterClient && photonView.IsMine))
                {
                    gameManager.enemiesAlive--;
                }
                player.GetComponent<PlayerManager>().UpdatePoints(100);
                Destroy(gameObject, 5f);
                Destroy(GetComponent<NavMeshAgent>());
                Destroy(GetComponent<EnemyManager>());
                Destroy(GetComponent<CapsuleCollider>());
            }
            else
            {

                player.GetComponent<PlayerManager>().UpdatePoints(10);
            }
        }
    }
    private void GetClosestPlayer()
    {
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject p in playersInScene)
        {
            if (p != null)
            {
                float distance = Vector3.Distance(p.transform.position, currentPosition);
                if (distance < minDistance)
                {
                    player = p;
                    minDistance = distance;
                }
            }
        }
    }
}
