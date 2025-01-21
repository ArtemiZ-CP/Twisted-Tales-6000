using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool _fixedXAxis;
    [SerializeField] private bool _fixedYAxis = true;
    [SerializeField] private bool _fixedZAxis;

    private Transform _cameraTransform;

    private void Start()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = transform.position + _cameraTransform.rotation * Vector3.forward;
        Vector3 targetOrientation = _cameraTransform.rotation * Vector3.up;

        if (_fixedXAxis)
        {
            targetOrientation.x = 0;
        }

        if (_fixedYAxis)
        {
            targetOrientation.y = 0;
        }

        if (_fixedZAxis)
        {
            targetOrientation.z = 0;
        }

        transform.LookAt(targetPosition, targetOrientation);
    }
}
