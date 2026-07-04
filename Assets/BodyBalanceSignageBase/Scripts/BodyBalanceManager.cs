using UnityEngine;
using OscJack;
using System.Collections.Concurrent;

namespace BodyBalanceSignage
{
    /// <summary>
    /// Balance Wii Boardを管理するアプリから重心（CenterOfGravity）の値が送られてくるので、それを受信するスクリプト
    /// </summary>
    public class BodyBalanceManager : MonoBehaviour
    {
        /// <summary>
        /// OSCポート
        /// </summary>
        [Header("OSC")]
        public int Port = 9000;
        /// <summary>
        /// OSCアドレス
        /// </summary>
        public string Address = "/centerOfGravity";

        /// <summary>
        /// デバッグの為にコンソールにメッセージを表示するかどうか
        /// </summary>
        [Header("Debug")]
        public bool ShowDebugLog = false;
        
        /// <summary>
        /// OSCサーバー（受信）
        /// </summary>
        OscServer _oscServer = default;

        /// <summary>
        /// OSCで送られてくるメッセージを格納するキュー
        /// </summary>
        private readonly ConcurrentQueue<Vector2> _queue = new ConcurrentQueue<Vector2>();

        /// <summary>
        /// 重心
        /// </summary>
        public Vector2 CenterOfGravity { get; private set; }

        /// <summary>
        /// 受信した値の数
        /// </summary>
        public int ReceiveCount { get; private set; }

        void Start()
        {
            _oscServer = new OscServer(Port);

            // メッセージ受信時の処理を登録する
            _oscServer.MessageDispatcher.AddCallback(
                Address,
                (string address, OscDataHandle data) =>
                {
                    if (data.GetElementCount() < 2)
                    {
                        return;
                    }

                    float cx = data.GetElementAsFloat(0);
                    float cy = data.GetElementAsFloat(1);

                    // キューに追加
                    AddQueue(new Vector2(cx, cy));
                }
            );
        }

        /// <summary>
        /// 毎フレーム行う処理
        /// </summary>
        void Update()
        {
            ProcessLatestOnly();
        }

        /// <summary>
        /// このGameObjectが削除されたときに行う処理
        /// </summary>
        void OnDestroy()
        {
            // OSCサーバーを適切に削除
            if (_oscServer != null)
            {
                _oscServer.Dispose();
                _oscServer = null;
            }
        }

        /// <summary>
        /// 送られてきた中で、一番新しい値のみを取り出す処理
        /// </summary>
        void ProcessLatestOnly()
        {
            bool hasValue = false;
            Vector2 latest = Vector2.zero;

            // キューにある値を処理し、最新のものを取り出す
            while (_queue.TryDequeue(out Vector2 value))
            {
                latest = value;
                hasValue = true;
            }
            // 値がなければ何も処理しない
            if (!hasValue)
            {
                return;
            }
            // 重心の値を代入
            CenterOfGravity = latest;

            // デバッグとして、コンソールに値を表示する
            if (ShowDebugLog)
            {
                Debug.Log($"{CenterOfGravity.x:0.000}, {CenterOfGravity.y:0.000}");
            }
        }

        /// <summary>
        /// 外部スクリプトから、デバッグの為にキューを追加する
        /// ※マウスエミュレータで使用
        /// </summary>
        /// <param name="centerOfGravity"></param>
        public void AddQueue(Vector2 centerOfGravity)
        {
            _queue.Enqueue(centerOfGravity);
            ReceiveCount++;
        }
    }
}