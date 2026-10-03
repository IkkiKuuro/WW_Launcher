using System.Collections.Generic;
using MP_Client.Data;
using MP_Client.UI;
using OriCoop;
using UnityEngine;

namespace MP_Client
{
    public class OriMPPlayer : MonoBehaviour
    {
        public CharacterSpriteMirror CharacterSpriteMirror;
        public bool LocalOri = false;
        public CharacterAnimationSystem Animator;
        public SeinSpiritMP SeinSpirit;
        public SeinStompMP SeinStomp;
        public FloatingNameTag NameTag;

        public string NickName = "Ori";
        private List<MeshRenderer> Colors = new List<MeshRenderer>();
        private Color LocalColor = Color.white;
        public int Id = 0;

        public void InitLocalPlayer()
        {
            CharacterSpriteMirror = base.gameObject.GetComponentInChildren<CharacterSpriteMirror>();
            LocalOri = true;
            Animator = base.gameObject.GetComponentInChildren<CharacterAnimationSystem>();
            SeinSpirit = base.gameObject.AddComponent<SeinSpiritMP>();
            SeinStomp = base.gameObject.AddComponent<SeinStompMP>();
            Animator.gameObject.AddComponent<SeinPickupMPProcessor>();
            SetupOriColor(Color.cyan);
        }

        public void InitOtherPlayer(int id, string username)
        {
            Id = id;
            NickName = string.IsNullOrEmpty(username) ? ("Jogador " + id) : username;

            if (MPGameManager.Instance != null && MPGameManager.Instance.Colors.ContainsKey(id))
            {
                SetupOriColor(MPGameManager.Instance.Colors[id]);
            }

            Animator = base.gameObject.GetComponentInChildren<CharacterAnimationSystem>();
            CharacterSpriteMirror = base.gameObject.GetComponentInChildren<CharacterSpriteMirror>();

            SpriteAnimator spriteAnim = base.gameObject.FindComponentInChildren<SpriteAnimator>();
            if (spriteAnim != null)
            {
                spriteAnim.gameObject.SetActive(false);
            }

            SeinSpirit = base.gameObject.AddComponent<SeinSpiritMP>();
            SeinStomp = base.gameObject.AddComponent<SeinStompMP>();

            // Attach floating nickname above player head!
            NameTag = FloatingNameTag.Attach(base.gameObject, NickName);
        }

        public void SetNickname(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            NickName = name;
            if (NameTag != null)
            {
                NameTag.SetNickname(name);
            }
        }

        public void SetupOriColor(Color color)
        {
            if (LocalColor == color) return;

            MeshRenderer[] components = base.gameObject.GetComponents<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in components)
            {
                if (meshRenderer.gameObject.name == "sprite")
                {
                    meshRenderer.material.color = color;
                    if (!Colors.Contains(meshRenderer))
                    {
                        Colors.Add(meshRenderer);
                    }
                }
            }

            MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer2 in componentsInChildren)
            {
                if (meshRenderer2.gameObject.name == "sprite")
                {
                    meshRenderer2.material.color = color;
                    if (!Colors.Contains(meshRenderer2))
                    {
                        Colors.Add(meshRenderer2);
                    }
                }
            }

            LocalColor = color;
            if (NameTag != null)
            {
                NameTag.SetColor(new Color(Mathf.Clamp01(color.r + 0.3f), Mathf.Clamp01(color.g + 0.3f), Mathf.Clamp01(color.b + 0.3f)));
            }
        }

        public void SetAnimation(string animName)
        {
            if (Animator != null && MPGameManager.Instance != null && MPGameManager.Instance.TryGetAnimation(animName, out var anim))
            {
                Animator.Animator.SetAnimation(anim);
            }
        }

        public void UseSkill(CoopSkillType type)
        {
            switch (type)
            {
                case CoopSkillType.Spirit:
                    if (SeinSpirit != null) SeinSpirit.Attack();
                    break;
                case CoopSkillType.Stomp:
                    if (SeinStomp != null) SeinStomp.Attack();
                    break;
            }
        }

        private void FixInvisible()
        {
            for (int i = 0; i < Colors.Count; i++)
            {
                MeshRenderer meshRenderer = Colors[i];
                if (meshRenderer != null)
                {
                    if (!meshRenderer.enabled) meshRenderer.enabled = true;
                    if (!meshRenderer.gameObject.activeSelf) meshRenderer.gameObject.SetActive(true);
                }
            }
        }

        private void FixedUpdate()
        {
            FixInvisible();

            if (!LocalOri) return;

            for (int i = 0; i < Colors.Count; i++)
            {
                if (Colors[i] != null && Colors[i].material.color != LocalColor)
                {
                    Colors[i].material.color = LocalColor;
                }
            }
        }
    }
}
