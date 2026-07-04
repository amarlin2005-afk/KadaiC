using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

[ExecuteAlways]
public class ProceduralMotion : MonoBehaviour
{
    /// <summary>
    /// パターン
    /// </summary>
    public enum Pattern
    {
        Sine = 0,
        Cosine = 1,
        Triangle = 2,
        Ramp = 3,
        Square = 4,
        Pulse = 5,
        Random = 6,
        ClassicPerlinNoise = 7,
        SimplexNoise = 8,
    };

    /// <summary>
    /// Transformのパターン
    /// </summary>
    public enum TransformPattern
    {
        PositionX = 0,
        PositionY = 1,
        PositionZ = 2,
        RotateX = 3,
        RotateY = 4,
        RotateZ = 5,
        ScaleX = 6,
        ScaleY = 7,
        ScaleZ = 8,
        ScaleXYZ = 9,
    };

    /// <summary>
    /// ローカル座標かワールド座標か
    /// </summary>
    public enum LocalOrGlobal
    {
        Local  = 0,
        Global = 1,
    };

    /// <summary>
    /// パターン
    /// </summary>
    public Pattern _motionPattern = Pattern.Sine;

    /// <summary>
    /// 値の振幅
    /// </summary>
    public float Amplitude      = 1.0f;
    /// <summary>
    /// 値のオフセット量
    /// </summary>
    public float Offset         = 0.0f;
    /// <summary>
    /// サイクル数
    /// </summary>
    public float NumberOfCycles = 2.0f;
    /// <summary>
    /// 時間による変化の速さ
    /// </summary>
    public float Speed          = 1.0f;
    /// <summary>
    /// 時間の位相
    /// </summary>
    public float Phase     = 0.0f;
    /// <summary>
    /// バイアス
    /// </summary>
    [Range(-0.5f, 0.5f)]
    public float Bias      = 0.0f;

    /// <summary>
    /// Transform適用時、ローカル座標で指定するのかワールド座標で指定するのか
    /// </summary>
    [Space]
    public LocalOrGlobal _localOrGlobal = LocalOrGlobal.Local;
    /// <summary>
    /// Transformの種類
    /// </summary>
    public TransformPattern _transformPattern = TransformPattern.PositionX;

    /// <summary>
    /// 値
    /// </summary>
    [Header("Private Variables for Debug")]
    [SerializeField, ReadOnly]
    float _value = 0.0f;

    /// <summary>
    /// 時間
    /// </summary>
    [SerializeField, ReadOnly]
    float _timer = 0.0f;

    void Start()
    {
        _timer = 0.0f;
    }

