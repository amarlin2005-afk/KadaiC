using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace BodyBalanceSignage
{
    public class NoisyMoveUpAndDownByBodyBalance : MonoBehaviour
    {
        /// <summary>
        /// BodyBalanceManagerスクリプトの参照
        /// </summary>
        [SerializeField]
        BodyBalanceManager _bodyBalanceManager;

        // 中心位置
        public float CenterPositionY = 0.0f;
        // 移動距離
        public float MoveDistance = 2.0f;
        // 移動スピード
        public float MoveSpeed = 0.2f;
        // 左右バランスの累積量
        float _bodyBalanceXValue = 0.0f;

        /// <summary>
        /// 毎フレーム実行される関数
        /// </summary>
        void Update()
        {
            // InteractionManagerがアタッチされていれば
            if (_bodyBalanceManager != null)
            {
                // 加算
                _bodyBalanceXValue += Time.deltaTime * _bodyBalanceManager.CenterOfGravity.x;
            }

            // ローカル座標における位置の値を取得
            Vector3 position = transform.localPosition;

            // ローカル座標における位置の値を代入
            transform.localPosition = new Vector3
            (
                position.x,
                // 上下の動き
                CenterPositionY + MoveDistance * noise.cnoise(float2(_bodyBalanceXValue * MoveSpeed, 0.0f)),
                position.z
            );
        }
    }
}