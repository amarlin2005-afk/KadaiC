using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BodyBalanceSignage
{
    [ExecuteAlways]
    public class LineRendererGrid : MonoBehaviour
    {
        [Header("Grid")]
        [Min(1)]
        public int gridCount = 20;

        [Min(0.01f)]
        public float cellSize = 1.0f;

        public float y = 0.0f;

        [Header("Line")]
        [Min(0.001f)]
        public float lineWidth = 0.02f;

        public Color lineColor = new Color(1f, 1f, 1f, 0.35f);
        public Color centerLineColor = new Color(1f, 1f, 1f, 0.8f);

        [Header("Rendering")]
        public bool useWorldSpace = false;
        public bool castShadows = false;
        public bool receiveShadows = false;

        [Header("Auto Update")]
        public bool autoRebuild = true;

        private const string LineObjectPrefix = "GridLine_";

        private Material lineMaterial;
        private bool needsRebuild;

        private void OnEnable()
        {
            RequestRebuild();
        }

        private void OnValidate()
        {
            // OnValidate内では、削除・生成・SetParentを絶対にしない
            if (!autoRebuild) return;

            RequestRebuild();
        }

        private void Update()
        {
            if (!needsRebuild) return;

            needsRebuild = false;
            RebuildGridNow();
        }

        [ContextMenu("Rebuild Grid")]
        public void RebuildGrid()
        {
            RequestRebuild();
        }

        private void RequestRebuild()
        {
            needsRebuild = true;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        private void RebuildGridNow()
        {
            if (!isActiveAndEnabled) return;

            ClearGrid();
            CreateMaterial();

            float halfSize = gridCount * cellSize * 0.5f;

            int index = 0;

            for (int i = 0; i <= gridCount; i++)
            {
                float p = -halfSize + i * cellSize;

                bool isCenterLine = Mathf.Approximately(p, 0.0f);
                Color color = isCenterLine ? centerLineColor : lineColor;

                // X方向の線
                CreateLine(
                    index++,
                    new Vector3(-halfSize, y, p),
                    new Vector3(halfSize, y, p),
                    color
                );

                // Z方向の線
                CreateLine(
                    index++,
                    new Vector3(p, y, -halfSize),
                    new Vector3(p, y, halfSize),
                    color
                );
            }
        }

        private void CreateLine(int index, Vector3 start, Vector3 end, Color color)
        {
            GameObject lineObject = new GameObject(LineObjectPrefix + index.ToString("000"));
            lineObject.transform.SetParent(transform, false);

            LineRenderer lr = lineObject.AddComponent<LineRenderer>();

            lr.useWorldSpace = useWorldSpace;
            lr.positionCount = 2;

            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;

            lr.startColor = color;
            lr.endColor = color;

            lr.material = lineMaterial;

            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;

            lr.shadowCastingMode = castShadows
                ? ShadowCastingMode.On
                : ShadowCastingMode.Off;

            lr.receiveShadows = receiveShadows;

            lr.sortingOrder = 0;
        }

        private void CreateMaterial()
        {
            if (lineMaterial != null) return;

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            lineMaterial = new Material(shader);
            lineMaterial.name = "LineRendererGridMaterial";
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;

            if (lineMaterial.HasProperty("_Color"))
            {
                lineMaterial.SetColor("_Color", Color.white);
            }
        }

        private void ClearGrid()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (!child.name.StartsWith(LineObjectPrefix)) continue;

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
#if UNITY_EDITOR
                    DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
                }
            }
        }
    }
}