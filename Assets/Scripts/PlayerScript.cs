using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkMessages;


public class PlayerScript : MonoBehaviour
{
    //NetworkClient
    public NetworkClient _client;
    void Update()
    {
        if (_client.inGame)
        {
            //Player movement
            PlayerInputMsg playerInputMsg = new PlayerInputMsg();
            if (Input.GetKey(KeyCode.A))
            {
                playerInputMsg.horKey = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                playerInputMsg.horKey = 1;
            }

            if (Input.GetKey(KeyCode.W))
            {
                playerInputMsg.vertKey = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                playerInputMsg.vertKey = -1;
            }
            
            _client.SendPlayerInput(playerInputMsg);
        }
    }
}
