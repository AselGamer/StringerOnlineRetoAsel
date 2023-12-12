using NetworkMessages;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIp;
    public ushort serverPort;

    public string idPlayer;
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndpoint.Parse(serverIp, serverPort);
        m_Connection = m_Driver.Connect(endpoint);
        
    }

    void OnDestroy()
    {
        m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();
        if (!m_Connection.IsCreated)
        {
            return;
        }
        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connectado");
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
                handshakeEnviar.player.nombre = "Asel " + idPlayer;
                SendToServer(handshakeEnviar);
                break;
        }
    }

    private void SendToServer(System.Object message)
    {
        string messageJSON = JsonUtility.ToJson(message);
        DataStreamWriter writer;
        m_Driver.BeginSend(m_Connection, out writer);
        NativeArray<byte> bytes = new NativeArray<byte>(System.Text.Encoding.ASCII.GetBytes(messageJSON), Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }
}
