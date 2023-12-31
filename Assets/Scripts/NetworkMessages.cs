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

    [System.Serializable]
    public class NetworkSpawnPoint : NetworkObject
    {
        public float posX, posY;
        public string tag;
    }

    [System.Serializable]
    public class NetworkBellPos : NetworkObject
    {
        public float posX, posY;
        public int idCampana;
        public int bellStage;
        public bool isActive;
    }

    [System.Serializable]
    public class NetworkDestroyProjectile : NetworkObject
    {
        public int idHitter;
        public string hitterType;
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
        BACKGROUND_MOVEMENT,
        UPDATE_POINTS,
        UPDATE_BELL,
        DESTROY_PROJECTILE,
        RESPAWN_ENEMIES,
        UPDATE_COUNTDOWN,
        GAME_END
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
        public List<NetworkSpawnPoint> spawnList;
        public ReadyMsg()
        {
            command = Commands.READY;
            playerList = new List<NetworkObject.NetworkPlayer>();
            spawnList = new List<NetworkSpawnPoint>();
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

    [System.Serializable]
    public class ShootBullet2Msg : NetworkHeader
    {
        public int shootingPlayer;
        public ShootBullet2Msg()
        {
            command = Commands.SHOOT_BULLET2;
            shootingPlayer = 0;
        }
    }

    [System.Serializable]
    public class UpdatePointsMsg : NetworkHeader
    {
        public int points, playerToUpdate;
        public UpdatePointsMsg()
        {
            command = Commands.UPDATE_POINTS;
            points = 0;
            playerToUpdate = 0;
        }
    }

    [System.Serializable]
    public class UpdateBellMsg : NetworkHeader
    {
        public NetworkBellPos networkBellPos;
        public UpdateBellMsg()
        {
            command = Commands.UPDATE_BELL;
            networkBellPos = new NetworkBellPos();
        }
    }

    [System.Serializable]
    public class DestroyProjectileMsg : NetworkHeader
    {
        public NetworkDestroyProjectile networkKillProjectile;
        public DestroyProjectileMsg()
        {
            command = Commands.DESTROY_PROJECTILE;
            networkKillProjectile = new NetworkDestroyProjectile();
        }
    }

    [System.Serializable]
    public class RespawnEnemiesMsg : NetworkHeader
    {
        public RespawnEnemiesMsg()
        {
            command = Commands.RESPAWN_ENEMIES;
        }
    }

    [System.Serializable]
    public class UpdateCountMsg : NetworkHeader
    {
        public int count;
        public UpdateCountMsg()
        {
            command = Commands.UPDATE_COUNTDOWN; 
            count = 0;
        }
    }

    [System.Serializable]
    public class GameEndMsg : NetworkHeader
    {
        public int[] puntsArray;
        public List<NetworkObject.NetworkLobbyPlayer> playerList;
        public GameEndMsg()
        { 
            command = Commands.GAME_END;
            puntsArray = new int[0];
            playerList = new List<NetworkObject.NetworkLobbyPlayer>();
        }
    }
}