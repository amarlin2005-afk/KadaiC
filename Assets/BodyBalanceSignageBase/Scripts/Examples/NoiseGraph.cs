using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Mathematics;
using static Unity.Mathematics.math;

/*
 * https://docs.unity.cn/Packages/com.unity.mathematics@1.2/api/Unity.Mathematics.noise.html
noise.cellular(float2(0, 0));
noise.cellular2x2(float2(0, 0));
noise.cellular2x2x2(float3(0, 0, 0));
noise.cnoise(float2(0, 0));
noise.cnoise(float3(0, 0, 0));
noise.cnoise(float4(0, 0, 0, 0));
noise.pnoise(float2(0, 0), float2(0, 0));
noise.pnoise(float3(0, 0, 0), float3(0, 0, 0));
noise.pnoise(float4(0, 0, 0, 0), float4(0, 0, 0, 0));
noise.psrdnoise(float2(0, 0), float2(0, 0));
noise.psrdnoise(float2(0, 0), float2(0, 0), 0);
noise.psrnoise(float2(0, 0), float2(0, 0));
noise.psrnoise(float2(0, 0), float2(0, 0), 0);
noise.snoise(float2(0, 0));
noise.snoise(float3(0, 0, 0));
noise.snoise(float4(0, 0, 0, 0));
float3 gradient;
noise.snoise(float3(0, 0, 0), out gradient);
noise.srdnoise(float2(0, 0));
noise.srdnoise(float2(0, 0), 0);
// 2-D non-tiling simplex noise with fixed gradients, without the analytical derivative.
noise.srnoise(float2(0, 0));
// 2-D non-tiling simplex noise with rotating gradients, without the analytical derivative.
noise.srnoise(float2(0, 0), 0);                    
*/

/// <summary>
/// 
/// 
/// 
/// 
/// 
/// </summary>
public class NoiseGraph : MonoBehaviour
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

    float[] _PerlinNoiseArr = new float[ArrLength];
    float[] _cellularArr  = new float[ArrLength];
    float[] _cnoiseArr    = new float[ArrLength];
    float[] _pnoiseArr    = new float[ArrLength];
    float[] _psrdnoiseArr = new float[ArrLength];
    float[] _psrnoiseArr  = new float[ArrLength];
    float[] _snoiseArr    = new float[ArrLength];
    float[] _srdnoiseArr  = new float[ArrLength];
    float[] _srnoiseArr   = new float[ArrLength];

    [SerializeField, ReadOnly]
    float _noiseTimer = 0.0f;


    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    void Update()
    {
        // ノイズの値を更新
        UpdateArrayValues(ref _PerlinNoiseArr, Mathf.PerlinNoise(0, _noiseTimer));
        UpdateArrayValues(ref _cellularArr,  noise.cellular (float2(0, _noiseTimer)).x);
        UpdateArrayValues(ref _cnoiseArr,    noise.cnoise   (float2(0, _noiseTimer)));
        UpdateArrayValues(ref _pnoiseArr,    noise.pnoise   (float2(0, _noiseTimer), PeriodOfRepetition));
        UpdateArrayValues(ref _psrdnoiseArr, noise.psrdnoise(float2(0, _noiseTimer), PeriodOfRepetition).x);
        UpdateArrayValues(ref _psrnoiseArr,  noise.psrnoise (float2(0, _noiseTimer), PeriodOfRepetition));            
        UpdateArrayValues(ref _snoiseArr,    noise.snoise   (float2(0, _noiseTimer)));
        UpdateArrayValues(ref _srdnoiseArr,  noise.srdnoise (float2(0, _noiseTimer)).x);
        UpdateArrayValues(ref _srnoiseArr,   noise.srnoise  (float2(0, _noiseTimer)));

        // LineRendererのPositionを更新
        UpdateLineRendererPositions(_lineRenderer0, _PerlinNoiseArr, LineXMinMax.x, LineXMinMax.y,   0.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer1, _cellularArr,    LineXMinMax.x, LineXMinMax.y,  -2.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer2, _cnoiseArr,      LineXMinMax.x, LineXMinMax.y,  -4.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer3, _pnoiseArr,      LineXMinMax.x, LineXMinMax.y,  -6.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer4, _psrdnoiseArr,   LineXMinMax.x, LineXMinMax.y,  -8.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer5, _psrnoiseArr,    LineXMinMax.x, LineXMinMax.y, -10.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer6, _snoiseArr,      LineXMinMax.x, LineXMinMax.y, -12.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer7, _srdnoiseArr,    LineXMinMax.x, LineXMinMax.y, -14.0f, 1.0f, 0.0f);
        UpdateLineRendererPositions(_lineRenderer8, _srnoiseArr,     LineXMinMax.x, LineXMinMax.y, -16.0f, 1.0f, 0.0f);

        // テキスト代入
        if (_textMesh0 != null) _textMesh0.text = _PerlinNoiseArr[0].ToString("0.0000");
        if (_textMesh1 != null) _textMesh1.text = _cellularArr[0].ToString("0.0000");
        if (_textMesh2 != null) _textMesh2.text = _cnoiseArr[0].ToString("0.0000");
        if (_textMesh3 != null) _textMesh3.text = _pnoiseArr[0].ToString("0.0000");
        if (_textMesh4 != null) _textMesh4.text = _psrdnoiseArr[0].ToString("0.0000");
        if (_textMesh5 != null) _textMesh5.text = _psrnoiseArr[0].ToString("0.0000");
        if (_textMesh6 != null) _textMesh6.text = _snoiseArr[0].ToString("0.0000");
        if (_textMesh7 != null) _textMesh7.text = _srdnoiseArr[0].ToString("0.0000");
        if (_textMesh8 != null) _textMesh8.text = _srnoiseArr[0].ToString("0.0000");


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
            lr.endColor   = Color.gray;
        }
    }
}