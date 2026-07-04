using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BodyBalanceSignage
{
    public class MoveUpAndDownByBodyBalance : MonoBehaviour
    {
        /// <summary>
        /// BodyBalanceManagerスクリプトの参照
        /// </summary>
        public BodyBalanceManager _bodyBalanceManager;

        // 中心位置
        public float CenterPositionY = 0.0f;
        // 移動距離
        public float MoveDistance = 2.0f;
        // 移動スピード
        public float MoveSpeed = 5.0f;
        // 左右のバランスの累積量
        float _balanceXValue = 0.0f;

        /// <summary>
        /// 毎フレーム実行される関数
        /// </summary>
        void Update()
        {
            // InteractionManagerがアタッチされていれば
            if (_bodyBalanceManager != null)
            {
                // 加算
                _balanceXValue += Time.deltaTime * _bodyBalanceManager.CenterOfGravity.x;
            }

            // ローカル座標における位置の値を取得
            Vector3 position = transform.localPosition;
            
            // ローカル座標における位置の値を代入
            transform.localPosition = new Vector3
            (
                position.x,
                // 上下の動き
                CenterPositionY + MoveDistance * Mathf.Sin(_balanceXValue * MoveSpeed),
                position.z
            );
        }
    }
}