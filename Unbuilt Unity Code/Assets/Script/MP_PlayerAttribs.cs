using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MP_PlayerAttribs : NetworkBehaviour
{
    public Slider hpBar;

    private float maxHp = 100f;
    private float damageVal = 20f;

    public NetworkVariableFloat currentHp = new NetworkVariableFloat(100f);
    [SerializeField] public NetworkVariableBool powerUp = new NetworkVariableBool(false);

    [SerializeField] private GameObject playerPrefab;

    public NetworkVariableInt deaths = new NetworkVariableInt(0);
    public NetworkVariableInt kills = new NetworkVariableInt(0);
    // Update is called once per frame
    void Update()
    {
        hpBar.value = currentHp.Value / maxHp;

        if (currentHp.Value < 0)
        {
            RespawnPlayerServerRpc();
            ResetPlayerClientRpc();
            if (IsOwner)
            {
                Debug.Log("You have Died");
            }
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && IsOwner)
        {
            if(collision.gameObject.GetComponent<MP_BulletScript>().spawnerPlayerId != OwnerClientId)
            {
                if(currentHp.Value - damageVal < 0)
                {
                    increseKillCountServerRpc(collision.gameObject.GetComponent<MP_BulletScript>().spawnerPlayerId);
                }

                TakeDamageServerRpc(damageVal);
                Destroy(collision.gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("MedKit") && IsOwner)
        {
            Debug.Log("I am Healed");
            HealDamageServerRpc();
        }
        else if (collision.gameObject.CompareTag("powerUp") && IsOwner)
        {
            Debug.Log("I have the power!");
            damageVal = damageVal / 2;
            powerUp.Value = true;
        }
    }



    [ServerRpc]
    private void TakeDamageServerRpc(float damage, ServerRpcParams svrParams = default)
    {
        currentHp.Value -= damage;
        if (currentHp.Value < 0 && OwnerClientId == svrParams.Receive.SenderClientId)
        {
            deaths.Value++;
        }
    }

    [ServerRpc]
    private void HealDamageServerRpc()
    {
        currentHp.Value += 25f;
        if (currentHp.Value > maxHp)
        {
            currentHp.Value = maxHp;
        }

    }

    [ServerRpc]
    private void RespawnPlayerServerRpc()
    {
        //set health to 100%
        currentHp.Value = maxHp;
    }

    [ClientRpc]
    private void ResetPlayerClientRpc()
    {
        //reset player position to spawn point
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int index = UnityEngine.Random.Range(0, spawnPoints.Length);


        GetComponent<CharacterController>().enabled = false;
        transform.position = spawnPoints[index].transform.position;
        GetComponent<CharacterController>().enabled = true;
    }

    [ServerRpc]
    private void increseKillCountServerRpc(ulong spawnerPlayerId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject playerObj in players)
        {
            if(playerObj.GetComponent<NetworkObject>().OwnerClientId == spawnerPlayerId)
            {
                playerObj.GetComponent<MP_PlayerAttribs>().kills.Value++;
            }
        }
    }
}
