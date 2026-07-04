using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using static Unity.Mathematics.math;


/// <summary>
/// 
/// 
/// 
/// 
/// 
/// </summary>
public class ProceduralMotionGraph : MonoBehaviour
{
    [Header("Parameters")]
    public float2 LineXMinMax = float2(0.0f, 0.0f);

    public float NoiseSpeed = 1.0f;

    public float2 PeriodOfRepetition = float2(1, 1);


    [Header("References")]
    [SerializeReference]
    LineRenderer _lineRenderer0;
    [SerializeReference]
    LineRenderer _lineRenderer1;
    [SerializeReference]
    LineRenderer _lineRenderer2;
    [SerializeReference]
    LineRenderer _lineRenderer3;
    [SerializeReference]
    LineRenderer _lineRenderer4;
    [SerializeReference]
    LineRenderer _lineRenderer5;
    [SerializeReference]
    LineRenderer _lineRenderer6;
    [SerializeReference]
    LineRenderer _lineRenderer7;
    [SerializeReference]
    LineRenderer _lineRenderer8;

    [Space]
    [SerializeReference]
    TextMeshPro _textMesh0 = default;
    [SerializeReference]
    TextMeshPro _textMesh1 = default;
    [SerializeReference]
    TextMeshPro _textMesh2 = default;
    [SerializeReference]
    TextMeshPro _textMesh3 = default;
    [SerializeReference]
    TextMeshPro _textMesh4 = default;
    [SerializeReference]
    TextMeshPro _textMesh5 = default;
    [SerializeReference]
    TextMeshPro _textMesh6 = default;
    [SerializeReference]
    TextMeshPro _textMesh7 = default;
    [SerializeReference]
    TextMeshPro _textMesh8 = default;

    [SerializeReference]
    Material _lineRenderMat = default;

    [Header("Private Variables")]
    const int ArrLength = 256;

    float[] _sineArr     = new float[ArrLength];
    float[] _cosineArr   = new float[ArrLength];
    float[] _triangleArr = new float[ArrLength];
    float[] _rampArr     = new float[ArrLength];
    float[] _squareArr   = new float[ArrLength];
    float[] _pulseArr    = new float[ArrLength];
    float[] _randomArr   = new float[ArrLength];
    float[] _cnoiseArr   = new float[ArrLength];
    float[] _snoiseArr   = new float[ArrLength];

    [SerializeField, ReadOnly]
    float _noiseTimer = 0.0f;


    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void Update()
    {

        var t_tri = (_noiseTimer) % 1.0f;

        if (t_tri < 0)
            t_tri = 1.0f - Mathf.Abs(t_tri);

        var triangle = t_tri < 0.5f ? t_tri : 1.0f - t_tri;
        triangle = 2.0f * triangle;

        var ramp = t_tri;

        var square = t_tri < 0.5f ? 1.0f : 0.0f;

        var pulse = t_tri < Time.deltaTime ? 1.0f : 0.0f;

        UpdateArrayValues(ref _sineArr, Mathf.Sin(_noiseTimer));
        UpdateArrayValues(ref _cosineArr, Mathf.Cos(_noiseTimer));
        UpdateArrayValues(ref _triangleArr, triangle);
        UpdateArrayValues(ref _rampArr, ramp);
        UpdateArrayValues(ref _squareArr, square);
        UpdateArrayValues(ref _pulseArr, pulse);
        UpdateArrayValues(ref _randomArr, UnityEngine.Random.value);
        UpdateArrayValues(ref _cnoiseArr, noise.cnoise(float2(0, _noiseTimer)));
        UpdateArrayValues(ref _snoiseArr, noise.snoise(float2(0, _noiseTimer)));

        // LineRendererのPositionを更新
        UpdateLineRendererPositions(_lineRenderer0, _sineArr, LineXMinMax.x, LineXMinMax.y, 0.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer1, _cosineArr, LineXMinMax.x, LineXMinMax.y, -2.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer2, _triangleArr, LineXMinMax.x, LineXMinMax.y, -4.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer3, _rampArr, LineXMinMax.x, LineXMinMax.y, -6.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer4, _squareArr, LineXMinMax.x, LineXMinMax.y, -8.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer5, _pulseArr, LineXMinMax.x, LineXMinMax.y, -10.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer6, _randomArr, LineXMinMax.x, LineXMinMax.y, -12.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer7, _cnoiseArr, LineXMinMax.x, LineXMinMax.y, -14.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer8, _snoiseArr, LineXMinMax.x, LineXMinMax.y, -16.0f, 1.0f, 0.0f);

        // テキスト代入
        if (_textMesh0 != null) _textMesh0.text = _sineArr[0].ToString("0.0000");
        if (_textMesh1 != null) _textMesh1.text = _cosineArr[0].ToString("0.0000");
        if (_textMesh2 != null) _textMesh2.text = _triangleArr[0].ToString("0.0000");
        if (_textMesh3 != null) _textMesh3.text = _rampArr[0].ToString("0.0000");
        if (_textMesh4 != null) _textMesh4.text = _squareArr[0].ToString("0.0000");
        if (_textMesh5 != null) _textMesh5.text = _pulseArr[0].ToString("0.0000");
        if (_textMesh6 != null) _textMesh6.text = _randomArr[0].ToString("0.0000");
        if (_textMesh7 != null) _textMesh7.text = _cnoiseArr[0].ToString("0.0000");
        if (_textMesh8 != null) _textMesh8.text = _snoiseArr[0].ToString("0.0000");


        // ノイズタイマーを更新
        _noiseTimer += Time.deltaTime * NoiseSpeed;
    }

