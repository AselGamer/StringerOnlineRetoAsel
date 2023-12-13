using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using NetworkMessages;

public class Server : MonoBehaviour
{
    //Connection
    public NetworkDriver m_Driver;
    public ushort serverPort;
    public NativeList<NetworkConnection> m_Connections;
    public List<NetworkObject.NetworkPlayer> m_Players;

    //Private values
    private int nextId = 0;
    private bool onLobby = false;

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
        //Bottleneck might be necessary though
        /*
        if (!c.IsCreated)
        {
            return;
        }
        */
        string messageJSON =  JsonUtility.ToJson(message);
        DataStreamWriter writer;
        m_Driver.BeginSend(NetworkPipeline.Null, c, out writer);
        Debug.Log(messageJSON);
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

                NetworkObject.NetworkPlayer nuevoJugador = new NetworkObject.NetworkPlayer();
                nuevoJugador.id = handShakeRecibido.player.id;
                nuevoJugador.nombre = handShakeRecibido.player.nombre;
                m_Players.Add(nuevoJugador);
                SendPlayerLobby();
                break;

            default:

                break;
        }
    }

    private void SendPlayerLobby()
    {
        //Could be summed up with a single for loop but this is more elegant
        // TODO: turn this into a for loop
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
        onLobby = true;
    }

}
