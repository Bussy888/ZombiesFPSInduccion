using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkingManager : MonoBehaviourPunCallbacks
{

    public Button multiplayerButton;
    void Start()
    {
        Debug.Log("Conexion A un servidor");
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Unirnos a un lobby");
        PhotonNetwork.JoinLobby();
        //base.OnConnectedToMaster();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Estamos Listos para Multijugador");
        // base.OnJoinedLobby();
        multiplayerButton.interactable = true;

    }
    public void FindMatch()
    {
        Debug.Log("Buscando Sala");
        PhotonNetwork.JoinRandomRoom();

    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Creando Sala...");
        MakeRoom();
    }
    private void MakeRoom()
    {
        int randomRoomName = Random.Range(0, 5000);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 6,
            PublishUserId = true
        };
        PhotonNetwork.CreateRoom($"RoomName_{randomRoomName}", roomOptions);
        Debug.Log($"Sala Creada: {randomRoomName}");

    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Cargando Escena del Juego");
        PhotonNetwork.LoadLevel(1);
    }
}
