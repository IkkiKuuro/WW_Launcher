using System;
using Game;
using UnityEngine;
using WWClient.Network;

namespace MP_Client.UI
{
    public class CoopHUD : MonoBehaviour
    {
        public static bool ShowHUD = false;
        private Rect _windowRect = new Rect(25, 25, 430, 560);

        private GUIStyle _boxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _sectionHeaderStyle;
        private GUIStyle _itemStyle;
        private GUIStyle _statusConnectedStyle;
        private GUIStyle _statusDisconnectedStyle;
        private GUIStyle _warningBoxStyle;
        private bool _stylesInitialized = false;
        private static int _lastToggleFrame = -1;
        private static readonly GUILayoutOption[] NoLayoutOptions = new GUILayoutOption[0];

        private string _serverIp = "127.0.0.1";
        private string _serverPort = "7777";
        private string _nickInput = "";
        private static readonly Color[] LocalColors =
        {
            Color.cyan,
            Color.green,
            Color.yellow,
            new Color(1f, 0.35f, 0.75f),
            new Color(0.65f, 0.35f, 1f),
            Color.white
        };

        public static void AddToast(string message, float duration = 4f)
        {
            if (string.IsNullOrEmpty(message)) return;
            try
            {
                Game.UI.Hints.Show(CustomMessageProvider.Create(message), HintLayer.HintZone, duration);
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("NOTIFICATION", "Erro ao exibir notificação temática: " + ex);
            }
        }

        private void Start()
        {
            WWClient.Logger.Info("CoopHUD", "Painel F8 inicializado.");
            if (ORIDEClientModule.GameData != null)
            {
                _serverIp = ORIDEClientModule.GameData.ServerIP;
                _serverPort = ORIDEClientModule.GameData.ServerPort.ToString();
                _nickInput = ORIDEClientModule.GameData.NickName;
            }
            else
            {
                _nickInput = Client.Nick;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;

            Texture2D darkBg = new Texture2D(1, 1);
            darkBg.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.13f, 0.94f));
            darkBg.Apply();

            Texture2D warnBg = new Texture2D(1, 1);
            warnBg.SetPixel(0, 0, new Color(0.25f, 0.18f, 0.05f, 0.92f));
            warnBg.Apply();

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = darkBg;
            _boxStyle.padding = new RectOffset(16, 16, 16, 16);

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 15;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = new Color(0.35f, 0.88f, 1f);

            _sectionHeaderStyle = new GUIStyle(GUI.skin.label);
            _sectionHeaderStyle.fontSize = 13;
            _sectionHeaderStyle.fontStyle = FontStyle.Bold;
            _sectionHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.4f);

            _itemStyle = new GUIStyle(GUI.skin.label);
            _itemStyle.fontSize = 12;
            _itemStyle.normal.textColor = Color.white;

            _statusConnectedStyle = new GUIStyle(GUI.skin.label);
            _statusConnectedStyle.fontSize = 12;
            _statusConnectedStyle.fontStyle = FontStyle.Bold;
            _statusConnectedStyle.normal.textColor = new Color(0.3f, 1f, 0.4f);

