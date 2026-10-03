using System;
using UnityEngine;

namespace MP_Client
{
    [Serializable]
    public class GameData
    {
        public Color LocalColor = Color.cyan;
        public string NickName = "OriPlayer";
        public string ServerIP = "127.0.0.1";
        public int ServerPort = 7777;
        public bool AutoConnect = true;

        public GameData()
        {
        }

        public GameData(Color color)
        {
            LocalColor = color;
            NickName = "OriPlayer";
            ServerIP = "127.0.0.1";
            ServerPort = 7777;
            AutoConnect = true;
        }
    }
}
