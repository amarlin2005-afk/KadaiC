using TMPro;
using UnityEngine;

[ExecuteAlways]
public class ColorByCameraZ : MonoBehaviour
{
    [Header("Reference Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Camera Z Range")]
    [SerializeField] private float cameraZAtColorA = -10f;
    [SerializeField] private float cameraZAtColorB = 10f;

    [Header("Color Range")]
    [SerializeField] private Color colorA = Color.white;
    [SerializeField] private Color colorB = new Color(1f, 1f, 1f, 0f);

    [Header("Targets")]
    [SerializeField] private bool controlTextMeshPro = true;
    [SerializeField] private TMP_Text targetText;

    [SerializeField] private bool controlRenderers = false;
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Options")]
    [SerializeField] private bool clamp01 = true;

    private MaterialPropertyBlock propertyBlock;

    private void Reset()
    {
        targetCamera = Camera.main;
        targetText = GetComponent<TMP_Text>();
        targetRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        float cameraZ = targetCamera.transform.position.z;

        float t = Mathf.InverseLerp(
            cameraZAtColorA,
            cameraZAtColorB,
            cameraZ
        );

        if (!clamp01)
        {
            float range = cameraZAtColorB - cameraZAtColorA;

            if (Mathf.Approximately(range, 0f))
            {
                t = 0f;
            }
            else
            {
                t = (cameraZ - cameraZAtColorA) / range;
            }
        }

        Color currentColor = Color.Lerp(colorA, colorB, t);

        if (controlTextMeshPro)
        {
            ApplyTextColor(currentColor);
        }

        if (controlRenderers)
        {
            ApplyRendererColor(currentColor);
        }
    }

    private void ApplyTextColor(Color color)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.color = color;
    }

    private void ApplyRendererColor(Color color)
    {
        if (targetRenderers == null)
        {
            return;
        }

        foreach (Renderer renderer in targetRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material sharedMaterial = renderer.sharedMaterial;

            if (sharedMaterial == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);

            if (sharedMaterial.HasProperty("_BaseColor"))
            {
                propertyBlock.SetColor("_BaseColor", color);
            }
            else if (sharedMaterial.HasProperty("_Color"))
            {
                propertyBlock.SetColor("_Color", color);
            }

            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}