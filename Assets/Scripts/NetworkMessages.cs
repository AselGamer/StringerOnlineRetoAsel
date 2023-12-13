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
        READY
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
}