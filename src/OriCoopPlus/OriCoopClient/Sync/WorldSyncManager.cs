using System;
using System.Collections.Generic;
using OriCoop;
using UnityEngine;
using WWClient;
using WWClient.Network;

namespace MP_Client.Sync
{
    public static class WorldSyncManager
    {
        public static bool IsSyncing = false;
        private static List<string> _openedDoors = new List<string>();

        public static void OnLocalLeverPushed(Lever lever, Lever.LeverDirections dir)
        {
            if (IsSyncing || lever == null || lever.MoonGuid == null) return;
            if (MPGameManager.Instance == null || !MPGameManager.Instance.Config.ShareDoorsAndLevers) return;

            Packet packet = new Packet();
            packet.Write((int)PacketType.SYNC_LEVER);
            packet.Write(lever.MoonGuid.A);
            packet.Write(lever.MoonGuid.B);
            packet.Write(lever.MoonGuid.C);
            packet.Write(lever.MoonGuid.D);
            packet.Write((int)dir);

            MPGameManager.Instance.Send(packet);
            WWClient.Logger.Info("WORLD_SYNC", $"Alavanca acionada localmente enviada: {lever.MoonGuid} -> {dir}");
        }

        public static void ReceiveLeverSync(int a, int b, int c, int d, int dir)
        {
            if (Lever.All == null) return;

            IsSyncing = true;
            try
            {
                foreach (Lever lever in Lever.All)
                {
                    if (lever != null && lever.MoonGuid != null &&
                        lever.MoonGuid.A == a && lever.MoonGuid.B == b &&
                        lever.MoonGuid.C == c && lever.MoonGuid.D == d)
                    {
                        lever.SetLeverDirection((Lever.LeverDirections)dir);
                        WWClient.Logger.Info("WORLD_SYNC", $"Alavanca sincronizada de amigo: {lever.MoonGuid} -> {(Lever.LeverDirections)dir}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("WORLD_SYNC", "Erro ao sincronizar alavanca: " + ex.Message);
            }
            finally
            {
                IsSyncing = false;
            }
        }

        public static void OnLocalDoorOpened(DoorWithSlots door)
        {
            if (IsSyncing || door == null || door.MoonGuid == null) return;
            if (MPGameManager.Instance == null || !MPGameManager.Instance.Config.ShareDoorsAndLevers) return;

            string guidStr = door.MoonGuid.ToString();
            if (_openedDoors.Contains(guidStr)) return;
            _openedDoors.Add(guidStr);

            Packet packet = new Packet();
            packet.Write((int)PacketType.SYNC_DOOR);
            packet.Write(door.MoonGuid.A);
            packet.Write(door.MoonGuid.B);
            packet.Write(door.MoonGuid.C);
            packet.Write(door.MoonGuid.D);

            MPGameManager.Instance.Send(packet);
            WWClient.Logger.Info("WORLD_SYNC", $"Porta keystone aberta localmente enviada: {guidStr}");
        }

        public static void ReceiveDoorSync(int a, int b, int c, int d)
        {
            IsSyncing = true;
            try
            {
                DoorWithSlots[] doors = UnityEngine.Object.FindObjectsOfType<DoorWithSlots>();
                foreach (DoorWithSlots door in doors)
                {
                    if (door != null && door.MoonGuid != null &&
                        door.MoonGuid.A == a && door.MoonGuid.B == b &&
                        door.MoonGuid.C == c && door.MoonGuid.D == d)
                    {
                        if (door.CurrentState != DoorWithSlots.State.Opened)
                        {
                            door.CurrentState = DoorWithSlots.State.Opened;
                            if (door.OnOpenedAction != null)
                            {
                                door.OnOpenedAction.Perform(null);
                            }
                            WWClient.Logger.Info("WORLD_SYNC", $"Porta keystone sincronizada e aberta: {door.MoonGuid}");
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("WORLD_SYNC", "Erro ao sincronizar porta: " + ex.Message);
            }
            finally
            {
                IsSyncing = false;
            }
        }

        public static void OnLocalWorldEventChanged(MoonGuid guid, int state)
        {
            if (IsSyncing || guid == null) return;
            if (MPGameManager.Instance == null || !MPGameManager.Instance.Config.ShareWorldEvents) return;

            Packet packet = new Packet();
            packet.Write((int)PacketType.SYNC_WORLDEVENT);
            packet.Write(guid.A);
            packet.Write(guid.B);
            packet.Write(guid.C);
            packet.Write(guid.D);
            packet.Write(state);

            MPGameManager.Instance.Send(packet);
            WWClient.Logger.Info("WORLD_SYNC", $"Evento de mundo local enviado: {guid} -> {state}");
        }

        public static void ReceiveWorldEventSync(int a, int b, int c, int d, int state)
        {
            IsSyncing = true;
            try
            {
                if (WorldEventsManager.Instance != null)
                {
                    MoonGuid targetGuid = new MoonGuid(a, b, c, d);
                    var field = typeof(WorldEventsManager).GetField("m_worldEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var dict = field.GetValue(WorldEventsManager.Instance) as Dictionary<MoonGuid, WorldEventsRuntime>;
                        if (dict != null)
                        {
                            foreach (var kvp in dict)
                            {
                                if (kvp.Key != null && kvp.Key.A == a && kvp.Key.B == b && kvp.Key.C == c && kvp.Key.D == d)
                                {
                                    kvp.Value.Value = state;
                                    WWClient.Logger.Info("WORLD_SYNC", $"Evento de mundo sincronizado: {targetGuid} -> {state}");
                                    return;
                                }
                            }
                            dict[targetGuid] = new WorldEventsRuntime(state);
                            WWClient.Logger.Info("WORLD_SYNC", $"Novo evento de mundo registrado: {targetGuid} -> {state}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("WORLD_SYNC", "Erro ao sincronizar evento de mundo: " + ex.Message);
            }
            finally
            {
                IsSyncing = false;
            }
        }
    }
}
