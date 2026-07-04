using UnityEngine;

[ExecuteAlways]
public class MoveByCameraZ : MonoBehaviour
{
    [Header("Reference Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Camera Z Range")]
    [SerializeField] private float cameraZAtA = -10f;
    [SerializeField] private float cameraZAtB = 10f;

    [Header("Move Points")]
    [SerializeField] private Vector3 pointA = new Vector3(-3f, 0f, 0f);
    [SerializeField] private Vector3 pointB = new Vector3(3f, 0f, 0f);

    [Header("Options")]
    [SerializeField] private bool useLocalPosition = false;
    [SerializeField] private bool clamp01 = true;

    [Header("Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float pointSize = 0.25f;

    private void Reset()
    {
        targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            return;
        }

        float cameraZ = targetCamera.transform.position.z;

        float t = Mathf.InverseLerp(cameraZAtA, cameraZAtB, cameraZ);

        if (!clamp01)
        {
            t = (cameraZ - cameraZAtA) / (cameraZAtB - cameraZAtA);
        }

        Vector3 newPosition = Vector3.Lerp(pointA, pointB, t);

        if (useLocalPosition)
        {
            transform.localPosition = newPosition;
        }
        else
        {
            transform.position = newPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }

        Vector3 a = GetWorldPoint(pointA);
        Vector3 b = GetWorldPoint(pointB);

        // 経路
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(a, b);

        // 始点 A
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(a, pointSize);

        // 終点 B
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(b, pointSize);

        // 現在位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pointSize * 1.25f);
    }

    private Vector3 GetWorldPoint(Vector3 point)
    {
        if (!useLocalPosition)
        {
            return point;
        }

        if (transform.parent != null)
        {
            return transform.parent.TransformPoint(point);
        }

        return point;
    }
}