using System;
using System.IO;
using HarmonyLib;
using MP_Client.UI;
using UnityEngine;
using UnityEngine.Events;
using WWClient.API;
using WWClient.Network;

namespace MP_Client
{
    public class ORIDEClientModule : ClientModule
    {
        public static string MainOriDir => Application.dataPath + "/../";
        public static GameData GameData;
        private static Harmony instance;
        public static MPGameManager GM;

        public override void OnEnable()
        {
            base.Name = "Ori Coop Plus Client Module";
            Info("==========================================");
            Info(" Ori Coop Plus Client Module CARREGANDO...");
            Info("==========================================");

            if (LoadSettings())
            {
                ClientEvents.OnPacketRecived = (UnityAction<int, Packet>)Delegate.Combine(ClientEvents.OnPacketRecived, new UnityAction<int, Packet>(MPGameManager.ReceiveCallback));
                ClientEvents.OnConnected = (UnityAction)Delegate.Combine(ClientEvents.OnConnected, new UnityAction(MPGameManager.OnConnected));

                Info("Aplicando patches de compatibilidade com Harmony...");
                try
                {
                    instance = new Harmony("OriCoopPlus.Patches");
                    instance.PatchAll();
                }
                catch (Exception ex)
                {
                    string err = $"Erro ao aplicar patches: {ex}";
                    Error(err);
                    WinMessageBox.Show(err, "ORI COOP PLUS", MessageBoxButtons.Ok, MessageBoxIcon.Error);
                    Application.Quit();
                    return;
                }

                Info("Patches aplicados com sucesso!");

                // Create Game Manager
                GameObject managerObj = new GameObject("OriCoopGameManager");
                GM = managerObj.AddComponent<MPGameManager>();
                UnityEngine.Object.DontDestroyOnLoad(managerObj);

                Info("==========================================");
                Info(" Ori Coop Plus ATIVADO!");
                Info(" Tecla [T] : Teleportar até um amigo");
                Info(" Tecla [F8]: Abrir painel de status / testes");
                Info("==========================================");
            }
        }

        private static bool LoadSettings()
        {
            string path = Path.Combine(MainOriDir, "MPSettings.json");
            if (File.Exists(path))
            {
                try
                {
                    GameData = JsonUtility.FromJson<GameData>(File.ReadAllText(path));
                    ApplySettingsToClient();
                    return true;
                }
                catch (Exception)
                {
                    CreateDefaultSettings(path);
                    return true;
                }
            }
            else
            {
                CreateDefaultSettings(path);
                return true;
            }
        }

        public static void ApplySettingsToClient()
        {
            if (GameData == null) return;

            if (!string.IsNullOrEmpty(GameData.NickName))
            {
                Client.Nick = GameData.NickName;
            }

            if (Client.Instance != null)
            {
                if (!string.IsNullOrEmpty(GameData.ServerIP))
                {
                    Client.Instance.ip = GameData.ServerIP;
                }
                if (GameData.ServerPort > 0)
                {
                    Client.Instance.port = GameData.ServerPort;
                }
            }
        }

        public static void SaveSettings()
        {
            try
            {
                string path = Path.Combine(MainOriDir, "MPSettings.json");
                if (GameData != null)
                {
                    File.WriteAllText(path, JsonUtility.ToJson(GameData, true));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erro ao salvar MPSettings.json: " + ex);
            }
        }

        private static void CreateDefaultSettings(string path)
        {
            try
            {
                GameData = new GameData(Color.cyan);
                GameData.NickName = "Ori_" + UnityEngine.Random.Range(100, 999);
                ApplySettingsToClient();
                File.WriteAllText(path, JsonUtility.ToJson(GameData, true));
            }
            catch (Exception ex)
            {
                Debug.LogError("Erro ao criar MPSettings.json padrão: " + ex);
            }
        }
    }
}
