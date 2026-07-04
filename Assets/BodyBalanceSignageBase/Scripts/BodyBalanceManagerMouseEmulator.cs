using UnityEngine;
using UnityEngine.InputSystem;

namespace BodyBalanceSignage
{
    /// <summary>
    /// マウスで、バランスWiiボードから送られてくる値（重心）をエミュレートするスクリプト
    /// このスクリプトは基本的に変更しない
    /// </summary>
    public class BodyBalanceManagerMouseEmulator : MonoBehaviour
    {
        /// <summary>
        /// OSCレシーバー
        /// Balance Wii Board を管理するアプリから送られてきた重心（CenterOfGravity）の値を受信するスクリプト
        /// </summary>
        [Header("Script References")]
        [SerializeField]
        private BodyBalanceManager _oscReceiver;

        [Header("Parameters")]
        [Tooltip("最終的な出力値の倍率")]
        public float ValueWeight = 0.95f;

        [Tooltip("画面短辺の何割をドラッグしたら1.0にするか。0.5なら短辺の半分で1.0")]
        [Range(0.01f, 1.0f)]
        public float DragNormalizeRatio = 0.5f;

        [Tooltip("値を -1.0 ～ 1.0 に制限する")]
        public bool ClampValue = true;

        [Tooltip("マウスを離した瞬間に 0 を1回だけ送信する")]
        public bool SendZeroWhenReleased = true;

        [Header("Debug Display")]
        public bool ShowCenterOfGravityLabel = true;

        [Tooltip("ドラッグ中だけ値を表示する")]
        public bool ShowLabelOnlyWhileDragging = false;

        public Vector2 LabelOffset = new Vector2(16.0f, 16.0f);

        public int LabelFontSize = 18;

        public Color LabelTextColor = Color.white;

        public Color LabelBackgroundColor = new Color(0, 0, 0, 0.65f);

        [Header("Variables")]
        [SerializeField, ReadOnly]
        private Vector2 _clickStartPosition = Vector2.zero;

        [SerializeField, ReadOnly]
        private Vector2 _currentMousePosition = Vector2.zero;

        [SerializeField, ReadOnly]
        private Vector2 _diffPosition = Vector2.zero;

        [SerializeField, ReadOnly]
        private Vector2 _normalizedDiffPosition = Vector2.zero;

        [SerializeField, ReadOnly]
        private Vector2 _centerOfGravity = Vector2.zero;

        [SerializeField, ReadOnly]
        private bool _isPressedLeftMouseButton = false;

        private GUIStyle _labelStyle;
        private Texture2D _backgroundTexture;

        private void Update()
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return;
            }

            Vector2 mousePosition = mouse.position.ReadValue();
            _currentMousePosition = mousePosition;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _clickStartPosition = mousePosition;
                _diffPosition = Vector2.zero;
                _normalizedDiffPosition = Vector2.zero;
                _centerOfGravity = Vector2.zero;
                _isPressedLeftMouseButton = true;
            }

            if (_isPressedLeftMouseButton)
            {
                _diffPosition = mousePosition - _clickStartPosition;

                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                float shortSide = Mathf.Min(screenWidth, screenHeight);

                float normalizeDistance = shortSide * DragNormalizeRatio;

                if (normalizeDistance <= 0.0f)
                {
                    normalizeDistance = 1.0f;
                }

                _normalizedDiffPosition = _diffPosition / normalizeDistance;

                if (ClampValue)
                {
                    _normalizedDiffPosition = new Vector2(
                        Mathf.Clamp(_normalizedDiffPosition.x, -1.0f, 1.0f),
                        Mathf.Clamp(_normalizedDiffPosition.y, -1.0f, 1.0f)
                    );
                }

                _centerOfGravity = _normalizedDiffPosition * ValueWeight;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                _clickStartPosition = Vector2.zero;
                _diffPosition = Vector2.zero;
                _normalizedDiffPosition = Vector2.zero;
                _centerOfGravity = Vector2.zero;
                _isPressedLeftMouseButton = false;

                // マウスを離した瞬間だけ 0 を送信する
                // 毎フレーム送ると ReceiveCount が常に更新されてしまう
                if (_oscReceiver != null && SendZeroWhenReleased)
                {
                    _oscReceiver.AddQueue(Vector2.zero);
                }
            }

            // 押している間だけ送信する
            if (_oscReceiver != null && _isPressedLeftMouseButton)
            {
                _oscReceiver.AddQueue(_centerOfGravity);
            }
        }

        private void OnGUI()
        {
            if (!ShowCenterOfGravityLabel)
            {
                return;
            }

            if (ShowLabelOnlyWhileDragging && !_isPressedLeftMouseButton)
            {
                return;
            }

            SetupGUIStyleIfNeeded();

            // InputSystem のマウス座標は左下原点
            // OnGUI は左上原点なので Y を反転する
            float guiX = _currentMousePosition.x + LabelOffset.x;
            float guiY = Screen.height - _currentMousePosition.y + LabelOffset.y;

            string text =
                $"centerOfGravity\n" +
                $"x : {_centerOfGravity.x:0.000}\n" +
                $"y : {_centerOfGravity.y:0.000}";

            Vector2 textSize = _labelStyle.CalcSize(new GUIContent(text));

            Rect rect = new Rect(
                guiX,
                guiY,
                textSize.x + 20.0f,
                textSize.y + 16.0f
            );

            GUI.Box(rect, GUIContent.none);
            GUI.Label(
                new Rect(rect.x + 10.0f, rect.y + 8.0f, rect.width, rect.height),
                text,
                _labelStyle
            );
        }

        private void SetupGUIStyleIfNeeded()
        {
            if (_backgroundTexture == null)
            {
                _backgroundTexture = new Texture2D(1, 1);
                _backgroundTexture.SetPixel(0, 0, LabelBackgroundColor);
                _backgroundTexture.Apply();
            }

            GUI.skin.box.normal.background = _backgroundTexture;

            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = LabelFontSize;
                _labelStyle.normal.textColor = LabelTextColor;
            }

            _labelStyle.fontSize = LabelFontSize;
            _labelStyle.normal.textColor = LabelTextColor;
        }
    }
}