using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerScript : MonoBehaviour
{
    //NetworkClient
    public NetworkClient _client;
    void Update()
    {
        if (_client.inGame)
        {
            //Player movement
            NetworkObject.NetworkPlayerInput playerInput = new NetworkObject.NetworkPlayerInput();
            if (Input.GetKeyDown(KeyCode.A))
            {
                playerInput.vertKey = 1;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                playerInput.vertKey = -1;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                playerInput.horKey = 1;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                playerInput.horKey = -1;
            }
            _client.SendPlayerInput(playerInput);
        }
    }
}
