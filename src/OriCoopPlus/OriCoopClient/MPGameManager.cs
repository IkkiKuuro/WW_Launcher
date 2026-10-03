using System;
using System.Collections.Generic;
using Core;
using Game;
using MP_Client.Data;
using MP_Client.Patches;
using MP_Client.Sync;
using MP_Client.UI;
using OriCoop;
using UnityEngine;
using WWClient;
using WWClient.API;
using WWClient.Network;

namespace MP_Client
{
    public class MPGameManager : MonoBehaviour
    {
        public static MPGameManager Instance;
        public CoopConfig Config = new CoopConfig();

        public Dictionary<int, OriMPPlayer> Players = new Dictionary<int, OriMPPlayer>();
        public Dictionary<int, Color> Colors = new Dictionary<int, Color>();
        public Dictionary<string, TextureAnimationWithTransitions> Animations = new Dictionary<string, TextureAnimationWithTransitions>();

        public OriMPPlayer Ori;
        public GameObject PlayerPrefab;
        public Color LocalColor = Color.white;
        public bool OriLoaded = false;
        private string LastAnim = "";
        private int _lastTeleportIndex = 0;

        public int LocalPlayerId => Client.Instance != null ? Client.Instance.myId : -1;
        public bool IsSyncScene => GameController.Instance != null && !GameController.Instance.IsLoadingGame;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            base.gameObject.name = "MPGameManager";
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            if (GetComponent<CoopHUD>() == null)
            {
                base.gameObject.AddComponent<CoopHUD>();
            }
            WWClient.Logger.Info("MPGameManager", "Ori Coop Plus Manager INICIADO!");

            if (ORIDEClientModule.GameData != null && ORIDEClientModule.GameData.AutoConnect)
            {
                InvokeRepeating("TryAutoConnect", 1.2f, 1f);
            }
        }

        public void TryAutoConnect()
        {
            if (Client.Instance == null)
            {
                WWClient.Logger.Info("CLIENT", "A API de rede ainda não inicializou; nova tentativa em 1 segundo.");
                return;
            }

            if (Client.isConnected || Client.isConnecting)
            {
                CancelInvoke("TryAutoConnect");
                return;
            }

            if (!Client.isConnected && !Client.isConnecting)
            {
                ORIDEClientModule.ApplySettingsToClient();
                WWClient.Logger.Info("CLIENT", $"Tentando auto-conexão ao servidor {Client.Instance.ip}:{Client.Instance.port}...");
                try
                {
                    Client.Instance.ConnectToServer();
                }
                catch (Exception ex)
                {
                    WWClient.Logger.Error("CLIENT", "Falha ao iniciar conexão: " + ex);
                }
            }
        }

        public static void OnConnected()
        {
            if (Instance != null)
            {
                WWClient.Logger.Info("MPGameManager", "Conectado ao servidor!");
                if (Client.Instance != null)
                {
                    Instance.ShowNotification($"Conectado ao Servidor Coop ({Client.Instance.ip}:{Client.Instance.port})!", 4f);
                }
                Instance.Invoke("SendColor", 1f);
            }
        }

        public void SendColor()
        {
            WWClient.Logger.Info("MPGameManager", "Enviando cor do jogador...");
            if (Client.isConnected &&
                Client.NetworkVars.TryGetValue("cc", out var value) &&
                value.Bool)
            {
                Packet packet = new Packet((int)PacketType.COLOR);
                if (ORIDEClientModule.GameData != null)
                {
                    Color c = ORIDEClientModule.GameData.LocalColor;
                    packet.Write((byte)(c.r * 255));
                    packet.Write((byte)(c.g * 255));
                    packet.Write((byte)(c.b * 255));
                    ClientSend.SendData(packet);
                    ApplyLocalColor(c);
                }
            }
        }

        public void ApplyLocalColor(Color color)
        {
            LocalColor = color;
            if (Ori != null)
            {
                Ori.SetupOriColor(color);
            }
        }

        public void AddAnimation(TextureAnimationWithTransitions anim)
        {
            if (anim != null && !Animations.ContainsKey(anim.name))
            {
                Animations.Add(anim.name, anim);
            }
        }

        public bool TryGetAnimation(string name, out TextureAnimationWithTransitions anim)
        {
            if (Animations.TryGetValue(name, out anim))
            {
                return true;
            }
            foreach (var kvp in Animations)
            {
                if (name.Contains(kvp.Key))
                {
                    anim = kvp.Value;
                    return true;
                }
            }
            anim = null;
            return false;
        }

