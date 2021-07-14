using Mirror;
using System;
using UnityEngine;

namespace Game.Online
{

    public class LobbyPlayer : NetworkBehaviour
    {
        public static bool inGame = false;
        public static bool isHost;

        public Guid matchId;
        public float positionHorizontal;        

        float intervalSend = 1f;
        float lastInterval;
        Player player;

        private void Awake() => player = GetComponent<Player>();
        private void Update()
        {
            if (!isHost) return;
            if (!inGame || lastInterval > Time.time) return;
            //if (!isLocalPlayer) return;
            lastInterval = Time.time + intervalSend;
            
            //positionHorizontal = player.
            CmdUpdatePosition(positionHorizontal);
        }

        [Command]
        void CmdUpdatePosition(float horizontalPosition)
        {
            positionHorizontal = horizontalPosition;
            RpcUpdatePosition(horizontalPosition);
        }
        
        [ClientRpc]
        void RpcUpdatePosition(float horizontalPosition)
        {
            if (isHost) return;
            positionHorizontal = horizontalPosition;
        }
    }    
}