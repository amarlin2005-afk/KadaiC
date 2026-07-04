using BodyBalanceSignage;
using UnityEngine;

namespace BodyBalanceSignage
{
    public class RotationByBodyBalance : MonoBehaviour
    {
        [SerializeField]
        BodyBalanceManager _bodyBalanceManager;

        public float RotationSpeed = 1.0f;
        public float RotationOffset = 0.0f;

        float _rotationValue = 0.0f;

        void Update()
        {
            if (_bodyBalanceManager != null)
            {
                _rotationValue += Time.deltaTime * RotationSpeed * _bodyBalanceManager.CenterOfGravity.x;
            }
            transform.localEulerAngles = new Vector3(0.0f, RotationOffset + _rotationValue, 0.0f);
        }
    }
}