        public void ShowNotification(string message, float duration = 3f)
        {
            CoopHUD.AddToast(message, duration);
        }

        public void SendDummyAction(int action, int extra = 0)
        {
            if (Client.isConnected)
            {
                Packet packet = new Packet();
                packet.Write((int)PacketType.DUMMY_ACTION);
                packet.Write(action);
                packet.Write(extra);
                Send(packet);
            }
        }

        private void Update()
        {
            // Teleport shortcut key (T)
            if (UnityEngine.Input.GetKeyDown(KeyCode.T))
            {
                if (Config.AllowTeleport)
                {
                    TeleportToNextFriend();
                }
                else
                {
                    ShowNotification("O teleporte está desativado pelo servidor.", 3f);
                }
            }

            // Clean players key (P)
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                DestroyAllPlayers();
            }
        }

        private void FixedUpdate()
        {
            if (Ori == null)
            {
                if (Characters.Ori == null || Characters.Sein == null) return;

                Ori = Characters.Sein.gameObject.AddComponent<OriMPPlayer>();

                if (PlayerPrefab != null)
                {
                    UnityEngine.Object.DestroyImmediate(PlayerPrefab);
                    PlayerPrefab = null;
                }

                try
                {
                    GameObject[] array = Resources.LoadAll<GameObject>("");
                    foreach (GameObject gameObject in array)
                    {
                        if (!gameObject.name.Contains("seinCharacter")) continue;

                        gameObject.SetActive(false);
                        GameObject clone = UnityEngine.Object.Instantiate(gameObject);
                        CleanPrefabComponents(clone);

                        PlayerPrefab = clone;
                        gameObject.SetActive(true);
                        break;
                    }
                }
                catch (Exception) { }

                // Fallback: Directly clone Characters.Sein if Resources.LoadAll didn't find it!
                if (PlayerPrefab == null && Characters.Sein != null)
                {
                    try
                    {
                        GameObject clone = UnityEngine.Object.Instantiate(Characters.Sein.gameObject);
                        CleanPrefabComponents(clone);
                        clone.SetActive(false);
                        PlayerPrefab = clone;
                        WWClient.Logger.Info("CLIENT", "PlayerPrefab criado com sucesso clonando Characters.Sein diretamente!");
                    }
                    catch (Exception ex)
                    {
                        WWClient.Logger.Error("CLIENT", "Erro ao clonar Characters.Sein: " + ex);
                    }
                }

                if (PlayerPrefab != null)
                {
                    UnityEngine.Object.DontDestroyOnLoad(PlayerPrefab);
                }

                Ori.InitLocalPlayer();
                Ori.SetupOriColor(LocalColor);
                OriLoaded = true;
                ShowNotification("Ori Co-op: Personagem pronto para multijogador!", 3.5f);
            }
            else if (Client.isConnected && IsSyncScene && LocalPlayerId > -1)
            {
                // Send animation update
                if (Ori.Animator != null && Ori.Animator.Animator != null && Ori.Animator.Animator.CurrentAnimation != null)
                {
                    string curAnim = Ori.Animator.Animator.CurrentAnimation.name;
                    if (LastAnim != curAnim)
                    {
                        LastAnim = curAnim;
                        Packet animPacket = new Packet();
                        animPacket.Write((int)PacketType.ANIM);
                        animPacket.Write(curAnim);
                        Send(animPacket);
                    }
                }

                // Send position update
                bool faceLeft = Ori.CharacterSpriteMirror != null && Ori.CharacterSpriteMirror.FaceLeft;
                Packet posPacket = new Packet();
                posPacket.Write((int)PacketType.POSITION);
                posPacket.Write(Ori.gameObject.transform.position);
                posPacket.Write(faceLeft);
                Send(posPacket);
            }
        }

        private void CleanPrefabComponents(GameObject go)
        {
            foreach (Component comp in go.GetComponents(typeof(Component)))
            {
                string typeName = comp.GetType().FullName;
                if (!typeName.Contains("UnityEngine") && !typeName.Contains("Sprite") &&
                    !typeName.Contains("CharacterAnimationSystem") && !typeName.Contains("CharacterSpriteMirror"))
                {
                    UnityEngine.Object.DestroyImmediate(comp);
                }
                else if (typeName.Contains("Rigidbody") || typeName.Contains("ParticleSystem"))
                {
                    UnityEngine.Object.DestroyImmediate(comp);
                }
            }

            foreach (Component comp2 in go.GetComponentsInChildren(typeof(Component)))
            {
                string typeName = comp2.GetType().FullName;
                if (!typeName.Contains("UnityEngine") && !typeName.Contains("Sprite") &&
                    !typeName.Contains("CharacterAnimationSystem") && !typeName.Contains("CharacterSpriteMirror"))
                {
                    UnityEngine.Object.DestroyImmediate(comp2);
                }
                else if (typeName.Contains("Rigidbody") || typeName.Contains("ParticleSystem"))
                {
                    UnityEngine.Object.DestroyImmediate(comp2);
                }
            }
        }

        public bool HasPlayer(int id) => Players.ContainsKey(id);

        public void SpawnPlayer(int id, string username, Vector3 position)
        {
            if (Players.ContainsKey(id))
            {
                if (Players[id] != null)
                {
                    UnityEngine.Object.DestroyImmediate(Players[id].gameObject);
                }
                Players.Remove(id);
            }

            if (PlayerPrefab == null && Characters.Sein != null)
            {
                try
                {
                    GameObject clone = UnityEngine.Object.Instantiate(Characters.Sein.gameObject);
                    CleanPrefabComponents(clone);
                    clone.SetActive(false);
                    PlayerPrefab = clone;
                    UnityEngine.Object.DontDestroyOnLoad(PlayerPrefab);
                    WWClient.Logger.Info("CLIENT", "PlayerPrefab criado sob demanda no SpawnPlayer!");
                }
                catch (Exception) { }
            }

            if (PlayerPrefab == null)
            {
                WWClient.Logger.Debug("CLIENT", "PlayerPrefab is null! Cannot spawn player.");
                return;
            }

            GameObject spawned = UnityEngine.Object.Instantiate(PlayerPrefab);
            spawned.SetActive(true);
            OriMPPlayer oriPlayer = spawned.AddComponent<OriMPPlayer>();
            oriPlayer.InitOtherPlayer(id, username);

            if (Colors.ContainsKey(id))
            {
                oriPlayer.SetupOriColor(Colors[id]);
            }

            spawned.transform.position = position;
            Players.Add(id, oriPlayer);

            WWClient.Logger.Info("CLIENT", $"[+] Jogador conectado spawnado: {username} (ID {id})");
            ShowNotification($"[+] {oriPlayer.NickName} entrou no jogo!", 3.5f);
        }

        public void SendSkillUsed(CoopSkillType type)
        {
            if (IsSyncScene && Client.isConnected)
            {
                Packet packet = new Packet();
                packet.Write((int)PacketType.SKILL);
                packet.Write((int)type);
                Send(packet);
            }
        }

        public void SendAbilityUnlocked(AbilityType ability)
        {
            if (IsSyncScene && Client.isConnected)
            {
                Packet packet = new Packet();
                packet.Write((int)PacketType.SYNC_ABILITY);
                packet.Write((int)ability);
                Send(packet);
            }
        }

        public void ReceiveAbilitySync(AbilityType ability)
        {
            if (Characters.Sein == null || Characters.Sein.PlayerAbilities == null) return;

            if (!Characters.Sein.PlayerAbilities.HasAbility(ability))
            {
                AbilityPatches.IsSyncing = true;
                try
                {
                    Characters.Sein.PlayerAbilities.SetAbility(ability, true);
                    ShowNotification($"Habilidade recebida em Co-op: {ability}!", 4f);
                    WWClient.Logger.Info("COOP", $"[+] Habilidade desbloqueada via co-op: {ability}");
                }
                catch (Exception ex)
                {
                    WWClient.Logger.Error("COOP", "Erro ao aplicar habilidade sincronizada: " + ex);
                }
                finally
                {
                    AbilityPatches.IsSyncing = false;
                }
            }
        }

        public void TeleportToNextFriend()
        {
            if (Players == null || Players.Count == 0)
            {
                ShowNotification("Nenhum amigo conectado para teleporte!", 3f);
                return;
            }

            List<OriMPPlayer> validPlayers = new List<OriMPPlayer>();
            foreach (var kvp in Players)
            {
                if (kvp.Value != null && kvp.Value.gameObject.activeInHierarchy)
                {
                    validPlayers.Add(kvp.Value);
                }
            }

            if (validPlayers.Count == 0)
            {
                ShowNotification("Nenhum amigo ativo no momento!", 3f);
                return;
            }

            _lastTeleportIndex = (_lastTeleportIndex + 1) % validPlayers.Count;
            OriMPPlayer target = validPlayers[_lastTeleportIndex];

            TeleportLocalSein(target.transform.position, target.NickName);
        }

        public void TeleportToPlayer(int playerId)
        {
            if (!Config.AllowTeleport)
            {
                ShowNotification("O teleporte está desativado pelo servidor.", 3f);
                return;
            }

            if (!Players.TryGetValue(playerId, out OriMPPlayer target) ||
                target == null ||
                !target.gameObject.activeInHierarchy)
            {
                ShowNotification("Esse jogador não está disponível para teleporte.", 3f);
                return;
            }

            TeleportLocalSein(target.transform.position, target.NickName);
        }

        public void TeleportLocalSein(Vector3 targetPosition, string friendName)
        {
            if (Characters.Sein == null) return;

            try
            {
                // 1. Preload scenes at destination so Ori doesn't fall through unloaded world
                if (Scenes.Manager != null)
                {
                    Scenes.Manager.AdditivelyLoadScenesAtPosition(targetPosition, async: false, loadingZones: false, keepPreloaded: true);
                }

                // 2. Set Sein position with safe offset
                Characters.Sein.Position = targetPosition + Vector3.up * 0.6f;

                // 3. Update Camera and Scenes instantly
                CameraPivotZone.InstantUpdate();
                if (Scenes.Manager != null)
                {
                    Scenes.Manager.UpdatePosition();
                    Scenes.Manager.EnableDisabledScenesAtPosition();
                }
                if (Game.UI.Cameras.Current != null)
                {
                    Game.UI.Cameras.Current.MoveCameraToTargetInstantly();
                }
                CameraFrustumOptimizer.ForceUpdate();

                // 4. Show friendly hint
                ShowNotification($"Teleportado até {friendName}!", 3f);
                WWClient.Logger.Info("TELEPORT", $"Teleportado até {friendName} em {targetPosition}");
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("TELEPORT", "Erro durante teleporte: " + ex);
            }
        }

        public void Send(Packet packet)
        {
            try
            {
                ClientSend.SendData(packet);
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("FATAL ERROR", ex.ToString());
            }
        }

        public void DestroyAllPlayers()
        {
            foreach (KeyValuePair<int, OriMPPlayer> player in Players)
            {
                if (player.Value != null)
                {
                    UnityEngine.Object.DestroyImmediate(player.Value.gameObject);
                }
            }
            Players.Clear();
        }

        public static void RegisterEntity(EntityController instance, GameObject prefab)
        {
            Entity entity = instance.transform.FindComponentUpwards<Entity>();
            if (entity == null) return;
            GameObject gameObject = entity.gameObject;
            string text = gameObject.name.ToUpper().Replace(" ", string.Empty);
            for (int i = 0; i < 100; i++)
            {
                text = text.Replace(i.ToString(), string.Empty);
            }
            EntitySync entitySync = gameObject.AddComponent<EntitySync>();
            entitySync.StartSync(text);
            GameObject gameObject2 = UnityEngine.Object.Instantiate(prefab);
            gameObject2.AddComponent<EntitySync>();
            if (Client.NetworkGameObjects.ContainsKey(text))
            {
                if (Client.NetworkGameObjects[text] == null)
                {
                    Client.NetworkGameObjects[text] = gameObject2;
                }
            }
            else
            {
                Client.NetworkGameObjects.Add(text, gameObject2);
            }
        }

        public static void ReceiveCallback(int packetid, Packet packet)
        {
            MPGameManager gm = Instance;
            if (gm == null) return;

            try
            {
                switch ((PacketType)packetid)
                {
                    case PacketType.ID:
                    {
                        gm.LocalColor = MPExtensions.ReadColor32(ref packet);
                        WWClient.Logger.Debug("CLIENT", $"Sua cor local configurada para: {gm.LocalColor}");
                        if (gm.Ori != null)
                        {
                            gm.Ori.SetupOriColor(gm.LocalColor);
                        }
                        break;
                    }
                    case PacketType.ANIM:
                    {
                        if (gm.IsSyncScene)
                        {
                            int id = packet.ReadInt();
                            string animation = packet.ReadString();
                            if (gm.HasPlayer(id) && gm.Players[id] != null)
                            {
                                gm.Players[id].SetAnimation(animation);
                            }
                            else
                            {
                                gm.SpawnPlayer(id, "", new Vector2(0f, 0f));
                            }
                        }
                        break;
                    }
                    case PacketType.POSITION:
                    {
                        if (gm.IsSyncScene)
                        {
                            int id = packet.ReadInt();
                            Vector3 position = packet.ReadVector3();
                            Color32 color = MPExtensions.ReadColor32(ref packet);
                            bool faceLeft = packet.ReadBool();

                            string nick = "";
                            if (packet.UnreadLength() > 0)
                            {
                                nick = packet.ReadString();
                            }

                            if (gm.HasPlayer(id) && gm.Players[id] != null)
                            {
                                gm.Players[id].transform.position = position;
                                gm.Players[id].SetupOriColor(color);
                                if (gm.Players[id].CharacterSpriteMirror != null)
                                {
                                    gm.Players[id].CharacterSpriteMirror.FaceLeft = faceLeft;
                                }
                                if (!string.IsNullOrEmpty(nick))
                                {
                                    gm.Players[id].SetNickname(nick);
                                }
                            }
                            else
                            {
                                gm.SpawnPlayer(id, nick, position);
                            }
                        }
                        break;
                    }
                    case PacketType.SKILL:
                    {
                        if (gm.IsSyncScene)
                        {
                            int id = packet.ReadInt();
                            CoopSkillType skill = (CoopSkillType)packet.ReadInt();
                            if (gm.HasPlayer(id) && gm.Players[id] != null)
                            {
                                gm.Players[id].UseSkill(skill);
                            }
                        }
                        break;
                    }
                    case PacketType.COLOR:
                    {
                        int id = packet.ReadInt();
                        Color color = packet.ReadColor();
                        if (!gm.Colors.ContainsKey(id))
                        {
                            gm.Colors.Add(id, color);
                        }
                        else
                        {
                            gm.Colors[id] = color;
                        }

                        if (gm.HasPlayer(id) && gm.Players[id] != null)
                        {
                            gm.Players[id].SetupOriColor(color);
                        }
                        break;
                    }
                    case PacketType.DISCONNECT:
                    {
                        int id = packet.ReadInt();
                        if (gm.Players.TryGetValue(id, out var pl) && pl != null)
                        {
                            UnityEngine.Object.DestroyImmediate(pl.gameObject);
                            gm.Players.Remove(id);
                            WWClient.Logger.Info("CLIENT", $"Jogador {id} desconectado e removido.");
                        }
                        break;
                    }
                    case PacketType.SYNC_ABILITY:
                    {
                        int senderId = packet.ReadInt();
                        int abilityId = packet.ReadInt();
                        gm.ReceiveAbilitySync((AbilityType)abilityId);
                        break;
                    }
                    case PacketType.SYNC_LEVER:
                    {
                        int senderId = packet.ReadInt();
                        int a = packet.ReadInt();
                        int b = packet.ReadInt();
                        int c = packet.ReadInt();
                        int d = packet.ReadInt();
                        int dir = packet.ReadInt();
                        WorldSyncManager.ReceiveLeverSync(a, b, c, d, dir);
                        break;
                    }
                    case PacketType.SYNC_DOOR:
                    {
                        int senderId = packet.ReadInt();
                        int a = packet.ReadInt();
                        int b = packet.ReadInt();
                        int c = packet.ReadInt();
                        int d = packet.ReadInt();
                        WorldSyncManager.ReceiveDoorSync(a, b, c, d);
                        break;
                    }
                    case PacketType.SYNC_WORLDEVENT:
                    {
                        int senderId = packet.ReadInt();
                        int a = packet.ReadInt();
                        int b = packet.ReadInt();
                        int c = packet.ReadInt();
                        int d = packet.ReadInt();
                        int state = packet.ReadInt();
                        WorldSyncManager.ReceiveWorldEventSync(a, b, c, d, state);
                        break;
                    }
                    case PacketType.CONFIG_SYNC:
                    {
                        gm.Config.AllowTeleport = packet.ReadBool();
                        gm.Config.ShareAbilities = packet.ReadBool();
                        gm.Config.ShareStoryOnly = packet.ReadBool();
                        gm.Config.ShareWorldEvents = packet.ReadBool();
                        gm.Config.ShareDoorsAndLevers = packet.ReadBool();
                        gm.Config.ShowNicknames = packet.ReadBool();
                        WWClient.Logger.Info("CLIENT", "Configurações sincronizadas com o servidor!");
                        break;
                    }
                    case PacketType.TELEPORT_REQUEST:
                    {
                        Vector3 targetPosition = packet.ReadVector3();
                        string targetName = packet.UnreadLength() > 0
                            ? packet.ReadString()
                            : "o jogador selecionado";
                        gm.TeleportLocalSein(targetPosition, targetName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("CLIENT", "Erro no callback UDP: " + ex);
            }
        }

        private void OnDestroy()
        {
            DestroyAllPlayers();
            Packet packet = new Packet();
            packet.Write((int)PacketType.DISCONNECT);
            Send(packet);
        }
    }
}