    void Update()
    {
        switch (_motionPattern)
        {
            case Pattern.Sine:
                _value = Mathf.Sin(_timer + Phase);
                break;

            case Pattern.Cosine:
                _value = Mathf.Cos(_timer + Phase);
                break;

            case Pattern.Triangle:

                if (NumberOfCycles <= 0.0f)
                {
                    _value = 0.0f;
                }
                else
                {                    
                    var t = (_timer * NumberOfCycles + Phase) % 1.0f;

                    if (t < 0)
                        t = 1.0f - Mathf.Abs(t);

                    _value = t < 0.5f ? t : 1.0f - t;
                    //_value = 4.0f * _value - 1.0f;
                    _value = 2.0f * _value;
                }
               
                break;

            case Pattern.Ramp:

                if (NumberOfCycles <= 0.0f)
                {
                    _value = 0.0f;
                }
                else
                {
                    var t = (_timer * NumberOfCycles + Phase) % 1.0f;

                    if (t < 0)
                        t = 1.0f - Mathf.Abs(t);

                    _value = t;
                }

                break;

            case Pattern.Square:

                if (NumberOfCycles <= 0.0f)
                {
                    _value = 0.0f;
                }
                else
                {
                    var t = (_timer * NumberOfCycles + Phase) % 1.0f;

                    if (t < 0)
                        t = 1.0f - Mathf.Abs(t);

                    _value = t < 0.5f + Bias ? 1.0f : 0.0f;
                }

                break;

            case Pattern.Pulse:

                if (NumberOfCycles <= 0.0f)
                {
                    _value = 0.0f;
                }
                else
                {
                    var t = (_timer * NumberOfCycles + Phase) % 1.0f;

                    if (t < 0)
                        t = 1.0f - Mathf.Abs(t);

                    _value = t < Time.deltaTime * NumberOfCycles ? 1.0f : 0.0f;
                }

                break;

            case Pattern.Random:

                _value = UnityEngine.Random.value;

                break;

            case Pattern.ClassicPerlinNoise:

                _value = cnoise(float2(0, _timer + Phase));

                break;

            case Pattern.SimplexNoise:

                _value = snoise(float2(0, _timer + Phase));

                break;

            default:

                break;
        }

        // 値の代入
        _value = Amplitude * _value + Offset;

        // Transformのセット
        // ローカル
        if (_localOrGlobal == LocalOrGlobal.Local)
        {
            if (_transformPattern == TransformPattern.PositionX)
            {
                var p = transform.localPosition;
                transform.localPosition = new Vector3(_value, p.y, p.z);
            }
            else if (_transformPattern == TransformPattern.PositionY)
            {
                var p = transform.localPosition;
                transform.localPosition = new Vector3(p.x, _value, p.z);
            }
            else if (_transformPattern == TransformPattern.PositionZ)
            {
                var p = transform.localPosition;
                transform.localPosition = new Vector3(p.x, p.y, _value);
            }
            else if (_transformPattern == TransformPattern.RotateX)
            {
                var r = transform.localRotation;
                transform.localRotation = Quaternion.Euler(_value, r.y, r.z);
            }
            else if (_transformPattern == TransformPattern.RotateY)
            {
                var r = transform.localRotation;
                transform.localRotation = Quaternion.Euler(r.x, _value, r.z);
            }
            else if (_transformPattern == TransformPattern.RotateZ)
            {
                var r = transform.localRotation;
                transform.localRotation = Quaternion.Euler(r.x, r.y, _value);
            }
            else if (_transformPattern == TransformPattern.ScaleX)
            {
                var s = transform.localScale;
                transform.localScale = new Vector3(_value, s.y, s.z);
            }
            else if (_transformPattern == TransformPattern.ScaleY)
            {
                var s = transform.localScale;
                transform.localScale = new Vector3(s.x, _value, s.z);
            }
            else if (_transformPattern == TransformPattern.ScaleZ)
            {
                var s = transform.localScale;
                transform.localScale = new Vector3(s.x, s.y,_value);
            }
            else if (_transformPattern == TransformPattern.ScaleXYZ)
            {
                transform.localScale = new Vector3(_value, _value, _value);
            }
        }
        // ワールド
        else if (_localOrGlobal == LocalOrGlobal.Global)
        {
            if (_transformPattern == TransformPattern.PositionX)
            {
                var p = transform.position;
                transform.position = new Vector3(_value, p.y, p.z);
            }
            else if (_transformPattern == TransformPattern.PositionY)
            {
                var p = transform.position;
                transform.position = new Vector3(p.x, _value, p.z);
            }
            else if (_transformPattern == TransformPattern.PositionZ)
            {
                var p = transform.position;
                transform.position = new Vector3(p.x, p.y, _value);
            }
            else if (_transformPattern == TransformPattern.RotateX)
            {
                var r = transform.rotation;
                transform.rotation = Quaternion.Euler(_value, r.y, r.z);
            }
            else if (_transformPattern == TransformPattern.RotateY)
            {
                var r = transform.rotation;
                transform.rotation = Quaternion.Euler(r.x, _value, r.z);
            }
            else if (_transformPattern == TransformPattern.RotateZ)
            {
                var r = transform.rotation;
                transform.rotation = Quaternion.Euler(r.x, r.y, _value);
            }
            else if (_transformPattern == TransformPattern.ScaleX)
            {
                var s = transform.lossyScale;
                transform.localScale = new Vector3(_value, s.y, s.z);
            }
            else if (_transformPattern == TransformPattern.ScaleY)
            {
                var s = transform.lossyScale;
                transform.localScale = new Vector3(s.x, _value, s.z);
            }
            else if (_transformPattern == TransformPattern.ScaleZ)
            {
                var s = transform.lossyScale;
                transform.localScale = new Vector3(s.x, s.y, _value);
            }
            else if (_transformPattern == TransformPattern.ScaleXYZ)
            {
                transform.localScale = new Vector3(_value, _value, _value);
            }
        }

        // タイマー更新
        _timer += Time.deltaTime * Speed;
    }
}
