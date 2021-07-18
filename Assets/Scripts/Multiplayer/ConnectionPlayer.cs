using TGM;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TGM.Multiplayer
{
    public class ConnectionPlayer : NetworkBehaviour
    {
        public static bool inGame = false;
        public static bool isHost;
        public bool isHostOnServer;
        public Guid matchId;
        float intervalSend = 0.03f;
        float lastInterval;
        static int spawnBalls = 0;
        public Player player;
        public NetObject ball;
        static Dictionary<uint, Player> playersClients = new Dictionary<uint, Player>();
        static Dictionary<uint, NetObject> netObjects = new Dictionary<uint, NetObject>();
        PlayerInput lastInputSended;
        [SyncVar(hook = nameof(OnChangeVarPlayerColor))] public Color playerColor;
        

        private void Start()
        {
            if (isClient)
            player = Instantiate(LobbyNetworkManager.instance.gamePlayer, transform.position, transform.rotation).GetComponent<Player>();
            playersClients.Add(netId, player);            
            ChangePlayerColor(playerColor);
            if (isLocalPlayer && LobbyNetworkManager.instance.cameraPosition != null) LobbyNetworkManager.instance.cameraPosition.transform.rotation = transform.rotation;
        }

        private void OnDestroy()
        {
            playersClients.Remove(netId);
            if (player!=null) Destroy(player.gameObject);
        }

        private void Update()
        {
            if (!inGame) return;
            if (!isLocalPlayer) return;
            
            player.input.x = Input.GetAxis("Horizontal");
            if (Input.GetButtonDown("Submit")) {
                if (isServer) NetworkManager.singleton.StopHost();
                else NetworkManager.singleton.StopClient();
            };
            if (isHost)
            {
                if (Input.GetButtonDown("Fire1")) spawnBalls++;

                if (spawnBalls > 0)
                {
                    spawnBalls--;
                    NetObject netBall = Instantiate(ball, new Vector3(UnityEngine.Random.Range(-2,2),0.5f, UnityEngine.Random.Range(-2, 2)), Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 360), 0)));
                    //Debug.Log("Pos instantiate log id: " + netBall.id);
                    Spawn(netBall);
                    netObjects.Add(netBall.id, netBall);
                }
            }

            ///UPDATE MULTIPLAYER
            if (lastInterval > Time.time) return;
            lastInterval = Time.time + intervalSend;
            
            if (isHost)
            {
                List<PlayerCmdPositions> playersPositionList = new List<PlayerCmdPositions>();
                foreach (KeyValuePair<uint, Player> player in playersClients)
                {
                    playersPositionList.Add(new PlayerCmdPositions { id=player.Key, position=player.Value.transform.position });
                }
                
                List<NetObjectCmdPositions> netObjectsList = new List<NetObjectCmdPositions>();
                foreach (KeyValuePair<uint, NetObject> netObject in netObjects)
                {
                    netObjectsList.Add(new NetObjectCmdPositions { id=netObject.Key, position=netObject.Value.transform.position});
                }
                
                CmdHostSendAllPositions(playersPositionList, netObjectsList);
            } else
            {
                if (!lastInputSended.Equals(player.input))
                {
                    CmdClientSendComands(player.input);
                    lastInputSended = player.input;
                }
            }
            

        }

        [Command]
        void CmdHostSendAllPositions(List<PlayerCmdPositions> playersPosition, List<NetObjectCmdPositions> netObjectCmdPositions)
        {
            if (!isHostOnServer) { Debug.LogError("Apenas o host pode spawnar"); return; }
            RpcUpdateAllPosition(playersPosition, netObjectCmdPositions);
        }
        
        [ClientRpc]
        void RpcUpdateAllPosition(List<PlayerCmdPositions> playersPosition, List<NetObjectCmdPositions> netObjectsCmdPositions)
        {
            if (isHost) return;           

            foreach (PlayerCmdPositions playerPositions in playersPosition)
            {
                Player player1;
                if (playersClients.TryGetValue(playerPositions.id, out player1))
                {
                    player1.transform.position = playerPositions.position;
                }
            }
            
            foreach (NetObjectCmdPositions netObjectCmd in netObjectsCmdPositions)
            {                
                NetObject netObject1;
                if (netObjects.TryGetValue(netObjectCmd.id, out netObject1))
                {
                    netObject1.transform.position = netObjectCmd.position;
                }
            }
        }


        [Command]
        void CmdClientSendComands(PlayerInput input)
        {
            Client client;
            if (!LobbyNetworkManager.instance.clients.TryGetValue(connectionToClient, out client)) return;
            Match match;
            if (!LobbyNetworkManager.instance.matches.TryGetValue(client.matchId, out match)) return;
            TargetHostGetInputs(match.host, new PlayerCmdInput { input=input, id=netId } );
        }

        [TargetRpc]
        void TargetHostGetInputs(NetworkConnection target, PlayerCmdInput playerCmd) {
            if (!isHost) { Debug.LogError("Isso só deveria chegar no host..."); return; }

            Player playerClient;
            if (playersClients.TryGetValue(playerCmd.id, out playerClient))
            {
                playerClient.input = playerCmd.input;
            }
        }

        void Spawn(NetObject netObject)
        {
            CmdSpawn(netObject.guidAssetId, netObject.id, netObject.transform.position, netObject.transform.rotation, netObject.transform.localScale);
        }

        [Command]
        void CmdSpawn(Guid assetId, uint netId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!isHostOnServer) { Debug.LogError("Apenas o host pode spawnar"); return; }
            RpcSpawn(assetId, netId, position, rotation, scale);
        }
        
        [ClientRpc]
        void RpcSpawn(Guid assetId, uint netId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (isHost) return;

            NetObject[] netObjectsAll = LobbyNetworkManager.instance.netObjects;
            foreach(NetObject netObject in netObjectsAll)
            {
                if (netObject.guidAssetId.Equals(assetId))
                {
                    NetObject netObjectOnGame = Instantiate(netObject, position, rotation);
                    netObjectOnGame.transform.localScale = scale;
                    netObjectOnGame.id = netId;
                    netObjects.Add(netId, netObjectOnGame);
                    break;
                }
            }
            
        }

        internal static void MakeHost(bool host)
        {
            isHost = host;            
        }
        void OnChangeVarPlayerColor(Color old, Color color) => ChangePlayerColor(color);
        private void ChangePlayerColor(Color color)
        {
            if (player) player.gameObject.GetComponentInChildren<Renderer>().material.color = color;
        }

        internal static void StartGame()
        {
            inGame = true;
            if (isHost)
            {
                spawnBalls = 2;
            }
        }

        public static void OnDisconnect()
        {
            Debug.Log("Disconnected");
            foreach (KeyValuePair<uint, NetObject> netObject in netObjects)
            {
                Destroy(netObject.Value.gameObject);
            }
            netObjects.Clear();
            playersClients.Clear();
            inGame = false;
        }
    }        
}

public struct PlayerCmdPositions
{
    public uint id;
    public Vector3 position;
}

public struct NetObjectCmdPositions
{
    public uint id;
    public Vector3 position;
    public Vector3 velocity;
}

public struct PlayerCmdInput
{
    public uint id;
    public PlayerInput input;
}