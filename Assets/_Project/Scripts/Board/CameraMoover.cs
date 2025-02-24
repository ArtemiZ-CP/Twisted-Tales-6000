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
        Vector3 currentRotation = transform.localRotation.eulerAngles + new Vector3(rotation, 0, 0);
        currentRotation.x = Mathf.Clamp(currentRotation.x, _minRotationAngle, _maxRotationAngle);
        transform.localRotation = Quaternion.Euler(currentRotation);
    }
}
