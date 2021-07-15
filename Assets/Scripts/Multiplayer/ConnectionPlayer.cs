using Game;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Multiplayer
{

    public class ConnectionPlayer : NetworkBehaviour
    {
        public static bool inGame = false;
        public static bool isHost;
        public Guid matchId;
        float intervalSend = 0.03f;
        float lastInterval;
        public Player player;
        public static Dictionary<uint, Player> playersClients = new Dictionary<uint, Player>();
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

            ///UPDATE MULTIPLAYER
            if (lastInterval > Time.time) return;
            lastInterval = Time.time + intervalSend;
            
            if (isHost)
            {
                List<PlayerCmdPositions> playersUpdatePosition = new List<PlayerCmdPositions>();
                foreach (KeyValuePair<uint, Player> player in playersClients)
                {
                    playersUpdatePosition.Add(new PlayerCmdPositions { id=player.Key, position=player.Value.transform.position });
                }

                CmdHostSendAllPositions(playersUpdatePosition);
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
        void CmdHostSendAllPositions(List<PlayerCmdPositions> playersPosition)
        {            
            RpcUpdateAllPosition(playersPosition);
        }
        
        [ClientRpc]
        void RpcUpdateAllPosition(List<PlayerCmdPositions> playersPosition)
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
            if (!isHost) Debug.LogError("Isso só deveria chegar no host...");

            Player playerClient;
            if (playersClients.TryGetValue(playerCmd.id, out playerClient))
            {
                playerClient.input = playerCmd.input;
            }
        }

        internal static void MakeHost()
        {
            isHost = true;
        }

        void OnChangeVarPlayerColor(Color old, Color color)
        {
            ChangePlayerColor(color);
        }

        private void ChangePlayerColor(Color color)
        {
            if (player) player.gameObject.GetComponentInChildren<Renderer>().material.color = color;
        }
    }        
}

public struct PlayerCmdPositions
{
    public uint id;
    public Vector3 position;
}

public struct PlayerCmdInput
{
    public uint id;
    public PlayerInput input;
}