            _statusDisconnectedStyle = new GUIStyle(GUI.skin.label);
            _statusDisconnectedStyle.fontSize = 12;
            _statusDisconnectedStyle.fontStyle = FontStyle.Bold;
            _statusDisconnectedStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);

            _warningBoxStyle = new GUIStyle(GUI.skin.box);
            _warningBoxStyle.normal.background = warnBg;
            _warningBoxStyle.padding = new RectOffset(10, 10, 10, 10);

            _stylesInitialized = true;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
            {
                ToggleHUD();
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F8)
            {
                ToggleHUD();
                Event.current.Use();
            }

            if (ShowHUD)
            {
                _windowRect = GUI.Window(999123, _windowRect, DrawWindow, "Ori Coop Plus - Painel Multijogador [F8]");
            }
        }

        private static void ToggleHUD()
        {
            if (_lastToggleFrame == Time.frameCount) return;
            _lastToggleFrame = Time.frameCount;
            ShowHUD = !ShowHUD;
            WWClient.Logger.Info("CoopHUD", "Painel F8 " + (ShowHUD ? "aberto." : "fechado."));
        }

        private void DrawWindow(int windowId)
        {
            try
            {

                if (_nickInput == null) _nickInput = "OriPlayer";
                if (_serverIp == null) _serverIp = "127.0.0.1";
                if (_serverPort == null) _serverPort = "7777";

            GUILayout.Label("Ori and the Blind Forest: DE - Multiplayer Coop", _titleStyle);
            GUILayout.Space(6);

            // ================= 1. CONEXÃO COM O SERVIDOR =================
            GUILayout.Label("--- 1. Conexão com o Servidor ---", _sectionHeaderStyle);

            bool connected = Client.isConnected;
            bool connecting = Client.isConnecting;

            if (connected)
            {
                string ipInfo = Client.Instance != null ? $"{Client.Instance.ip}:{Client.Instance.port}" : "127.0.0.1:7777";
                int myId = Client.Instance != null ? Client.Instance.myId : -1;
                GUILayout.Label($"● Status: CONECTADO ({ipInfo}) - Seu ID: {myId}", _statusConnectedStyle);

                if (GUILayout.Button("■ Desconectar do Servidor", GUILayout.Height(24)))
                {
                    if (Client.Instance != null) Client.Instance.Disconnect();
                    AddToast("Desconectado do servidor.", 3f);
                }
            }
            else if (connecting)
            {
                GUILayout.Label("● Status: CONECTANDO ao servidor...", _statusDisconnectedStyle);
                if (GUILayout.Button("Cancelar Tentativa", GUILayout.Height(24)))
                {
                    if (Client.Instance != null) Client.Instance.CancelConnect();
                }
            }
            else
            {
                GUILayout.Label("● Status: DESCONECTADO (Não conectado a um servidor)", _statusDisconnectedStyle);
                GUILayout.Space(3);

                // Option 1: Be the HOST
                if (GUILayout.Button("👑 CRIAR / HOSPEDAR PARTIDA (Iniciar Meu Servidor)", GUILayout.Height(30)))
                {
                    StartHostServer();
                }

                GUILayout.Space(4);
                GUILayout.Label("Ou conectar a um amigo / servidor existente:", _itemStyle);

                GUILayout.BeginHorizontal(NoLayoutOptions);
                GUILayout.Label("IP:", GUILayout.Width(25));
                _serverIp = GUILayout.TextField(_serverIp, GUILayout.Width(130));
                GUILayout.Label("Porta:", GUILayout.Width(45));
                _serverPort = GUILayout.TextField(_serverPort, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                if (GUILayout.Button("▶ Conectar ao Servidor", GUILayout.Height(28)))
                {
                    ConnectToServer();
                }
            }

            GUILayout.BeginHorizontal(NoLayoutOptions);
            GUILayout.Label("Seu Nick:", GUILayout.Width(65));
            _nickInput = GUILayout.TextField(_nickInput, GUILayout.Width(140));
            if (GUILayout.Button("Salvar Nick", GUILayout.Width(90)))
            {
                Client.Nick = _nickInput.Trim();
                if (ORIDEClientModule.GameData != null)
                {
                    ORIDEClientModule.GameData.NickName = Client.Nick;
                    ORIDEClientModule.SaveSettings();
                }
                AddToast($"Nick alterado para: {Client.Nick}", 3f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.Label("--- 2. Aparencia ---", _sectionHeaderStyle);
            Color localColor = ORIDEClientModule.GameData != null
                ? ORIDEClientModule.GameData.LocalColor
                : Color.cyan;
            GUILayout.Label($"Cor local: {localColor}", _itemStyle);
            if (GUILayout.Button("Alterar cor do meu Ori", GUILayout.Height(24)))
            {
                ChangeLocalColor();
            }

            // ================= 3. DIAGNÓSTICO DO SAVE / PERSONAGEM =================
            GUILayout.Label("--- 3. Diagnóstico do Jogo / Save ---", _sectionHeaderStyle);

            bool oriActive = Characters.Sein != null;
            if (oriActive)
            {
                Vector3 pos = Characters.Sein.Position;
                GUILayout.Label($"● Ori no Mundo: ATIVO (Pos: {pos.x:F1}, {pos.y:F1})", _statusConnectedStyle);
                GUILayout.Label("Personagem pronto! Outros jogadores aparecerão ao seu lado.", _itemStyle);
            }
            else
            {
                GUILayout.BeginVertical(_warningBoxStyle, NoLayoutOptions);
                GUILayout.Label("● Ori no Mundo: INATIVO (Prólogo ou Menu Inicial)", _statusDisconnectedStyle);
                GUILayout.Space(4);
                GUILayout.Label("⚠️ <b>ATENÇÃO AO TESTAR EM SAVE NOVO:</b>", _itemStyle);
                GUILayout.Label("No Ori DE, um save novo começa no <b>PRÓLOGO</b> (jogando com a Naru).", _itemStyle);
                GUILayout.Label("O Ori e os outros jogadores <b>só aparecem na fase normal</b> após o prólogo!", _itemStyle);
                GUILayout.Space(4);
                GUILayout.Label("<b>Como testar o multiplayer agora:</b>", _itemStyle);
                GUILayout.Label("  1. Ao criar um novo save, marque a opção <b>'Pular Prólogo'</b> (Skip); OU", _itemStyle);
                GUILayout.Label("  2. Carregue um save já avançado onde você já controla o Ori; OU", _itemStyle);
                GUILayout.Label("  3. Prossiga a história do prólogo até o Ori adulto acordar!", _itemStyle);
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);

            // ================= 4. CONFIGURAÇÕES SINCRONIZADAS DO SERVIDOR =================
            GUILayout.Label("--- 4. Regras do Servidor ---", _sectionHeaderStyle);
            GUILayout.Label($"• Teleporte [Tecla T]: {(ORIDEClientModule.GM != null && ORIDEClientModule.GM.Config.AllowTeleport ? "ATIVADO" : "DESATIVADO")}", _itemStyle);
            GUILayout.Label($"• Compartilhar Habilidades: {(ORIDEClientModule.GM != null && ORIDEClientModule.GM.Config.ShareAbilities ? "ATIVADO" : "DESATIVADO")}", _itemStyle);
            GUILayout.Label($"• Portas e Alavancas: {(ORIDEClientModule.GM != null && ORIDEClientModule.GM.Config.ShareDoorsAndLevers ? "ATIVADO" : "DESATIVADO")}", _itemStyle);
            GUILayout.Label($"• Eventos de História: {(ORIDEClientModule.GM != null && ORIDEClientModule.GM.Config.ShareWorldEvents ? "ATIVADO" : "DESATIVADO")}", _itemStyle);
            GUILayout.Label($"• Nomes em Cima do Boneco: {(ORIDEClientModule.GM != null && ORIDEClientModule.GM.Config.ShowNicknames ? "ATIVADO" : "DESATIVADO")}", _itemStyle);

            GUILayout.Space(10);

            // ================= 5. JOGADORES CONECTADOS =================
            GUILayout.Label("--- 5. Jogadores Conectados ---", _sectionHeaderStyle);
            if (ORIDEClientModule.GM != null && ORIDEClientModule.GM.Players.Count > 0)
            {
                foreach (var kvp in ORIDEClientModule.GM.Players)
                {
                    OriMPPlayer pl = kvp.Value;
                    if (pl == null) continue;

                    float dist = 0f;
                    if (Characters.Sein != null)
                    {
                        dist = Vector3.Distance(Characters.Sein.Position, pl.transform.position);
                    }

                    GUILayout.BeginHorizontal(NoLayoutOptions);
                    string playerName = string.IsNullOrEmpty(pl.NickName) ? $"Jogador {kvp.Key}" : pl.NickName;
                    GUILayout.BeginVertical(NoLayoutOptions);
                    GUILayout.Label($"{playerName} (ID {kvp.Key}) - {dist:F1}m", _itemStyle);
                    GUI.enabled = ORIDEClientModule.GM.Config.AllowTeleport;
                    if (GUILayout.Button($"Teleportar até {playerName}", GUILayout.Height(24)))
                    {
                        ORIDEClientModule.GM.TeleportToPlayer(kvp.Key);
                    }
                    GUI.enabled = true;
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("Nenhum outro jogador conectado no momento.", _itemStyle);
            }

            GUILayout.Space(10);

            // ================= 6. FERRAMENTAS DE TESTE RÁPIDO =================
            GUILayout.Label("--- 6. Ferramentas de Teste Rápido ---", _sectionHeaderStyle);

            if (GUILayout.Button("🔔 Testar Notificação na Tela", GUILayout.Height(24)))
            {
                AddToast("Notificação de teste funcionando com sucesso na tela!", 4f);
            }

            GUILayout.Space(12);

            if (GUILayout.Button("Fechar Painel [F8]", GUILayout.Height(26)))
            {
                ShowHUD = false;
            }

            }
            catch (Exception ex)
            {
                GUILayout.Label($"<color=red>GUI ERROR: {ex.Message}</color>");
                WWClient.Logger.Error("CoopHUD", "Error in DrawWindow: " + ex);
            }
            finally
            {
                GUI.DragWindow();
            }
        }

        private void ChangeLocalColor()
        {
            if (ORIDEClientModule.GameData == null)
            {
                AddToast("As configurações do mod ainda não foram carregadas.", 3f);
                return;
            }

            int nextColor = 0;
            for (int i = 0; i < LocalColors.Length; i++)
            {
                if (Vector4.Distance(ORIDEClientModule.GameData.LocalColor, LocalColors[i]) < 0.01f)
                {
                    nextColor = (i + 1) % LocalColors.Length;
                    break;
                }
            }

            ORIDEClientModule.GameData.LocalColor = LocalColors[nextColor];
            ORIDEClientModule.SaveSettings();

            if (ORIDEClientModule.GM != null)
            {
                ORIDEClientModule.GM.ApplyLocalColor(ORIDEClientModule.GameData.LocalColor);
                ORIDEClientModule.GM.SendColor();
            }

            AddToast("Cor do seu Ori alterada.", 3f);
        }

        private void StartHostServer()
        {
            try
            {
                string serverExe = System.IO.Path.Combine(ORIDEClientModule.MainOriDir, "Server\\WWDedicatedServer.exe");
                if (System.IO.File.Exists(serverExe))
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = serverExe,
                        Arguments = "--auto",
                        WorkingDirectory = System.IO.Path.GetDirectoryName(serverExe),
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                    AddToast("Servidor Host iniciado! Conecte-se abaixo se não for automático...", 4f);
                    _serverIp = "127.0.0.1";
                    _serverPort = "7777";
                }
                else
                {
                    AddToast("WWDedicatedServer.exe não encontrado na pasta Server!", 4f);
                }
            }
            catch (Exception ex)
            {
                WWClient.Logger.Error("HOST", "Erro ao iniciar host: " + ex.Message);
                AddToast("Erro ao iniciar host: " + ex.Message, 4f);
            }
        }

        private void ConnectToServer()
        {
            if (Client.Instance != null)
            {
                Client.Instance.ip = _serverIp.Trim();
                if (int.TryParse(_serverPort.Trim(), out int port))
                {
                    Client.Instance.port = port;
                }
                if (ORIDEClientModule.GameData != null)
                {
                    ORIDEClientModule.GameData.ServerIP = Client.Instance.ip;
                    ORIDEClientModule.GameData.ServerPort = Client.Instance.port;
                    ORIDEClientModule.SaveSettings();
                }
                WWClient.Logger.Info("CLIENT", $"Iniciando conexão manual com {Client.Instance.ip}:{Client.Instance.port}...");
                try
                {
                    Client.Instance.ConnectToServer();
                    AddToast($"Tentando conectar a {Client.Instance.ip}:{Client.Instance.port}...", 3f);
                }
                catch (Exception ex)
                {
                    WWClient.Logger.Error("CLIENT", "Falha ao iniciar conexão manual: " + ex);
                    AddToast("Falha ao iniciar a conexão: " + ex.Message, 4f);
                }
            }
            else
            {
                WWClient.Logger.Error("CLIENT", "Não foi possível conectar: a API de rede ainda não inicializou.");
                AddToast("A rede do cliente ainda não inicializou. Tente novamente em alguns segundos.", 4f);
            }
        }
    }
}
