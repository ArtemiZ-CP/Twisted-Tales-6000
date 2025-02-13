using UnityEngine;

public class CameraMoover : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private float _rotationSpeed = 100f;
    [SerializeField] private float _minRotationAngle = 35;
    [SerializeField] private float _maxRotationAngle = 70;
    
    private void Update()
    {
        float scrollDelta = PlayerInput.GetMouseScrollDelta();
        if (scrollDelta != 0 && _targetTransform != null)
        {
            RotateAroundTarget(scrollDelta);
        }
    }
    
    private void RotateAroundTarget(float scrollDelta)
    {
        float rotation = scrollDelta * _rotationSpeed * Time.deltaTime;
        transform.RotateAround(_targetTransform.position, Vector3.right, rotation);
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.x = Mathf.Clamp(currentRotation.x, _minRotationAngle, _maxRotationAngle);
        transform.rotation = Quaternion.Euler(currentRotation);
    }
}
