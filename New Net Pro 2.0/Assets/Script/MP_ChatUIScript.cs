using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MP_ChatUIScript : NetworkBehaviour

{
    public Text chatText = null;
    public InputField chatInput = null;

    NetworkVariableString messages = new NetworkVariableString("Temp");

    public NetworkList<MP_PlayerInfo> chatPlayers;
    private string playerName = "N/A";
    // Start is called before the first frame update
    void Start()
    {
        messages.OnValueChanged += updateUIClientRpc;
        foreach (MP_PlayerInfo player in chatPlayers)
        {
            if (NetworkManager.LocalClientId == player.networkClientID)
            {
                playerName = player.networkPlayerName;
            }
        }
    }



    public void handleSend()
    {
        if (!IsServer)
        {
            sendMessageServerRpc(chatInput.text);
        }
        else
        {
            messages.Value += "\n" + playerName + " says: " + chatInput.text;
        }

    }
    [ClientRpc]
    private void updateUIClientRpc(string previousValue, string newValue)
    {
        chatText.text += newValue.Substring(previousValue.Length, newValue.Length - previousValue.Length);
    }

    [ServerRpc]
    private void sendMessageServerRpc(string text, ServerRpcParams svrParam = default)
    {
        foreach (MP_PlayerInfo player in chatPlayers)
        {
            if (svrParam.Receive.SenderClientId == player.networkClientID)
            {
                playerName = player.networkPlayerName;
            }
        }
        messages.Value += "\n" + playerName + " says: " + text;
    }
}
