using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

namespace Game.Multiplayer
{
    public class LobbyNetworkManager : NetworkManager
    {
        public static LobbyNetworkManager instance;
        [SerializeField] Transform[] Spawns;
        public GameObject gamePlayer;
        public GameObject cameraPosition;
        Match currentMatch;
        public Dictionary<Guid, Match> matches { get; private set; } = new Dictionary<Guid, Match>();
        public Dictionary<NetworkConnection, Client> clients { get; private set; } = new Dictionary<NetworkConnection, Client>();

        #region Unity Callbacks

        public override void OnValidate()
        {
            base.OnValidate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            instance = this;
            base.Awake();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Start()
        {
            base.Start();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Start & Stop

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public override void ConfigureServerFrameRate()
        {
            base.ConfigureServerFrameRate();
        }

        /// <summary>
        /// called when quitting the application by closing the window / pressing stop in the editor
        /// </summary>
        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        public override void OnServerChangeScene(string newSceneName) { }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName) { }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        /// <param name="conn">The network connection that the scene change message arrived on.</param>
        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerConnect(NetworkConnection conn) {                        
            if (currentMatch.players.Count>=currentMatch.maxPlayers) { CreateNewMatch(); }

            bool host = false;
            if (currentMatch.host==null) {

                Match matchAddHost = matches[currentMatch.id];
                matchAddHost.host = conn;
                matches[currentMatch.id] = matchAddHost;
                currentMatch = matchAddHost;
                Debug.Log($"HOST: {matches[currentMatch.id].host} id {conn}");
                host = true;
                Debug.Log("Add on host list");
            }
            clients.Add(conn, new Client { matchId=currentMatch.id, isHost=host });
            currentMatch.players.Add(conn);

            Debug.Log("Conectei ao server " + conn);
        }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnection conn)
        {
            int playersInCurrentMatch = currentMatch.players.Count;
            GameObject player = Instantiate(playerPrefab, Spawns[playersInCurrentMatch-1].position, Spawns[playersInCurrentMatch-1].rotation);
            player.GetComponent<ConnectionPlayer>().matchId = currentMatch.id;
            player.GetComponent<ConnectionPlayer>().playerColor  = (new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.cyan })[playersInCurrentMatch-1];


            //if (!NetworkClient.ready) NetworkClient.Ready();
            
            NetworkServer.AddPlayerForConnection(conn, player);

            if (clients[conn].isHost)
            {
                conn.Send(new ToClientMessage { message = ClientMessageType.Host });
            }
            base.OnServerReady(conn);
        }

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnection conn)
        {            
            base.OnServerAddPlayer(conn);
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Client client;
            if (clients.TryGetValue(conn, out client) && matches.ContainsKey(client.matchId))
            {
                Match match = matches[client.matchId];
                matches[match.id].players.Remove(conn);

                if (client.isHost && match.players.Count>0)
                {
                    //TODO player � host de uma partida. deve ent�o finaliz�-la ou trocar o host
                    //match.players.GETSOMEOTHERPLAYER.Send(new ClientMessage { message = ClientMessageType.Host });
                }

                clients.Remove(conn);
            }
            
            base.OnServerDisconnect(conn);
        }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        public override void OnClientConnect(NetworkConnection conn)
        {
            ConnectionPlayer.inGame = true;
            Debug.Log("Conectei ao server");
            
            base.OnClientConnect(conn);
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        public override void OnClientDisconnect(NetworkConnection conn)
        {
            ConnectionPlayer.inGame = false;
            Debug.Log("!");
            Destroy(conn.identity.GetComponent<ConnectionPlayer>().player);
            base.OnClientDisconnect(conn);
        }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        /// <param name="conn">Connection to the server.</param>
        public override void OnClientNotReady(NetworkConnection conn) { }

        #endregion

        #region Start & Stop Callbacks

        // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
        // their functionality, users would need override all the versions. Instead these callbacks are invoked
        // from all versions, so users only need to implement this one case.

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartHost() { }

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer() {
            CreateNewMatch();
        }

        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        public override void OnStartClient() {
            Debug.Log("START CLIENT register");
            NetworkClient.RegisterHandler<ToClientMessage>(OnClientMessage);
        }

        void OnClientMessage(ToClientMessage clientMessage)
        {
            switch (clientMessage.message)
            {
                case ClientMessageType.Host: ConnectionPlayer.MakeHost(); Debug.Log("Entendi a responsa, capit�o"); break;
                default : Debug.LogError("Missing message type");break; 
            }
        }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public override void OnStopHost() { }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer() {
            RemoveAllMatches();
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient() {
            foreach(Player objs in FindObjectsOfType<Player>())
            {
                Destroy(objs.gameObject);
            }
        }

        #endregion

        void CreateNewMatch()
        {
            currentMatch = new Match { id = Guid.NewGuid(), players = new HashSet<NetworkConnection>(), status = MatchStatus.Waiting, maxPlayers=3 };
            matches.Add(currentMatch.id, currentMatch);
        }

        void RemoveAllMatches()
        {
            matches.Clear();
            currentMatch = Match.empty;
        }
        
    }

    public struct Match
    {
        public Guid id;
        public HashSet<NetworkConnection> players;
        public NetworkConnection host;
        public int maxPlayers;
        public MatchStatus status;
        public static Match empty { get => new Match { id = Guid.Empty }; }
        public Match setHost(NetworkConnection conn) { host = conn; return this; }
    }

    public struct Client
    {
        public Guid matchId;
        public bool isHost;
    }

    public enum MatchStatus
    {
        Waiting, OnGame
    }

    public struct ToClientMessage : NetworkMessage
    {
        public ClientMessageType message;
    }

    public enum ClientMessageType
    {
        None,
        Host,
        ClientsCommands
    }
    
    public enum ServerMessageType
    {
        None,
        Host,
        ClientsUpdate
    }
}