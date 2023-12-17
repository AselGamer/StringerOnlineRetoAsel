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
            bool sendMsg = false;
            //Player movement
            PlayerInputMsg playerInputMsg = new PlayerInputMsg();
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                playerInputMsg.horKey = -1;
                sendMsg = true;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                playerInputMsg.horKey = 1;
                sendMsg = true;
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                playerInputMsg.vertKey = 1;
                sendMsg = true;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                playerInputMsg.vertKey = -1;
                sendMsg = true;
            }

            if (Input.GetKeyUp(KeyCode.Z))
            {
                playerInputMsg.shootKey = 1;
                sendMsg = true;
            }

            if (Input.GetKey(KeyCode.X))
            {
                playerInputMsg.shootKey2 = 1;
                sendMsg = true;
            }

            if (sendMsg)
            {
                _client.SendPlayerInput(playerInputMsg);
            }
        }
    }
}
