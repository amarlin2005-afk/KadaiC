using UnityEngine;

namespace BodyBalanceSignage
{
    /// <summary>
    /// 重心でカメラを前後、左右に動かす
    /// </summary>
    public class CameraMover : MonoBehaviour
    {
        /// <summary>
        /// 重心の値の更新がない場合、カメラの位置をどのように元に戻すか
        /// </summary>
        public enum NoUpdateBehavior
        {
            /// <summary>
            /// 更新停止後、すぐに補間で戻る
            /// </summary>
            ReturnImmediately,
            /// <summary>
            /// 更新停止後、指定秒数後に補間で戻る
            /// </summary>
            ReturnAfterDelay,
            /// <summary>
            /// その場に残す
            /// </summary>
            Hold
        }

        /// <summary>
        /// それぞれの軸で、どのように移動するのか
        /// </summary>
        public enum AxisMoveMode
        {
            /// <summary>
            /// 加算（累積）
            /// </summary>
            Additive,
            /// <summary>
            /// 直接代入
            /// </summary>
            DirectSet,
            /// <summary>
            /// 線形補間（若干、スムーズ）
            /// </summary>
            LerpToTarget
        }

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

        /// <summary>
        /// 重心
        /// </summary>
        [SerializeField, ReadOnly]
        Vector2 _centerOfGravity = Vector2.zero;

        /// <summary>
        /// カメラ位置
        /// </summary>
        [SerializeField, ReadOnly]
        Vector3 _cameraPosition = Vector3.zero;

        [Header("参照")]
        [SerializeField]
        Camera _cameraRef = default;

        /// <summary>
        /// 大事なやつ
        /// </summary>
        [SerializeField]
        BodyBalanceManager _bodyBalanceManager = default;

        /// <summary>
        /// 受信したメッセージのカウント
        /// </summary>
        int _lastReceiveCount = -1;
        /// <summary>
        /// 最後にOSCが送られてきた時の時間
        /// </summary>
        float _lastOscUpdateTime = -999.0f;

        /// <summary>
        /// OSCの更新があったか
        /// </summary>
        bool _wasOscUpdating = false;

        /// <summary>
        /// 初期位置に戻っている最中かどうか（X軸）
        /// </summary>
        bool _isReturningX = false;
        /// <summary>
        /// 戻り処理の経過時間（X軸）
        /// </summary>
        float _returnElapsedX = 0.0f;
        /// <summary>
        /// 戻り処理の開始時の値（X軸）
        /// </summary>
        float _returnStartX = 0.0f;

        /// <summary>
        /// 初期位置に戻っている最中かどうか（Z軸）
        /// </summary>
        bool _isReturningZ = false;
        /// <summary>
        /// 戻り処理の経過時間（Z軸）
        /// </summary>
        float _returnElapsedZ = 0.0f;
        /// <summary>
        /// 戻り処理の開始時の値（Z軸）
        /// </summary>
        float _returnStartZ = 0.0f;

