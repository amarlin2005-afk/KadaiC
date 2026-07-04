using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace BodyBalanceSignage
{
    /// <summary>
    /// 
    /// </summary>
    public class ControlAnimTimeByBodyBalance : MonoBehaviour
    {
        public Animator _animator;

        [SerializeField]
        BodyBalanceManager _oscReceiver;

        public float PlayBackSpeed = 0.0f;

        float _motionTime = 0.0f;

        void Update()
        {
            if (_animator != null)
            {
                _motionTime += PlayBackSpeed * _oscReceiver.CenterOfGravity.x;
                _motionTime = _motionTime % 1.0f;
                if (_motionTime < 0.0f)
                {
                    _motionTime = 1.0f - abs(_motionTime);
                }

                _animator.SetFloat("MotionTime", _motionTime);
                
            }
        }
    }
}