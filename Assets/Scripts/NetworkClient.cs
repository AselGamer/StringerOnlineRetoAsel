using NetworkMessages;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class NetworkClient : MonoBehaviour
{
    //Connection
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIp;
    public ushort serverPort;

    //Private values
    private string idPlayer;
    private string playerName;
    private bool conectar;
    private float offsetLobby = -100f;

    //Inputs
    public InputField ipInput;
    public InputField nombreInput;
    public Button connectButton;
    public Button startButton;

    //Canvas
    public Canvas connectCanvas;
    public Canvas waitCanvas;

    //Prefabs
    public GameObject waitingPlayer;

    //Players In Lobby
    private List<GameObject> simulatedPlayers = new List<GameObject>();

    //Player Color Array
    public Sprite[] arrayColorJug;


    public void Conectar()
    {
        playerName = nombreInput.text;
        serverIp = ipInput.text;
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndpoint.Parse(serverIp, serverPort);
        m_Connection = m_Driver.Connect(endpoint);
        conectar = m_Connection.IsCreated;
        connectButton.enabled = conectar;
    }

    void OnDestroy()
    {
        m_Driver.Disconnect(m_Connection);
        m_Driver.ScheduleUpdate().Complete();
        m_Driver.Dispose();
    }

    void Update()
    {
        if (conectar)
        {
            m_Driver.ScheduleUpdate().Complete();
            if (!m_Connection.IsCreated)
            {
                ipInput.text = string.Empty;
                nombreInput.text = string.Empty;
                connectButton.enabled = true;
                conectar = false;
                return;
            }

            Unity.Collections.DataStreamReader stream;
            NetworkEvent.Type cmd = m_Connection.PopEvent(m_Driver, out stream);
            while (cmd != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    OnConnect();
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    OnDisconnect();
                }
                cmd = m_Connection.PopEvent(m_Driver, out stream);
            }
        }
    }

    private void OnConnect()
    {
        Debug.Log("Connectado");
        connectCanvas.gameObject.SetActive(false);
        waitCanvas.gameObject.SetActive(true);
    }

    private void OnDisconnect()
    {
        m_Connection = default(NetworkConnection);
    }

    private void OnData(DataStreamReader stream)
    {
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length, Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);
        switch (header.command)
        {
            case Commands.HANDSHAKE:
                HandshakeMsg handshakeRecibido = JsonUtility.FromJson<HandshakeMsg>(recMsg);
                HandshakeMsg handshakeEnviar = new HandshakeMsg();
                idPlayer = handshakeRecibido.player.id;
                handshakeEnviar.player.nombre = playerName;
                handshakeEnviar.player.id = idPlayer;
                SendToServer(handshakeEnviar);
                break;
            case Commands.LOBBY:
                offsetLobby = -100f;
                simulatedPlayers.ForEach(player => Destroy(player));
                simulatedPlayers.Clear();
                LobbyMsg lobbyMsg = JsonUtility.FromJson<LobbyMsg>(recMsg);
                foreach (var player in lobbyMsg.players)
                {
                    waitingPlayer.GetComponentInChildren<TextMeshProUGUI>().text = player.nombre;
                    waitingPlayer.GetComponentInChildren<SpriteRenderer>().sprite = arrayColorJug[player.colorJug];
                    waitingPlayer.transform.position = new Vector3(offsetLobby, 0f);
                    simulatedPlayers.Add(Instantiate(waitingPlayer, waitCanvas.transform));
                    offsetLobby += 100f;
                }
                startButton.gameObject.SetActive(lobbyMsg.players.Count > 1);
                break;
            case Commands.READY:

                break;
            default:
                Debug.Log("Mensaje Desconocido");
                break;
        }
    }

    private void SendToServer(object message)
    {
        string messageJSON = JsonUtility.ToJson(message);
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        NativeArray<byte> bytes = new NativeArray<byte>(System.Text.Encoding.ASCII.GetBytes(messageJSON), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    public void Disconnect()
    {
        m_Driver.Disconnect(m_Connection);
        m_Driver.ScheduleUpdate().Complete();
        m_Connection = default(NetworkConnection);
        simulatedPlayers.ForEach(player => Destroy(player));
        simulatedPlayers.Clear();
        conectar = false;
        connectButton.enabled = true;
        serverIp = "";
        connectCanvas.gameObject.SetActive(true);
        waitCanvas.gameObject.SetActive(false);
    }
}
