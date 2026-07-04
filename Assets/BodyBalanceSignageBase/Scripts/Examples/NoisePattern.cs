using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Unity.Rendering.Universal;

[ExecuteAlways]
public class NoisePattern : MonoBehaviour
{
    #region Enums
    public enum GradientOrTexture
    {
        Gradient,
        Texture,
    };

    public enum CoordinateType
    {
        UV,
        Position,
        WorldPosition,
        ScreenPosition,
    };
    #endregion

    #region References
    [SerializeField]
    Shader _fbmShader = default;
    #endregion

    #region Public Parameters
    public GradientOrTexture gradientOrTexture = GradientOrTexture.Gradient;

    public Gradient  ColorGradient = default;
    public Texture2D ColorTexture  = default;

    public CoordinateType InputGeometryType = CoordinateType.Position;
    public Vector3 NoiseScale = Vector3.one;
    public Vector3 NoiseSpeed = Vector3.zero;

    [Range(1, 8)]
    public int Octaves = 1;
    #endregion

    #region Private Variables and Resources
    Vector3 _noiseTranslate = Vector3.zero;

    Texture2D _colorGradientTex = null;

    const int GRADIENT_TEX_WIDTH = 128;

    Material _mat = default;
    #endregion

    void Update()
    {
        CreateResources();
    
        var dt = Time.deltaTime;
        _noiseTranslate.x += NoiseSpeed.x * dt;
        _noiseTranslate.y += NoiseSpeed.y * dt;
        _noiseTranslate.z += NoiseSpeed.z * dt;

        // Set Property to Shader
        if (_mat != null)
        {         
            // Geometry Type
            if (InputGeometryType == CoordinateType.UV)
            {
                //_mat.DisableKeyword("_GEOMETRYTYPE_UV");
                _mat.DisableKeyword("_GEOMETRYTYPE_POSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_WORLDPOSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_SCREENPOSITION");
                _mat.EnableKeyword("_GEOMETRYTYPE_UV");                
            }
            else if (InputGeometryType == CoordinateType.Position)
            {
                _mat.DisableKeyword("_GEOMETRYTYPE_UV");
                //_mat.DisableKeyword("_GEOMETRYTYPE_POSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_WORLDPOSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_SCREENPOSITION");
                _mat.EnableKeyword("_GEOMETRYTYPE_POSITION");
            }
            else if (InputGeometryType == CoordinateType.WorldPosition)
            {
                _mat.DisableKeyword("_GEOMETRYTYPE_UV");
                _mat.DisableKeyword("_GEOMETRYTYPE_POSITION");
                //_mat.DisableKeyword("_GEOMETRYTYPE_WORLDPOSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_SCREENPOSITION");
                _mat.EnableKeyword("_GEOMETRYTYPE_WORLDPOSITION");
            }
            else if (InputGeometryType == CoordinateType.ScreenPosition)
            {
                _mat.DisableKeyword("_GEOMETRYTYPE_UV");
                _mat.DisableKeyword("_GEOMETRYTYPE_POSITION");
                _mat.DisableKeyword("_GEOMETRYTYPE_WORLDPOSITION");
                //_mat.DisableKeyword("_GEOMETRYTYPE_SCREENPOSITION");
                _mat.EnableKeyword("_GEOMETRYTYPE_SCREENPOSITION");
            }

            if (gradientOrTexture == GradientOrTexture.Gradient)
            {
                // グラデーションをテクスチャに転写する
                if (_colorGradientTex != null)
                {
                    var cols = new Color[GRADIENT_TEX_WIDTH];
                    for (var i = 0; i < GRADIENT_TEX_WIDTH; i++)
                    {
                        var t = i / (float)GRADIENT_TEX_WIDTH;
                        cols[i] = ColorGradient.Evaluate(t);
                    }
                    _colorGradientTex.SetPixels(cols);
                    _colorGradientTex.Apply();
                }
                _mat.SetTexture("_Gradient", _colorGradientTex);
            }
            else if (gradientOrTexture == GradientOrTexture.Texture)
            {
                if (ColorTexture != null)
                {
                    _mat.SetTexture("_Gradient", ColorTexture);
                }
            }

            _mat.SetInt("_Octaves", Octaves);

            _mat.SetVector("_NoiseScale", NoiseScale);
            _mat.SetVector("_NoisePosition", _noiseTranslate);
        }
    }

    void OnEnable()
    {
        CreateResources();
    }

    void OnDisable()
    {
        DeleteResources();
    }

    void OnDestroy()
    {
        DeleteResources();
    }

    void CreateResources()
    {
        if (_colorGradientTex == null)
        {
            _colorGradientTex = new Texture2D(GRADIENT_TEX_WIDTH, 1, TextureFormat.ARGB32, false);
            _colorGradientTex.hideFlags  = HideFlags.HideAndDontSave;
            _colorGradientTex.wrapMode   = TextureWrapMode.Clamp;
            _colorGradientTex.filterMode = FilterMode.Bilinear;
        }

        if (_mat == null)
        {
            if (_fbmShader != null)
            {
                _mat = new Material(_fbmShader);
                _mat.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        if (_mat != null)
        {
            if (GetComponent<Renderer>() != null)
            {
                GetComponent<Renderer>().material = _mat;
            }
        }   
    }

    void DeleteResources()
    {
        if (_colorGradientTex != null)
        {
            if (Application.isEditor)
                Texture2D.DestroyImmediate(_colorGradientTex);
            else
                Texture2D.Destroy(_colorGradientTex);
            _colorGradientTex = null;
        }
    }
}