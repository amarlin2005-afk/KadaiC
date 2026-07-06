using UnityEngine;

namespace BodyBalanceSignage
{
    public class CameraMover : MonoBehaviour
    {
        public enum NoUpdateBehavior
        {
            ReturnImmediately,
            ReturnAfterDelay,
            Hold
        }

        public enum AxisMoveMode
        {
            Additive,
            DirectSet,
            LerpToTarget
        }

        // ★追加：方向通知（true=前進, false=後退）
        public System.Action<bool> OnMoveDirectionChanged;

        [Header("初期位置")]
        public bool UseCurrentPositionAsInitialPosition = true;
        public Vector3 InitialLocalPosition = Vector3.zero;

        [Header("X軸：移動方式")]
        public AxisMoveMode XMoveMode = AxisMoveMode.DirectSet;
        public float MoveSpeedX = 10.0f;
        public float XDirectScale = 1.0f;

        [Header("Z軸：移動方式")]
        public AxisMoveMode ZMoveMode = AxisMoveMode.Additive;
        public float MoveSpeedZ = 10.0f;
        public float ZDirectScale = 1.0f;

        [Header("OSC更新停止判定")]
        public float OscUpdateTimeout = 0.2f;

        [Header("X軸：更新停止時の挙動")]
        public NoUpdateBehavior XNoUpdateBehavior = NoUpdateBehavior.ReturnAfterDelay;
        public float XReturnDelay = 2.0f;
        public float XReturnDuration = 2.0f;

        [Header("Z軸：更新停止時の挙動")]
        public NoUpdateBehavior ZNoUpdateBehavior = NoUpdateBehavior.ReturnAfterDelay;
        public float ZReturnDelay = 2.0f;
        public float ZReturnDuration = 2.0f;

        [Header("移動範囲")]
        public float PositionRangeXMin = -2.0f;
        public float PositionRangeXMax = 2.0f;
        public float PositionRangeZMin = -10.0f;
        public float PositionRangeZMax = 10.0f;

        [SerializeField, ReadOnly]
        Vector2 _centerOfGravity = Vector2.zero;

        [SerializeField, ReadOnly]
        Vector3 _cameraPosition = Vector3.zero;

        [Header("参照")]
        [SerializeField] Camera _cameraRef = default;
        [SerializeField] BodyBalanceManager _bodyBalanceManager = default;

        int _lastReceiveCount = -1;
        float _lastOscUpdateTime = -999.0f;
        bool _wasOscUpdating = false;

        bool _isReturningX = false;
        float _returnElapsedX = 0.0f;
        float _returnStartX = 0.0f;

        bool _isReturningZ = false;
        float _returnElapsedZ = 0.0f;
        float _returnStartZ = 0.0f;

        // ★追加：前フレーム位置
        float _lastZ;

