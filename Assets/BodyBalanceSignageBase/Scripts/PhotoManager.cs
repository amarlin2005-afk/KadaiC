using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace BodyBalanceSignage
{
    /// <summary>
    /// 連番写真管理
    /// </summary>
    public class PhotoManager : MonoBehaviour
    {
        /// <summary>
        /// テクスチャ配列
        /// </summary>
        [SerializeField]
        Texture2D[] _textureArray;
        
        /// <summary>
        /// Renderer
        /// </summary>
        [SerializeField]
        Renderer _renderer = null;

        /// <summary>
        /// マテリアルプロパティブロック
        /// </summary>
        MaterialPropertyBlock _mpb = null;

        /// <summary>
        /// インデックス
        /// </summary>
        public float IndexValue = 0.0f;

        /// <summary>
        /// インデックスのオフセット
        /// </summary>
        public float IndexValueOffset = 0.0f;

        /// <summary>
        /// 画像のプロパティ名
        /// </summary>
        [SerializeField]
        string _BaseMapPropertyName = "_BaseMap";

        void Awake()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            _mpb = new MaterialPropertyBlock();
        }

        void Start()
        {
            // _textureArray = Resources.LoadAll<Texture2D>(ImagePath);

            foreach (var tex in _textureArray)
            {
                Debug.Log("Loaded --- " + tex.name);
            }
        }

        int _texIdx = 0;

        void Update()
        {
            // 回転量から、画像のインデックス（0.0～1.0）に変換
            var imgIdxNormalized = ((IndexValue + IndexValueOffset) % 360.0f) / 360.0f;

            if (imgIdxNormalized < 0.0f)
            {
                imgIdxNormalized = 1.0f - abs(imgIdxNormalized);
            }
            // 画像のインデックス（0.0～1.0）から画像のインデックス（整数）に変換
            _texIdx = Mathf.FloorToInt(imgIdxNormalized * _textureArray.Length);

            // レンダラーがセットされていれば、テクスチャをセット
            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_mpb);

                if (_textureArray != null && _textureArray.Length > 0)
                {
                    _mpb.SetTexture(_BaseMapPropertyName, _textureArray[_texIdx]);
                }

                _renderer.SetPropertyBlock(_mpb);
            }
        }

        void OnDestroy()
        {
            if (_mpb != null)
            {
                _mpb.Clear();
                _mpb = null;
            }
        }
    }
}