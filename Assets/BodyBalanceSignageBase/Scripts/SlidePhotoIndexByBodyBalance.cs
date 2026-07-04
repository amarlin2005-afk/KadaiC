using UnityEngine;

namespace BodyBalanceSignage
{
    /// <summary>
    /// 身体の傾きで、連番写真の表示するインデックスを操作する
    /// </summary>
    public class SlidePhotoIndexByBodyBalance : MonoBehaviour
    {
        [SerializeField]
        PhotoManager _photoManager;

        [SerializeField]
        BodyBalanceManager _bodyBalanceManager;

        public float SlideSpeed = 1.0f;

        float _indexValue = 0.0f;

        void Update()
        {
            if (_photoManager != null && _bodyBalanceManager != null)
            {
                _indexValue += Time.deltaTime * SlideSpeed * _bodyBalanceManager.CenterOfGravity.x;
            }
            _photoManager.IndexValue = _indexValue;
        }
    }
}