    /// <summary>
    /// 配列の値を更新
    /// </summary>
    /// <param name="arr"></param>
    /// <param name="newValue"></param>
    void UpdateArrayValues(ref float[] arr, float newValue)
    {
        if (arr == null || arr.Length == 0)
            return;

        // 配列の値を右にシフトさせる
        ArrayShiftRight(ref arr);
        // 新しい値をセット
        arr[0] = newValue;
    }

    /// <summary>
    /// LineRendererのPositionを更新
    /// </summary>
    /// <param name="lr">LineRendererの参照</param>
    /// <param name="arr">配列</param>
    /// <param name="xmin">Position x の範囲（最小値）</param>
    /// <param name="xmax">Position x の範囲（最大値）</param>
    /// <param name="centerY">Position y の基準となる値</param>
    /// <param name="scaleY">Position y のスケール</param>
    /// <param name="centerZ">Position z の基準となる値</param>
    void UpdateLineRendererPositions(LineRenderer lr, float[] arr, float xmin, float xmax, float centerY, float scaleY, float centerZ)
    {
        if (lr == null)
            return;

        if (arr == null || arr.Length == 0)
            return;

        if (lr.positionCount != arr.Length)
            lr.positionCount = arr.Length;

        for (var i = 0; i < arr.Length; i++)
        {
            var x = xmin + abs(xmax - xmin) / (arr.Length - 1) * i;
            var y = centerY + scaleY * arr[i];
            var z = centerZ;
            lr.SetPosition(i, float3(x, y, z));
            lr.widthMultiplier = 0.05f;
            lr.startColor = Color.green;
            lr.endColor = Color.green;
        }
    }

    /// <summary>
    /// 配列の値を一つずつ右にシフトさせる
    /// </summary>
    /// <param name="arr"></param>
    void ArrayShiftRight(ref float[] arr)
    {
        var temp = arr[arr.Length - 1];
        System.Array.Copy(arr, 0, arr, 1, arr.Length - 1);
        arr[0] = temp;
    }

    [ContextMenu("CreateLineRendererForReference")]
    void CreateLineRendererForReference()
    {
        var lineNum = 9;

        var groupGo = new GameObject("ReferenceLineRenderers");
        groupGo.transform.parent = transform;
        groupGo.transform.localPosition = Vector3.zero;

        for (var i = 0; i < lineNum; i++)
        {
            var go = new GameObject();
            go.transform.parent = groupGo.transform;
            go.transform.localPosition = Vector3.zero;

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.positionCount = 2;
            if (_lineRenderMat != null)
                lr.material = _lineRenderMat;
            var poses = new Vector3[2];
            poses[0] = new Vector3(LineXMinMax.x, i * -2.0f, 0.0f);
            poses[1] = new Vector3(LineXMinMax.y, i * -2.0f, 0.0f);
            lr.SetPositions(poses);
            lr.widthMultiplier = 0.025f;
            lr.startColor = Color.gray;
            lr.endColor = Color.gray;
        }
    }
}