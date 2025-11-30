using UnityEngine;

public class CopyCameraRotation : MonoBehaviour
{
    [SerializeField] private Transform target; // CameraOffset ³Ö±â

    private void LateUpdate()
    {
        if (target != null)
            transform.rotation = target.rotation;
    }
}
