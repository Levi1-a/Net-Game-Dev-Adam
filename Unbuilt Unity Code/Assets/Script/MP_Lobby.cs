using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using UnityEngine.UI;
using MLAPI.NetworkVariable.Collections;
using System;
using MLAPI.Connection;
using MLAPI.SceneManagement;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;

public class MP_Lobby : NetworkBehaviour
{
    [SerializeField] private LPanel[] lobbyPlayers;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Button startGameButton;
    //holds a list of network players
    private NetworkList<MP_PlayerInfo> nwPlayers = new NetworkList<MP_PlayerInfo>();

    [SerializeField] private GameObject chatPrefab;
    void Start()
    {
        if (IsOwner)
        {
            UpdateConnListServerRpc(NetworkManager.LocalClientId);
        }
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
    }

    public override void NetworkStart()
    {
        Debug.Log("StartingServer");
         if(IsClient)
         {
        nwPlayers.OnListChanged += PlayersInfoChanged;
         }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnectedHandle;
            //handle for people connected
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ClientConnectedHandle(client.ClientId);
            }

        }
    }
    private void OnDestroy()
    {
        nwPlayers.OnListChanged -= PlayersInfoChanged;
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnectedHandle;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnectedHandle;
        }
    }


    private void PlayersInfoChanged(NetworkListEvent<MP_PlayerInfo> changeEvent)
    {

        //update the UI lobby
        int index = 0;
        foreach (MP_PlayerInfo connectedplayer in nwPlayers)
        {
            //Debug.Log("Player " + connectedplayer.networkPlayerName + "| Ready: " + connectedplayer.networkPlayerReady);
            lobbyPlayers[index].playerName.text = connectedplayer.networkPlayerName;
            lobbyPlayers[index].readyIcon.SetIsOnWithoutNotify(connectedplayer.networkPlayerReady);
            index++;
        }
        for (; index < 4; index++)
        {
            lobbyPlayers[index].playerName.text = "Player Name";
            lobbyPlayers[index].readyIcon.SetIsOnWithoutNotify(false);
            index++;
        }


        if (IsHost)
        {
            startGameButton.gameObject.SetActive(true);
            startGameButton.interactable = CheckEveryoneReady();

        }




    }


    public void StartGame()
    {
        if (IsServer)
        {
            NetworkSceneManager.OnSceneSwitched += SceneSwitched;
            NetworkSceneManager.SwitchScene("MainGame");
        }
        else
        {
            Debug.Log("You are not the host");
        }
    }


    /*****************************************************
     *                          HANDLES
     * ****************************************************/
    private void HandleClientConnected(ulong clientId)
    {
        if (IsOwner)
        {
            UpdateConnListServerRpc(clientId);
        }
        Debug.Log("A Player has connected ID: " + clientId);
    }

    [ServerRpc]
    private void UpdateConnListServerRpc(ulong clientId)
    {

        nwPlayers.Add(new MP_PlayerInfo(clientId, PlayerPrefs.GetString("PName"), false));

    }
    private void ClientDisconnectedHandle(ulong clientId)
    {
        for (int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if (clientId == nwPlayers[indx].networkClientID)
            {
                nwPlayers.RemoveAt(indx);
                Debug.Log("A Player has left ID: " + clientId);

                break;
            }
        }


    }
    private void ClientConnectedHandle(ulong clientId)
    {

    }

    //Part II the ready button
    [ServerRpc(RequireOwnership = false)]
    private void ReadyUpServerRpc(ServerRpcParams serverRpcParams = default)
    {

        for (int indx = 0; indx < nwPlayers.Count; indx++)
        {
            if (nwPlayers[indx].networkClientID == serverRpcParams.Receive.SenderClientId)
            {
                Debug.Log("Updated with new");
                nwPlayers[indx] = new MP_PlayerInfo(nwPlayers[indx].networkClientID, nwPlayers[indx].networkPlayerName, !nwPlayers[indx].networkPlayerReady);
            }

        }

    }
    public void ReadyButtonPressed()
    {
        ReadyUpServerRpc();
    }

    private void SceneSwitched()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        //spawn a playerprefab for each connected client
        foreach (MP_PlayerInfo tmpClient in nwPlayers)
        {
            //get random spawn point location
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int index = UnityEngine.Random.Range(0, spawnPoints.Length);
            GameObject currentPoint = spawnPoints[index];

            //spawn player
            GameObject playerSpawn = Instantiate(playerPrefab, currentPoint.transform.position, Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientID);
            // Debug.Log("Player spawned for: " + tmpClient.networkPlayerName);

            //add chat ui
            GameObject chatUISpawn = Instantiate(chatPrefab);
            chatUISpawn.GetComponent<NetworkObject>().SpawnWithOwnership(tmpClient.networkClientID);
            chatUISpawn.GetComponent<MP_ChatUIScript>().chatPlayers = nwPlayers;

        }
    }

    private bool CheckEveryoneReady()
    {
        foreach (MP_PlayerInfo player in nwPlayers)
        {
            if (!player.networkPlayerReady)
            {
                return false;
            }
        }
        return true;
    }
}
