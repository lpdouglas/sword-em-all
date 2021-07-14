using Mirror;
using System.Collections.Generic;

namespace Game.Online
{
    public class MatchInterestManager : InterestManagement
    {
        public float rebuildInterval = 10;
        double lastRebuildTime;

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
        {
            return identity.GetComponent<LobbyPlayer>().matchId.ToString() == newObserver.identity.GetComponent<LobbyPlayer>().matchId.ToString();
        }

        public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
        {            
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
                if (conn != null && conn.isAuthenticated && conn.identity != null)
                    if (conn.identity.GetComponent<LobbyPlayer>().matchId.ToString() == identity.GetComponent<LobbyPlayer>().matchId.ToString())
                        newObservers.Add(conn);
        }

        void Update()
        {
            if (!NetworkServer.active) return;

            if (NetworkTime.time >= lastRebuildTime + rebuildInterval)
            {
                RebuildAll();
                lastRebuildTime = NetworkTime.time;
            }
        }
    }
}