        void Awake()
        {
            // 移動するカメラの参照がなければ取得する
            if (_cameraRef == null)
            {
                _cameraRef = GetComponent<Camera>();
            }
            // 移動するカメラの参照がなければ何もしない
            if (_cameraRef == null)
            {
                return;
            }

            if (UseCurrentPositionAsInitialPosition)
            {
                // 現在のカメラの位置を初期位置として登録する
                InitialLocalPosition = _cameraRef.transform.localPosition;
            }
            else
            {
                // あらかじめ登録した初期位置を現在のカメラの位置に代入する
                _cameraRef.transform.localPosition = InitialLocalPosition;
            }
            // カメラの位置を管理する変数に代入
            _cameraPosition = _cameraRef.transform.localPosition;

            // ボディバランス管理スクリプトのメッセージ受信カウントの値を代入
            if (_bodyBalanceManager != null)
            {
                _lastReceiveCount = _bodyBalanceManager.ReceiveCount;
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (_cameraRef == null)
            {
                return;
            }
            // 新しいOSCメッセージを受信したかどうか
            bool receivedNewOsc = false;

            if (_bodyBalanceManager != null)
            {
                // 受信カウントが異なれば、
                if (_bodyBalanceManager.ReceiveCount != _lastReceiveCount)
                {
                    // 重心の値を取得
                    _lastReceiveCount = _bodyBalanceManager.ReceiveCount;
                    _lastOscUpdateTime = Time.time;
                    _centerOfGravity = _bodyBalanceManager.CenterOfGravity;
                    receivedNewOsc = true;
                }
            }
            // OSCの値が更新されているかどうか（しばらく値が来なければ、カメラの位置を初期位置に戻す、等の処理を行うかどうかの判断に使用）
            bool isOscUpdating = Time.time - _lastOscUpdateTime <= OscUpdateTimeout;

            // OSCの値が更新されていれば
            if (isOscUpdating)
            {
                // OSCが来ている間は、戻り処理を停止
                if (!_wasOscUpdating || receivedNewOsc)
                {
                    CancelReturnX();
                    CancelReturnZ();
                }
                // カメラの位置（X軸）を更新
                _cameraPosition.x = ApplyInputMove(
                    current: _cameraPosition.x,
                    input: _centerOfGravity.x,
                    moveMode: XMoveMode,
                    additiveSpeed: MoveSpeedX,
                    directScale: XDirectScale,
                    initialValue: InitialLocalPosition.x,
                    dt: dt
                );

                // カメラの位置（Z軸）を更新
                _cameraPosition.z = ApplyInputMove(
                    current: _cameraPosition.z,
                    input: _centerOfGravity.y,
                    moveMode: ZMoveMode,
                    additiveSpeed: MoveSpeedZ,
                    directScale: ZDirectScale,
                    initialValue: InitialLocalPosition.z,
                    dt: dt
                );
                // カメラの位置（Y軸）を更新
                _cameraPosition.y = InitialLocalPosition.y;
            }
            // （指定時間）新しいOSCメッセージが来ていない場合
            else
            {
                // 更新がなくなってから経過した時間
                float noUpdateElapsed = Time.time - _lastOscUpdateTime;

                // 初期位置に戻す（X軸）
                _cameraPosition.x = ApplyReturnToInitial(
                    current: _cameraPosition.x,
                    target: InitialLocalPosition.x,
                    behavior: XNoUpdateBehavior,
                    delay: XReturnDelay,
                    duration: XReturnDuration,
                    noUpdateElapsed: noUpdateElapsed,
                    dt: dt,
                    isReturning: ref _isReturningX,
                    returnElapsed: ref _returnElapsedX,
                    returnStart: ref _returnStartX
                );

                // 初期位置に戻す（Z軸）
                _cameraPosition.z = ApplyReturnToInitial(
                    current: _cameraPosition.z,
                    target: InitialLocalPosition.z,
                    behavior: ZNoUpdateBehavior,
                    delay: ZReturnDelay,
                    duration: ZReturnDuration,
                    noUpdateElapsed: noUpdateElapsed,
                    dt: dt,
                    isReturning: ref _isReturningZ,
                    returnElapsed: ref _returnElapsedZ,
                    returnStart: ref _returnStartZ
                );

                // 初期位置を代入
                _cameraPosition.y = InitialLocalPosition.y;
            }
            // 前のフレームでOSCの更新があったかどうか
            _wasOscUpdating = isOscUpdating;

            // カメラの位置の移動範囲を制限（X軸）
            _cameraPosition.x = Mathf.Clamp(
                _cameraPosition.x,
                PositionRangeXMin,
                PositionRangeXMax
            );
            // カメラの位置の移動範囲を制限（Z軸）
            _cameraPosition.z = Mathf.Clamp(
                _cameraPosition.z,
                PositionRangeZMin,
                PositionRangeZMax
            );
            // カメラ位置
            _cameraRef.transform.localPosition = _cameraPosition;
        }

        /// <summary>
        /// カメラの位置を更新
        /// </summary>
        /// <param name="current">現在の値</param>
        /// <param name="input">重心の値</param>
        /// <param name="moveMode">移動モード</param>
        /// <param name="additiveSpeed">加算時のスピード</param>
        /// <param name="directScale">代入時のスケール</param>
        /// <param name="initialValue">初期値</param>
        /// <param name="dt">前フレームとの時間差分</param>
        /// <returns></returns>
        float ApplyInputMove(
            float current,
            float input,
            AxisMoveMode moveMode,
            float additiveSpeed,
            float directScale,
            float initialValue,
            float dt
        )
        {
            switch (moveMode)
            {
                // 加算（累積）
                case AxisMoveMode.Additive:
                    return current + input * additiveSpeed * dt;
                // 直接代入（スケーリングはする）
                case AxisMoveMode.DirectSet:
                    return initialValue + input * directScale;
                // 線形補間（ちょっとスムーズ）
                case AxisMoveMode.LerpToTarget:
                    return Mathf.Lerp(current, initialValue + input * directScale, dt * additiveSpeed);

                default:
                    return current;
            }
        }

        /// <summary>
        /// カメラを初期位置に戻す処理
        /// </summary>
        /// <param name="current">現在の値</param>
        /// <param name="target">目標とする値</param>
        /// <param name="behavior">値の更新がない場合の、戻り方</param>
        /// <param name="delay">遅延時間</param>
        /// <param name="duration">戻る時間尺</param>
        /// <param name="noUpdateElapsed">更新がなくなってから経過した時間</param>
        /// <param name="dt">前フレームとの時間差分</param>
        /// <param name="isReturning">戻り中かどうか</param>
        /// <param name="returnElapsed">戻り中処理の経過時間</param>
        /// <param name="returnStart">戻り処理の開始時の値</param>
        /// <returns></returns>
        float ApplyReturnToInitial(
            float current,
            float target,
            NoUpdateBehavior behavior,
            float delay,
            float duration,
            float noUpdateElapsed,
            float dt,
            ref bool isReturning,
            ref float returnElapsed,
            ref float returnStart
        )
        {   
            // そのままの位置に留まる
            if (behavior == NoUpdateBehavior.Hold)
            {
                isReturning = false;
                returnElapsed = 0.0f;
                return current;
            }

            float actualDelay = 0.0f;
            // 遅延時間のあと戻り処理を行う
            if (behavior == NoUpdateBehavior.ReturnAfterDelay)
            {
                actualDelay = delay;
            }
            
            if (noUpdateElapsed < actualDelay)
            {
                return current;
            }

            // ここは1回だけ通る
            if (!isReturning)
            {
                isReturning = true;
                returnElapsed = 0.0f;
                returnStart = current;
            }

            // 滑らかに移動する処理
            returnElapsed += dt;

            float safeDuration = Mathf.Max(0.0001f, duration);
            float t = Mathf.Clamp01(returnElapsed / safeDuration);

            float easedT = EaseInOutSmoothStep(t);

            float result = Mathf.LerpUnclamped(
                returnStart,
                target,
                easedT
            );

            if (t >= 1.0f)
            {
                result = target;
            }

            return result;
        }

        /// <summary>
        /// 補間
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        float EaseInOutSmoothStep(float t)
        {
            return t * t * (3.0f - 2.0f * t);
        }

        /// <summary>
        /// X軸戻りをキャンセル
        /// </summary>
        void CancelReturnX()
        {
            _isReturningX = false;
            _returnElapsedX = 0.0f;
        }

        /// <summary>
        /// Z軸戻りをキャンセル
        /// </summary>
        void CancelReturnZ()
        {
            _isReturningZ = false;
            _returnElapsedZ = 0.0f;
        }
    }
}