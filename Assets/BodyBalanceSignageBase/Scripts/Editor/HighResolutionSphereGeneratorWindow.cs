using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class HighResolutionSphereGeneratorWindow : EditorWindow
{
    private enum NormalDirection
    {
        Outside,
        Inside
    }

    private int uSegments = 128;
    private int vSegments = 64;
    private float radius = 1.0f;

    private NormalDirection normalDirection = NormalDirection.Outside;

    private bool createNewGameObject = true;
    private bool assignToSelectedObject = false;
    private bool saveMeshAsAsset = true;

    private string objectName = "High Resolution Sphere";
    private string assetPath = "Assets/GeneratedSphere.asset";

    [MenuItem("Window/Custom/High Resolution Sphere Generator")]
    public static void Open()
    {
        GetWindow<HighResolutionSphereGeneratorWindow>("Sphere Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("High Resolution Sphere Generator", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        uSegments = EditorGUILayout.IntField("U Segments", uSegments);
        vSegments = EditorGUILayout.IntField("V Segments", vSegments);
        radius = EditorGUILayout.FloatField("Radius", radius);

        uSegments = Mathf.Max(3, uSegments);
        vSegments = Mathf.Max(2, vSegments);
        radius = Mathf.Max(0.001f, radius);

        EditorGUILayout.Space();

        normalDirection = (NormalDirection)EditorGUILayout.EnumPopup(
            "Normal Direction",
            normalDirection
        );

        EditorGUILayout.Space();

        createNewGameObject = EditorGUILayout.Toggle("Create New GameObject", createNewGameObject);
        assignToSelectedObject = EditorGUILayout.Toggle("Assign To Selected Object", assignToSelectedObject);
        saveMeshAsAsset = EditorGUILayout.Toggle("Save Mesh As Asset", saveMeshAsAsset);

        EditorGUILayout.Space();

        objectName = EditorGUILayout.TextField("Object Name", objectName);

        if (saveMeshAsAsset)
        {
            EditorGUILayout.BeginHorizontal();

            assetPath = EditorGUILayout.TextField("Asset Path", assetPath);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "Save Sphere Mesh",
                    "GeneratedSphere",
                    "asset",
                    "Save generated sphere mesh asset"
                );

                if (!string.IsNullOrEmpty(path))
                {
                    assetPath = path;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        int vertexCount = (uSegments + 1) * (vSegments + 1);
        int triangleCount = uSegments * (vSegments - 1) * 2;

        EditorGUILayout.HelpBox(
            $"Vertices: {vertexCount}\nTriangles: {triangleCount}",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Sphere", GUILayout.Height(32)))
        {
            GenerateSphere();
        }
    }

    private void GenerateSphere()
    {
        Mesh mesh = CreateSphereMesh(
            uSegments,
            vSegments,
            radius,
            normalDirection == NormalDirection.Outside
        );

        mesh.name = objectName + "_Mesh";

        if (saveMeshAsAsset)
        {
            SaveMeshAsset(mesh);
        }

        if (assignToSelectedObject && Selection.activeGameObject != null)
        {
            AssignMeshToObject(Selection.activeGameObject, mesh);
        }

        if (createNewGameObject)
        {
            GameObject sphereObject = new GameObject(objectName);

            MeshFilter meshFilter = sphereObject.AddComponent<MeshFilter>();
            sphereObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = mesh;

            Selection.activeGameObject = sphereObject;

            Undo.RegisterCreatedObjectUndo(sphereObject, "Create High Resolution Sphere");
        }

        Debug.Log(
            $"Generated Sphere Mesh: U={uSegments}, V={vSegments}, Radius={radius}, Normal={normalDirection}"
        );
    }

    private void AssignMeshToObject(GameObject targetObject, Mesh mesh)
    {
        MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            meshFilter = targetObject.AddComponent<MeshFilter>();
        }

        MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            targetObject.AddComponent<MeshRenderer>();
        }

        Undo.RecordObject(meshFilter, "Assign Sphere Mesh");
        meshFilter.sharedMesh = mesh;

        EditorUtility.SetDirty(meshFilter);
    }

    private void SaveMeshAsset(Mesh mesh)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            assetPath = "Assets/GeneratedSphere.asset";
        }

        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(mesh, uniquePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Saved Mesh Asset: {uniquePath}");
    }

    private static Mesh CreateSphereMesh(
        int uSegments,
        int vSegments,
        float radius,
        bool outsideNormal
    )
    {
        uSegments = Mathf.Max(3, uSegments);
        vSegments = Mathf.Max(2, vSegments);
        radius = Mathf.Max(0.001f, radius);

        int vertexCount = (uSegments + 1) * (vSegments + 1);

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        int index = 0;

        for (int v = 0; v <= vSegments; v++)
        {
            float vRate = (float)v / vSegments;
            float theta = vRate * Mathf.PI;

            float sinTheta = Mathf.Sin(theta);
            float cosTheta = Mathf.Cos(theta);

            for (int u = 0; u <= uSegments; u++)
            {
                float uRate = (float)u / uSegments;
                float phi = uRate * Mathf.PI * 2.0f;

                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);

                Vector3 normal = new Vector3(
                    sinTheta * cosPhi,
                    cosTheta,
                    sinTheta * sinPhi
                ).normalized;

                vertices[index] = normal * radius;

                if (outsideNormal)
                {
                    normals[index] = normal;
                }
                else
                {
                    normals[index] = -normal;
                }

                uvs[index] = new Vector2(uRate, 1.0f - vRate);

                index++;
            }
        }

        List<int> triangles = new List<int>();

        for (int v = 0; v < vSegments; v++)
        {
            for (int u = 0; u < uSegments; u++)
            {
                int a = v * (uSegments + 1) + u;
                int b = a + 1;
                int c = a + (uSegments + 1);
                int d = c + 1;

                if (outsideNormal)
                {
                    // 外側から見える向き
                    if (v != 0)
                    {
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(c);
                    }

                    if (v != vSegments - 1)
                    {
                        triangles.Add(b);
                        triangles.Add(d);
                        triangles.Add(c);
                    }
                }
                else
                {
                    // 内側から見える向き
                    if (v != 0)
                    {
                        triangles.Add(a);
                        triangles.Add(c);
                        triangles.Add(b);
                    }

                    if (v != vSegments - 1)
                    {
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(d);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();

        if (vertexCount > 65535)
        {
            mesh.indexFormat = IndexFormat.UInt32;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();

        return mesh;
    }
}