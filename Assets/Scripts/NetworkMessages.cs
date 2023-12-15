using NetworkObject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkObject
{
    [System.Serializable]
    public class NetworkObject
    {
        public string id;
    }

    [System.Serializable]
    public class NetworkPlayer : NetworkObject
    {
        public Vector3 posJugador;
        public string nombre;
    }

    [System.Serializable]
    public class NetworkLobbyPlayer : NetworkObject
    {
        public int colorJug;
        public string nombre;
    }
}

namespace NetworkMessages
{
    public enum Commands
    {
        HANDSHAKE,
        LOBBY,
        READY,
        START,
        PLAYER_INPUT,
        PLAYER_MOVEMENT,
        SHOOT_BULLET1,
        SHOOT_BULLET2,
        BACKGROUND_MOVEMENT
    }

    [System.Serializable]
    public class NetworkHeader
    {
        public Commands command;
    }

    [System.Serializable]
    public class HandshakeMsg : NetworkHeader
    {
        public NetworkObject.NetworkPlayer player;
        public HandshakeMsg()
        {
            command = Commands.HANDSHAKE;
            player = new NetworkObject.NetworkPlayer();
        }
    }

    [System.Serializable]
    public class LobbyMsg : NetworkHeader
    {
        public List<NetworkObject.NetworkLobbyPlayer> players;
        public LobbyMsg()
        {
            command = Commands.LOBBY;
            players = new List<NetworkObject.NetworkLobbyPlayer>();
        }
    }

    [System.Serializable]
    public class ReadyMsg : NetworkHeader
    {
        public List<NetworkObject.NetworkPlayer> playerList;
        public ReadyMsg()
        {
            command = Commands.READY;
            playerList = new List<NetworkObject.NetworkPlayer>();
        }
    }

    [System.Serializable]
    public class StartMsg : NetworkHeader
    {
        public StartMsg()
        {
            command = Commands.START;
        }
    }

    [System.Serializable]
    public class PlayerInputMsg : NetworkHeader
    {
        public string id;

        public float vertKey;
        public float horKey;

        public byte shootKey;
        public byte shootKey2;
        public PlayerInputMsg()
        {
            command = Commands.PLAYER_INPUT;
            id = string.Empty;
            vertKey = 0;
            horKey = 0;

            shootKey = 0;
            shootKey2 = 0;
        }
    }

    [System.Serializable]
    public class PlayerMovementMsg : NetworkHeader
    {
        public List<Vector3> playerList;
        public PlayerMovementMsg()
        {
            command = Commands.PLAYER_MOVEMENT;
            playerList = new List<Vector3>();
        }
    }

    [System.Serializable]
    public class BackgroundMovementMsg : NetworkHeader
    {
        public float[] backGroundPos;
        public BackgroundMovementMsg()
        {
            command = Commands.BACKGROUND_MOVEMENT;
            backGroundPos = new float[2];
        }
    }

    [System.Serializable]
    public class ShootBulletMsg : NetworkHeader
    {
        public int shootingPlayer;
        public ShootBulletMsg()
        {
            command = Commands.SHOOT_BULLET1;
            shootingPlayer = 0;
        }
    }
}