using UnityEngine;

[ExecuteAlways]
public class FitQuadToCameraView : MonoBehaviour
{
    [Header("参照")]
    [SerializeField]
    private Camera _cameraRef;

    [Header("配置")]
    [Tooltip("カメラからQuadまでの距離。Perspectiveの場合はnear clipより大きくしてください。")]
    public float Depth = 10.0f;

    [Tooltip("Quadをカメラの子として扱い、localPosition/localRotation/localScaleを設定する")]
    public bool UseLocalTransform = true;

    [Header("調整")]
    [Tooltip("画面より少し大きくしたい場合は 1.01 などにする")]
    public float ScaleMultiplier = 1.0f;

    [Tooltip("毎フレーム更新する。FOVや解像度が変わる場合はON")]
    public bool UpdateEveryFrame = true;

    private void Reset()
    {
        _cameraRef = GetComponentInParent<Camera>();
    }

    private void Awake()
    {
        if (_cameraRef == null)
        {
            _cameraRef = GetComponentInParent<Camera>();
        }

        ApplyScale();
    }

    private void LateUpdate()
    {
        if (!UpdateEveryFrame && Application.isPlaying)
        {
            return;
        }

        ApplyScale();
    }

    public void ApplyScale()
    {
        if (_cameraRef == null)
        {
            _cameraRef = GetComponentInParent<Camera>();
        }

        if (_cameraRef == null)
        {
            return;
        }

        float width;
        float height;

        if (_cameraRef.orthographic)
        {
            height = _cameraRef.orthographicSize * 2.0f;
            width = height * _cameraRef.aspect;
        }
        else
        {
            float safeDepth = Mathf.Max(Depth, _cameraRef.nearClipPlane + 0.001f);

            height = 2.0f * safeDepth * Mathf.Tan(_cameraRef.fieldOfView * 0.5f * Mathf.Deg2Rad);
            width = height * _cameraRef.aspect;
        }

        width *= ScaleMultiplier;
        height *= ScaleMultiplier;

        if (UseLocalTransform)
        {
            transform.localPosition = new Vector3(0.0f, 0.0f, Depth);
            transform.localRotation = Quaternion.identity;

            // Unity標準のQuadは 1 x 1 なので、そのまま幅・高さをscaleに入れる
            transform.localScale = new Vector3(width, height, 1.0f);
        }
        else
        {
            transform.position = _cameraRef.transform.position + _cameraRef.transform.forward * Depth;
            transform.rotation = _cameraRef.transform.rotation;
            transform.localScale = new Vector3(width, height, 1.0f);
        }
    }
}