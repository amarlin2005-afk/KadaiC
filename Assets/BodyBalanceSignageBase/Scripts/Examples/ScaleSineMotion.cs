using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

public class ScaleSineMotion : MonoBehaviour
{

    public float BaseScale  = 2.0f;
    public float Amplitude  = 0.5f;
    public float Speed      = 0.1f;
    public float Phase      = 0.0f;

    [SerializeField, ReadOnly]
    float _timer = 0.0f;

    void Start()
    {
        _timer = 0.0f;
    }

    void Update()
    {
        _timer += Time.deltaTime * Speed;
        var scale = BaseScale + Amplitude * sin(_timer + Phase);

        transform.localScale = Vector3.one * scale;
    }
}
