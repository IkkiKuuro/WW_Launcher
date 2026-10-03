using UnityEngine;

namespace MP_Client.UI
{
    public class FloatingNameTag : MonoBehaviour
    {
        private TextMesh _textMesh;
        private TextMesh _shadowMesh;
        private string _nick = "Player";
        private Transform _targetTransform;

        public static FloatingNameTag Attach(GameObject target, string nickname)
        {
            GameObject container = new GameObject("CoopNameTag");
            container.transform.SetParent(target.transform, false);

            FloatingNameTag tag = container.AddComponent<FloatingNameTag>();
            tag._targetTransform = target.transform;
            tag.Init(nickname);
            return tag;
        }

        private void Init(string nickname)
        {
            _nick = nickname;

            // Shadow for contrast
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(transform, false);
            shadowObj.transform.localPosition = new Vector3(0.04f, -0.04f, 0.01f);
            _shadowMesh = shadowObj.AddComponent<TextMesh>();
            _shadowMesh.text = nickname;
            _shadowMesh.fontSize = 30;
            _shadowMesh.characterSize = 0.075f;
            _shadowMesh.anchor = TextAnchor.MiddleCenter;
            _shadowMesh.alignment = TextAlignment.Center;
            _shadowMesh.color = new Color(0f, 0f, 0f, 0.85f);

            // Main Text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);
            textObj.transform.localPosition = Vector3.zero;
            _textMesh = textObj.AddComponent<TextMesh>();
            _textMesh.text = nickname;
            _textMesh.fontSize = 30;
            _textMesh.characterSize = 0.075f;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.color = Color.white;

            transform.localPosition = new Vector3(0f, 1.35f, 0f);
        }

        public void SetNickname(string nickname)
        {
            if (string.IsNullOrEmpty(nickname)) return;
            _nick = nickname;
            if (_textMesh != null) _textMesh.text = nickname;
            if (_shadowMesh != null) _shadowMesh.text = nickname;
        }

        public void SetColor(Color color)
        {
            if (_textMesh != null) _textMesh.color = color;
        }

        private void LateUpdate()
        {
            // Keep text tag upright and unflipped regardless of character orientation
            transform.rotation = Quaternion.identity;

            // Prevent scale flipping when Ori sprite mirror flips X
            if (_targetTransform != null)
            {
                float parentSign = Mathf.Sign(_targetTransform.lossyScale.x);
                transform.localScale = new Vector3(parentSign >= 0 ? 1f : -1f, 1f, 1f);
            }

            // Check if nicknames are enabled in config
            if (MPGameManager.Instance != null)
            {
                bool enabled = MPGameManager.Instance.Config.ShowNicknames;
                if (_textMesh != null && _textMesh.gameObject.activeSelf != enabled)
                {
                    _textMesh.gameObject.SetActive(enabled);
                    if (_shadowMesh != null) _shadowMesh.gameObject.SetActive(enabled);
                }
            }
        }
    }
}
