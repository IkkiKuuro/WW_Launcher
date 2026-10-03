using UnityEngine;
using WWClient;
using WWClient.API;
using WWClient.Network;

namespace MP_Client.Sync
{
    public class EntitySync : NetworkBehaviour
    {
        private SpriteEntity _Entity;
        private SpriteAnimatorWithTransitions _Animator;
        private float Timer = 0f;
        private string LatesAnim;

        public SpriteEntity Entity
        {
            get
            {
                if (_Entity == null)
                {
                    Entity component = GetComponent<Entity>();
                    if (component == null)
                    {
                        return null;
                    }
                    _Entity = component as SpriteEntity;
                }
                return _Entity;
            }
        }

        public SpriteAnimatorWithTransitions Animator
        {
            get
            {
                if (!_Animator)
                {
                    if (!Entity)
                    {
                        _Animator = GetComponent<SpriteAnimatorWithTransitions>();
                    }
                    else
                    {
                        _Animator = Entity.SpriteAnimator;
                    }
                }
                return _Animator;
            }
        }

        public void UpdateFace(bool Left)
        {
            Vector3 localEulerAngles = base.transform.localEulerAngles;
            localEulerAngles.y = (Left ? 180 : 0);
            base.transform.localEulerAngles = localEulerAngles;
        }

        private void OnDisable()
        {
            this.Info("DISABLED!");
            if (Client.SpawnedNetworkObjects.TryGetValue(base.ClientId, out var value) && value.ContainsKey(base.NetId))
            {
                value.Remove(base.NetId);
            }
            if (base.SyncStarted)
            {
                Packet packet = new Packet(-3);
                packet.Write(value: true);
                packet.Write(base.NetId);
                packet.Write(base.Type);
                ClientSend.SendData(packet);
            }
            Object.Destroy(this);
        }

        private void Update()
        {
            if (!base.SyncStarted && base.ReciveStarted)
            {
                Timer += 0.01f;
                if (Timer > 1f)
                {
                    Object.Destroy(base.gameObject);
                }
            }
        }

        private void PlayAnim(string anim)
        {
            if (LatesAnim != anim)
            {
                LatesAnim = anim;
                if (MPGameManager.Instance != null && MPGameManager.Instance.Animations.TryGetValue(anim, out var value) && (bool)Animator)
                {
                    Animator.SetAnimation(value, ignoreIfSameAnimation: true);
                }
            }
        }

        public override void ReadPacket(ref Packet packet)
        {
            Timer = 0f;
            base.transform.position = packet.ReadVector3();
            PlayAnim(packet.ReadString().ToUpper());
            UpdateFace(packet.ReadBool());
        }

        public override bool WritePacket(ref Packet packet)
        {
            if (!base.enabled || Animator == null || Animator.CurrentAnimation == null || Entity == null)
            {
                return false;
            }
            packet.Write(base.transform.position);
            packet.Write(Animator.CurrentAnimation.name);
            packet.Write(Entity.FaceLeft);
            return true;
        }
    }
}
