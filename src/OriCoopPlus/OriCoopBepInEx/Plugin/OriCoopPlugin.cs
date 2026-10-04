using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using OriCoopBepInEx.Domain;
using OriCoopBepInEx.Networking;

namespace OriCoopBepInEx.Plugin
{
    [BepInPlugin("com.ikkikuuro.oricoop", "Ori Coop", "0.1.0")]
    public sealed class OriCoopPlugin : BaseUnityPlugin, IPlayerStateSink
    {
        public static OriCoopPlugin Instance { get; private set; }

        private INetworkService _network;
        private Harmony _harmony;
        private ConfigEntry<string> _serverHost;
        private ConfigEntry<int> _serverPort;
        private ConfigEntry<int> _playerId;
        private readonly Dictionary<int, PlayerSnapshot> _remotePlayers = new Dictionary<int, PlayerSnapshot>();
        private readonly Queue<Action> _mainThreadActions = new Queue<Action>();
        private Vector3Data _localPosition;

        private void Awake()
        {
            Instance = this;
            _serverHost = Config.Bind("Network", "Host", "127.0.0.1", "UDP server host.");
            _serverPort = Config.Bind("Network", "Port", 7777, "UDP server port.");
            _playerId = Config.Bind("Network", "PlayerId", -1, "Local player identifier; keep -1 for server assignment.");

            _network = new NetworkService(_serverHost.Value, _serverPort.Value, _playerId.Value);
            _network.PlayerSnapshotReceived += OnPlayerSnapshotReceived;
            _network.TeleportRequested += OnTeleportRequested;
            _network.ChatMessageReceived += OnChatMessageReceived;
            _network.EntitySyncChanged += OnEntitySyncChanged;
            _network.Start();

            _harmony = new Harmony("com.ikkikuuro.oricoop");
            _harmony.PatchAll();
            Logger.LogInfo("Ori Coop BepInEx plugin loaded.");
        }

        public void Publish(PlayerSnapshot snapshot)
        {
            if (snapshot != null)
            {
                snapshot.PlayerId = _playerId.Value;
                _localPosition = snapshot.Position;
                _network.SendPlayerSnapshot(snapshot);
            }
        }

        private void OnPlayerSnapshotReceived(PlayerSnapshot snapshot)
        {
            lock (_remotePlayers)
            {
                _remotePlayers[snapshot.PlayerId] = snapshot;
            }
        }

        private void OnTeleportRequested(Vector3Data position, string destination)
        {
            lock (_mainThreadActions)
            {
                _mainThreadActions.Enqueue(delegate
                {
                    UnityEngine.GameObject sein = UnityEngine.GameObject.Find("Characters/Sein");
                    if (sein == null)
                    {
                        sein = UnityEngine.GameObject.Find("Sein");
                    }
                    if (sein == null)
                    {
                        Logger.LogWarning("Teleport received, but the local Sein object was not found.");
                        return;
                    }

                    sein.transform.position = new UnityEngine.Vector3(position.X, position.Y, position.Z);
                    Logger.LogMessage("<color=cyan>SERVER</color>: Teleported to " + destination + ".");
                });
            }
        }

        private void OnChatMessageReceived(string sender, string message)
        {
            Logger.LogMessage(sender + ": " + message);
        }

        private void OnEntitySyncChanged(bool enabled)
        {
            Logger.LogInfo("Entity synchronization: " + (enabled ? "enabled" : "disabled") + ".");
        }

        private void Update()
        {
            lock (_mainThreadActions)
            {
                while (_mainThreadActions.Count > 0)
                {
                    _mainThreadActions.Dequeue()();
                }
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.T))
            {
                int targetId = FindNearestRemotePlayer();
                if (targetId >= 0)
                {
                    _network.SendTeleportRequest(targetId);
                }
                else
                {
                    Logger.LogWarning("No remote player is available for teleport.");
                }
            }
        }

        private int FindNearestRemotePlayer()
        {
            lock (_remotePlayers)
            {
                float bestDistance = float.MaxValue;
                int bestId = -1;
                foreach (KeyValuePair<int, PlayerSnapshot> entry in _remotePlayers)
                {
                    float x = entry.Value.Position.X - _localPosition.X;
                    float y = entry.Value.Position.Y - _localPosition.Y;
                    float z = entry.Value.Position.Z - _localPosition.Z;
                    float distance = x * x + y * y + z * z;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestId = entry.Key;
                    }
                }
                return bestId;
            }
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
            }
            if (_network != null)
            {
                _network.PlayerSnapshotReceived -= OnPlayerSnapshotReceived;
                _network.TeleportRequested -= OnTeleportRequested;
                _network.ChatMessageReceived -= OnChatMessageReceived;
                _network.EntitySyncChanged -= OnEntitySyncChanged;
                _network.Dispose();
            }
            Instance = null;
        }
    }
}
