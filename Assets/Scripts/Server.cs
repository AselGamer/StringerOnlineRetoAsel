using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using NetworkMessages;
using TMPro;
using Unity.VisualScripting;

public class Server : MonoBehaviour
{
    //Connection
    public NetworkDriver m_Driver;
    public ushort serverPort;
    public NativeList<NetworkConnection> m_Connections;
    public List<NetworkObject.NetworkPlayer> m_Players;

    //Game simulation
    public GameObject[] jugadoresSimulados;
    public GameObject gameBackground;

    //Prefabs
    public GameObject jugadorPrefab;
    public Canvas gameCanvas;
    private const float SPAWN_POS = -265f;

    //Player movement
    public float speed;
    public Vector3 _direction;

    //Private values
    private int nextId = 0;
    private bool inGame = false;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(serverPort);
        if (m_Driver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind to port " + serverPort);
        }
        else
        {
            m_Driver.Listen();
        }
    }

    void OnDestroy()
    {
        if (m_Driver.IsCreated)
        {
            m_Driver.Dispose();
            m_Connections.Dispose();
        }
    }


    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        NetworkConnection c = m_Driver.Accept();
        while (c != default(NetworkConnection))
        {
            OnConnect(c);
            c = m_Driver.Accept();
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            while (cmd != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream, i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    OnDisconnect(i);
                }
                cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            }
        }
    }

    void OnConnect(NetworkConnection c)
    {
        m_Connections.Add(c);
        Debug.Log("Accepted connection");

        HandshakeMsg m = new HandshakeMsg();
        m.player.id = nextId.ToString();
        nextId++;
        SendToClient(m, c);
    }

    void OnDisconnect(int i)
    {
        m_Connections[i] = default(NetworkConnection);
        m_Driver.Disconnect(m_Connections[i]);
        Debug.Log("Jugador " + m_Players[i].nombre + " desconectado");
        m_Players.RemoveAt(i);
        SendPlayerLobby();
    }

    private void SendToClient(object message, NetworkConnection c)
    {
        string messageJSON =  JsonUtility.ToJson(message);
        DataStreamWriter writer;
        m_Driver.BeginSend(NetworkPipeline.Null, c, out writer);
        NativeArray<byte> bytes = new NativeArray<byte>(System.Text.Encoding.ASCII.GetBytes(messageJSON), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    private void OnData(DataStreamReader stream, int numJugador)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = System.Text.Encoding.ASCII.GetString(bytes);
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);

        switch (header.command)
        {
            case Commands.HANDSHAKE:
                HandshakeMsg handShakeRecibido = JsonUtility.FromJson<HandshakeMsg>(recMsg);
                Debug.Log("se ha conectado: " + handShakeRecibido.player.nombre);
                if (inGame)
                {
                    int lastConnection = m_Connections.Length - 1;
                    m_Driver.Disconnect(m_Connections[lastConnection]);
                    m_Driver.ScheduleUpdate().Complete();
                    m_Connections[lastConnection] = default(NetworkConnection);
                    Debug.Log("Partida en proceso desconectando jugador: " + handShakeRecibido.player.nombre);
                    return;
                }

                NetworkObject.NetworkPlayer nuevoJugador = new NetworkObject.NetworkPlayer();
                float vertOffset = (nextId - 1) * 100f;
                nuevoJugador.id = handShakeRecibido.player.id;
                nuevoJugador.nombre = handShakeRecibido.player.nombre;
                nuevoJugador.posJugador = new Vector3(SPAWN_POS, vertOffset ,0f);
                m_Players.Add(nuevoJugador);
                SendPlayerLobby();
                break;
            case Commands.START:
                if (inGame)
                {
                    return;
                }
                ReadyMsg readyMsg = new ReadyMsg();
                readyMsg.playerList = m_Players;
                foreach (var connection in m_Connections)
                {
                    SendToClient(readyMsg, connection);
                }
                inGame = true;
                gameBackground.SetActive(true);
                StartGameServer();
                break;
            case Commands.PLAYER_INPUT:
                if (inGame)
                {
                    PlayerInputMsg playerInput = JsonUtility.FromJson<PlayerInputMsg>(recMsg);
                    GameObject jugadorInput = jugadoresSimulados[numJugador];
                    bool shootBullet = false;
                    _direction = new Vector3(playerInput.horKey, playerInput.vertKey, 0f);
                    if (jugadorInput != null)
                    {
                        jugadorInput.transform.Translate(_direction * speed * Time.deltaTime);
                        PlayerMovementMsg playerMovementMsg = new PlayerMovementMsg();
                        foreach (var jugador in jugadoresSimulados)
                        {
                            playerMovementMsg.playerList.Add(jugador.transform.position);
                        }
                        if (playerInput.shootKey == 1)
                        {
                            GameObject pooledObj = jugadorInput.GetComponent<PlayerGameScript>().GetPooledObject();
                            if (pooledObj != null)
                            {
                                pooledObj.transform.position = new Vector3(jugadorInput.transform.position.x + 1.5f, jugadorInput.transform.position.y);
                                pooledObj.SetActive(true);
                                shootBullet = true;
                            }
                        }
                        foreach (var connection in m_Connections)
                        {
                            SendToClient(playerMovementMsg, connection);
                            if (shootBullet)
                            {
                                ShootBulletMsg shootMsg = new ShootBulletMsg();
                                shootMsg.shootingPlayer = numJugador;
                                SendToClient(shootMsg, connection);
                            }
                        }
                    }
                }
                break;
            default:

                break;
        }
    }

    private void SendPlayerLobby()
    {
        List<NetworkObject.NetworkLobbyPlayer> lobbyPlayers = new List<NetworkObject.NetworkLobbyPlayer>();
        foreach (var player in m_Players)
        {
            var lobbyPlayer = new NetworkObject.NetworkLobbyPlayer();
            lobbyPlayer.nombre = player.nombre;
            lobbyPlayer.colorJug = m_Players.IndexOf(player);
            lobbyPlayers.Add(lobbyPlayer);
        }
        LobbyMsg lobbyMsg = new LobbyMsg();
        foreach (var connection in m_Connections)
        {
            lobbyMsg.players = lobbyPlayers;
            if (connection.IsCreated)
            {
                SendToClient(lobbyMsg, connection);
            }            
        }
    }

    private void StartGameServer()
    {
        int playerCount = m_Players.Count;
        System.Array.Resize(ref jugadoresSimulados, playerCount);
        for (int i = 0; i < playerCount; i++)
        {
            jugadorPrefab.transform.position = m_Players[i].posJugador;
            jugadorPrefab.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = m_Players[i].nombre;
            jugadorPrefab.gameObject.name = m_Players[i].id;
            jugadoresSimulados[i] = Instantiate(jugadorPrefab, gameCanvas.transform);
        }
        Debug.Log("Partida Empezada");
    }

    public void SendBackground(float[] posBack)
    {
        BackgroundMovementMsg backgroundMovementMsg = new BackgroundMovementMsg();
        backgroundMovementMsg.backGroundPos = posBack;
        foreach (var connection in m_Connections)
        {
            SendToClient(backgroundMovementMsg, connection);
        }
    }

}
