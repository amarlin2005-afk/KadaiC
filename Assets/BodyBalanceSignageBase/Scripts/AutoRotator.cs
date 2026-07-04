using UnityEngine;

public class AutoRotator : MonoBehaviour
{
    /// <summary>
    /// 回転の軸
    /// </summary>
    [SerializeField]
    Vector3 RotationAxis = Vector3.one;
    
    /// <summary>
    /// 回転の速度
    /// </summary>
    public float RotationSpeed = 1.0f;

    void Update()
    {
        transform.Rotate(RotationAxis, Time.deltaTime * RotationSpeed);
    }
}