        void Awake()
        {
            if (_cameraRef == null)
                _cameraRef = GetComponent<Camera>();

            if (_cameraRef == null)
                return;

            if (UseCurrentPositionAsInitialPosition)
                InitialLocalPosition = _cameraRef.transform.localPosition;
            else
                _cameraRef.transform.localPosition = InitialLocalPosition;

            _cameraPosition = _cameraRef.transform.localPosition;

            // ★追加
            _lastZ = _cameraPosition.z;

            if (_bodyBalanceManager != null)
                _lastReceiveCount = _bodyBalanceManager.ReceiveCount;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (_cameraRef == null) return;

            bool receivedNewOsc = false;

            if (_bodyBalanceManager != null)
            {
                if (_bodyBalanceManager.ReceiveCount != _lastReceiveCount)
                {
                    _lastReceiveCount = _bodyBalanceManager.ReceiveCount;
                    _lastOscUpdateTime = Time.time;
                    _centerOfGravity = _bodyBalanceManager.CenterOfGravity;
                    receivedNewOsc = true;
                }
            }

            bool isOscUpdating = Time.time - _lastOscUpdateTime <= OscUpdateTimeout;

            if (isOscUpdating)
            {
                if (!_wasOscUpdating || receivedNewOsc)
                {
                    CancelReturnX();
                    CancelReturnZ();
                }

                _cameraPosition.x = ApplyInputMove(
                    _cameraPosition.x,
                    _centerOfGravity.x,
                    XMoveMode,
                    MoveSpeedX,
                    XDirectScale,
                    InitialLocalPosition.x,
                    dt
                );

                _cameraPosition.z = ApplyInputMove(
                    _cameraPosition.z,
                    _centerOfGravity.y,
                    ZMoveMode,
                    MoveSpeedZ,
                    ZDirectScale,
                    InitialLocalPosition.z,
                    dt
                );

                _cameraPosition.y = InitialLocalPosition.y;
            }
            else
            {
                float noUpdateElapsed = Time.time - _lastOscUpdateTime;

                _cameraPosition.x = ApplyReturnToInitial(
                    _cameraPosition.x,
                    InitialLocalPosition.x,
                    XNoUpdateBehavior,
                    XReturnDelay,
                    XReturnDuration,
                    noUpdateElapsed,
                    dt,
                    ref _isReturningX,
                    ref _returnElapsedX,
                    ref _returnStartX
                );

                _cameraPosition.z = ApplyReturnToInitial(
                    _cameraPosition.z,
                    InitialLocalPosition.z,
                    ZNoUpdateBehavior,
                    ZReturnDelay,
                    ZReturnDuration,
                    noUpdateElapsed,
                    dt,
                    ref _isReturningZ,
                    ref _returnElapsedZ,
                    ref _returnStartZ
                );

                _cameraPosition.y = InitialLocalPosition.y;
            }

            // ★★★ここが本体：方向検出★★★

            float deltaZ = _cameraPosition.z - _lastZ;

            if (Mathf.Abs(deltaZ) > 0.0001f)
            {
                bool isForward = deltaZ > 0f;

                OnMoveDirectionChanged?.Invoke(isForward);
            }

            _lastZ = _cameraPosition.z;

            _wasOscUpdating = isOscUpdating;

            _cameraPosition.x = Mathf.Clamp(_cameraPosition.x, PositionRangeXMin, PositionRangeXMax);
            _cameraPosition.z = Mathf.Clamp(_cameraPosition.z, PositionRangeZMin, PositionRangeZMax);

            _cameraRef.transform.localPosition = _cameraPosition;
        }

        float ApplyInputMove(float current, float input, AxisMoveMode moveMode, float additiveSpeed, float directScale, float initialValue, float dt)
        {
            switch (moveMode)
            {
                case AxisMoveMode.Additive:
                    return current + input * additiveSpeed * dt;

                case AxisMoveMode.DirectSet:
                    return initialValue + input * directScale;

                case AxisMoveMode.LerpToTarget:
                    return Mathf.Lerp(current, initialValue + input * directScale, dt * additiveSpeed);
            }
            return current;
        }

        float ApplyReturnToInitial(float current, float target, NoUpdateBehavior behavior, float delay, float duration, float noUpdateElapsed, float dt, ref bool isReturning, ref float returnElapsed, ref float returnStart)
        {
            if (behavior == NoUpdateBehavior.Hold)
            {
                isReturning = false;
                returnElapsed = 0;
                return current;
            }

            float actualDelay = behavior == NoUpdateBehavior.ReturnAfterDelay ? delay : 0f;

            if (noUpdateElapsed < actualDelay)
                return current;

            if (!isReturning)
            {
                isReturning = true;
                returnElapsed = 0;
                returnStart = current;
            }

            returnElapsed += dt;

            float t = Mathf.Clamp01(returnElapsed / Mathf.Max(0.0001f, duration));
            float eased = t * t * (3f - 2f * t);

            if (t >= 1f)
                return target;

            return Mathf.LerpUnclamped(returnStart, target, eased);
        }

        void CancelReturnX()
        {
            _isReturningX = false;
            _returnElapsedX = 0;
        }

        void CancelReturnZ()
        {
            _isReturningZ = false;
            _returnElapsedZ = 0;
        }

        
    }
    
}