using System;
using UnityEngine;

namespace MP_Client.Sync
{
    [Serializable]
    public class PlayerPos
    {
        public int PlayerId = 0;
        public Vector3 Position;
        public Vector3 Scale;
        public string NickName = "";
        public string animation = "";

        public PlayerPos(Vector3 Position, int PlayerId, string NickName, string animation, Vector3 Scale)
        {
            this.Position = Position;
            this.PlayerId = PlayerId;
            this.NickName = NickName;
            this.animation = animation;
            this.Scale = Scale;
        }
    }
}
