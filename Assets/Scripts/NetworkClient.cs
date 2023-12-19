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
    private bool gameOver = false;
    private float offsetLobby = -100f;
    private const float SPAWN_POS = -265f;

    //Public values
    public bool inGame = false;
    public GameObject[] simulatedPlayersGame;
    public EnemySpawnerScript enemySpawnerScript;
    public GameObject[] spawnPointEnemigos;
    public GameObject timeText;
    public GameObject waitText;

    //Inputs
    public InputField ipInput;
    public InputField nombreInput;
    public Button connectButton;
    public Button startButton;
    public Button disconnectButton;

    //Canvas
    public Canvas connectCanvas;
    public Canvas waitCanvas;
    public Canvas gameCanvas;

    //Game Background
    public GameObject gameBackground;

    //Prefabs
    public GameObject waitingPlayer;
    public GameObject gamePlayer;
    public GameObject gameOverPoints;

    //Players In Lobby
    private List<GameObject> simulatedPlayers = new List<GameObject>();

    //Player Color Array
    public Sprite[] arrayColorJug;

    //Player Points
    public List<GameObject> playerPoints;
    public GameObject playerPointsPrefab;


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
                if (!gameOver)
                {
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
                }
                break;
            case Commands.READY:
                waitCanvas.gameObject.SetActive(false);
                ClearPlayers();
                ReadyMsg readyMsg = JsonUtility.FromJson<ReadyMsg>(recMsg);
                foreach (var spawnPoint in readyMsg.spawnList)
                {
                    int tipoEnemigo = -1;
                    int.TryParse(spawnPoint.tag[spawnPoint.tag.Length - 1]+"", out tipoEnemigo);
                    var tmpSpawn = spawnPointEnemigos[tipoEnemigo];
                    tmpSpawn.transform.localPosition = new Vector3(spawnPoint.posX, spawnPoint.posY);
                    Instantiate(tmpSpawn, enemySpawnerScript.mapaJuego);
                }
                int playerCount = readyMsg.playerList.Count;
                float vertOffset = 0f;
                float offsetPunts = -230f;
                Array.Resize(ref simulatedPlayersGame, playerCount);
                for (int i = 0; i < playerCount; i++)
                {
                    var tmpPrefab = playerPointsPrefab;
                    tmpPrefab.transform.localPosition = new Vector3(offsetPunts, 150f);
                    offsetPunts += 200f;
                    playerPoints.Add(Instantiate(tmpPrefab ,gameCanvas.transform));
                    gamePlayer.transform.position = new Vector3(SPAWN_POS, vertOffset, 0f);
                    gamePlayer.GetComponentInChildren<TextMeshProUGUI>().text = readyMsg.playerList[i].nombre;
                    gamePlayer.GetComponentInChildren<SpriteRenderer>().sprite = arrayColorJug[i];
                    gamePlayer.name = readyMsg.playerList[i].id;
                    simulatedPlayersGame[i] = Instantiate(gamePlayer, gameCanvas.transform);
                    vertOffset += 100f;
                }
                gameCanvas.gameObject.SetActive(true);
                enemySpawnerScript.findSpawners();
                inGame = true;
                break;
            case Commands.PLAYER_MOVEMENT:
                PlayerMovementMsg playerMovementMsg = JsonUtility.FromJson<PlayerMovementMsg>(recMsg);
                int simPlayerCount = playerMovementMsg.playerList.Count;
                for (int i = 0; i < simPlayerCount; i++)
                {
                    simulatedPlayersGame[i].transform.position = playerMovementMsg.playerList[i];
                }
                break;
            case Commands.SHOOT_BULLET1:
                ShootBulletMsg shootMsg = JsonUtility.FromJson<ShootBulletMsg>(recMsg);
                GameObject shootingPlayer = simulatedPlayersGame[shootMsg.shootingPlayer];
                GameObject pooledObj = shootingPlayer.GetComponent<PlayerGameScript>().GetPooledObject();
                if (pooledObj != null)
                {
                    pooledObj.transform.position = new Vector3(shootingPlayer.transform.position.x + 1.5f, shootingPlayer.transform.position.y);
                    pooledObj.SetActive(true);
                }
                break;
            case Commands.SHOOT_BULLET2:
                ShootBullet2Msg shoot2Msg = JsonUtility.FromJson<ShootBullet2Msg>(recMsg);
                GameObject shootingPlayer2 = simulatedPlayersGame[shoot2Msg.shootingPlayer];
                GameObject pooledObj2 = shootingPlayer2.GetComponent<PlayerGameScript>().GetPooledObjectHeart();
                if (pooledObj2 != null)
                {
                    pooledObj2.transform.position = new Vector3(shootingPlayer2.transform.position.x, shootingPlayer2.transform.position.y + 0.5f);
                    pooledObj2.SetActive(true);
                }
                break;
            case Commands.BACKGROUND_MOVEMENT:
                BackgroundMovementMsg backgroundMovementMsg = JsonUtility.FromJson<BackgroundMovementMsg>(recMsg);
                gameBackground.transform.localPosition = new Vector3(backgroundMovementMsg.backGroundPos[0], backgroundMovementMsg.backGroundPos[1]);
                break;
            case Commands.UPDATE_POINTS:
                UpdatePointsMsg updatePointsMsg = JsonUtility.FromJson<UpdatePointsMsg>(recMsg);
                playerPoints[updatePointsMsg.playerToUpdate].GetComponentInChildren<TextMeshPro>().text = updatePointsMsg.points+"";
                break;
            case Commands.UPDATE_BELL:
                UpdateBellMsg updateBellMsg = JsonUtility.FromJson<UpdateBellMsg>(recMsg);
                GameObject[] campanasSimuladas = GameObject.FindGameObjectsWithTag("campana");
                int campanasLength = campanasSimuladas.Length;
                for (int i = 0; i < campanasLength; i++)
                {
                    int tmpIdCampana = campanasSimuladas[i].GetComponent<BellScript>().idCampana;
                    if (tmpIdCampana == updateBellMsg.networkBellPos.idCampana)
                    {
                        campanasSimuladas[i].GetComponent<BellScript>().bellStage = updateBellMsg.networkBellPos.bellStage;
                        campanasSimuladas[i].transform.position = new Vector3(updateBellMsg.networkBellPos.posX, updateBellMsg.networkBellPos.posY);
                        campanasSimuladas[i].SetActive(updateBellMsg.networkBellPos.isActive);
                    }
                }
                break;
            case Commands.DESTROY_PROJECTILE:
                DestroyProjectileMsg destroyProjectileMsg = JsonUtility.FromJson<DestroyProjectileMsg>(recMsg);
                GameObject[] projectilesSimulados = GameObject.FindGameObjectsWithTag(destroyProjectileMsg.networkKillProjectile.hitterType);
                int projectilesLength = projectilesSimulados.Length;
                for (int i = 0; i < projectilesLength; i++)
                {
                    if (destroyProjectileMsg.networkKillProjectile.hitterType == "heart")
                    {
                        var tmpScript = projectilesSimulados[i].GetComponent<HeartScript>();
                        if (projectilesSimulados[i].tag == destroyProjectileMsg.networkKillProjectile.hitterType && tmpScript.idHeart == destroyProjectileMsg.networkKillProjectile.idHitter)
                        {
                            projectilesSimulados[i].SetActive(false);
                        }
                    } 

                    if(destroyProjectileMsg.networkKillProjectile.hitterType == "bala")
                    {
                        var tmpScript = projectilesSimulados[i].GetComponent<BulletScript>();
                        if (projectilesSimulados[i].tag == destroyProjectileMsg.networkKillProjectile.hitterType && tmpScript.idBullet == destroyProjectileMsg.networkKillProjectile.idHitter)
                        {
                            projectilesSimulados[i].SetActive(false);
                        }
                    }
                    
                }
                break;
            case Commands.RESPAWN_ENEMIES:
                enemySpawnerScript.ReactivateSpawners();
                break;
            case Commands.UPDATE_COUNTDOWN:
                UpdateCountMsg updateCountMsg = JsonUtility.FromJson<UpdateCountMsg>(recMsg);
                timeText.GetComponent<TextMeshProUGUI>().text = updateCountMsg.count + "";
                break;
            case Commands.GAME_END:
                inGame = false;
                gameOver = true;
                GameEndMsg gameEndMsg = JsonUtility.FromJson<GameEndMsg>(recMsg);
                gameCanvas.gameObject.SetActive(false);
                waitText.gameObject.GetComponent<TextMeshProUGUI>().text = "Game Over";
                int playerListSize = gameEndMsg.playerList.Count;
                offsetLobby = -100;
                for (int i = 0; i < playerListSize; i++)
                {
                    var tmpPointsText = gameOverPoints;
                    waitingPlayer.GetComponentInChildren<TextMeshProUGUI>().text = gameEndMsg.playerList[i].nombre;
                    waitingPlayer.GetComponentInChildren<SpriteRenderer>().sprite = arrayColorJug[gameEndMsg.playerList[i].colorJug];
                    waitingPlayer.transform.position = new Vector3(offsetLobby, 0f);
                    tmpPointsText.transform.position = new Vector3(offsetLobby + 90, -50f);
                    tmpPointsText.GetComponentInChildren<TextMeshProUGUI>().text = gameEndMsg.puntsArray[i]+"";
                    simulatedPlayers.Add(Instantiate(waitingPlayer, waitCanvas.transform));
                    Instantiate(tmpPointsText, waitCanvas.transform);
                    offsetLobby += 100f;
                }
                waitCanvas.gameObject.SetActive(true);
                startButton.gameObject.SetActive(false);
                disconnectButton.gameObject.SetActive(false);
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
        SendToServer(playerInput);
    }
}
