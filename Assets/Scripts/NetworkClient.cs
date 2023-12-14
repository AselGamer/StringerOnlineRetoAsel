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

    //Public values
    public bool inGame = false;

    //Inputs
    public InputField ipInput;
    public InputField nombreInput;
    public Button connectButton;
    public Button startButton;

    //Canvas
    public Canvas connectCanvas;
    public Canvas waitCanvas;
    public Canvas gameCanvas;

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
        //Too lazy to move this
        // TODO: move this
        Disconnect();
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
                ClearPlayers();
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
                waitCanvas.gameObject.SetActive(false);
                ClearPlayers();
                gameCanvas.gameObject.SetActive(true);
                inGame = true;
                break;
            case Commands.PLAYER_MOVEMENT:
                // TODO: implement
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
        ClearPlayers();
        conectar = false;
        inGame = false;
        connectButton.enabled = true;
        startButton.enabled = true;
        serverIp = "";
        connectCanvas.gameObject.SetActive(true);
        waitCanvas.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        startButton.enabled = false;
        SendToServer(new StartMsg());
    }

    private void ClearPlayers()
    {
        simulatedPlayers.ForEach(player => Destroy(player));
        simulatedPlayers.Clear();
    }

    public void SendPlayerInput(PlayerInputMsg playerInput)
    {
        playerInput.id = idPlayer;
        if (playerInput.vertKey != 0 || playerInput.horKey != 0)
        {
            Debug.Log("Hor: " + playerInput.horKey);
            Debug.Log("Ver: " + playerInput.vertKey);
        }
        SendToServer(playerInput);
    }
}
