using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public int enemiesAlive;
    public int round;
    public GameObject[] spawners;
    public TextMeshProUGUI roundText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI roundsSurvived;
    public GameObject pausePanel;
    public Animator fadePanelAnimator;
    public bool isPaused;
    public bool isGameOver;
    public PhotonView photonView;

    void Start()
    {
        
        isPaused = false;
        isGameOver = false;
        Time.timeScale = 1.0f;
        spawners = GameObject.FindGameObjectsWithTag("Spawners");
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            if (enemiesAlive == 0)
            {
                round++;
                if (round <= 4)
                {
                    NextWave(round * 3);
                }
                else if (round <= 6 && round > 4)
                {
                    NextWave(round * 5);
                }
                else
                {
                    NextWave(35);
                }
                DisplayNextRound(round);
                if (PhotonNetwork.InRoom)
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("currentRound", round);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);

                }
                else
                {
                    DisplayNextRound(round);
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Pause();
        }

    }

    /* if (round <= 4)
                {
                    NextRound(round * 3);
                }
                else if (round <= 6 && round > 4)
                {
                    NextRound(round * 5);
                }
                else
                {
                    NextRound(35);
                }*/
    public void NextWave(int round)
    {
        for (int i = 0; i < round; i++)
        {

            int randomPos = Random.Range(0, spawners.Length);
            GameObject spawnPoint = spawners[randomPos];

            GameObject enemyInstance;
            if (PhotonNetwork.InRoom)
            {
                enemyInstance = PhotonNetwork.Instantiate("Zombie", spawnPoint.transform.position, Quaternion.identity);
            }
            else
            {
                enemyInstance = Instantiate(Resources.Load("Zombie"), spawnPoint.transform.position, Quaternion.identity) as GameObject;
            }
            enemyInstance.GetComponent<EnemyManager>().gameManager = GetComponent<GameManager>();
            enemiesAlive++;
        }
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);

        roundsSurvived.text = round.ToString();
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;
        }
        Cursor.lockState = CursorLockMode.None;
        isGameOver = true;
    }

    public void RestartGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        SceneManager.LoadScene(0);
    }
    public void BackToMenu()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        AudioListener.volume = 1;
        fadePanelAnimator.SetTrigger("FadeIn");
        Invoke("LoadMainMenuScene", 0.5f);

    }
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(0);
    }
    public void Pause()
    {
        if (gameOverPanel.activeSelf != true)
        {
            pausePanel.SetActive(true);
            AudioListener.volume = 0;
            if (!PhotonNetwork.InRoom)
            {
                Time.timeScale = 0;
            }
            Cursor.lockState = CursorLockMode.None;
            isPaused = true;
        }
    }
    public void Resume()
    {
        pausePanel.SetActive(false);
        AudioListener.volume = 1;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;

    }

    void DisplayNextRound(int round)
    {

        roundText.text = $"{round}";
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (photonView.IsMine)
        {
            if (changedProps["currentRound"] != null)
            {
                DisplayNextRound((int)changedProps["currentRound"]);
            }
        }
    }
}
