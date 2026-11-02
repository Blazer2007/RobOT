// HybridCameraController.cs
using System;
using UnityEngine;

public class HybridCameraController : MonoBehaviour, ICameraView
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 headOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private float thirdPersonDistance = 4f;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private LayerMask obstructionLayers = ~0;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private KeyCode toggleKey = KeyCode.V;

    private float yaw;
    private float pitch;

    public bool IsFirstPerson { get; private set; }
    public event Action<bool> ViewModeChanged;

    public Quaternion PlanarRotation => Quaternion.Euler(0f, yaw, 0f);
    public bool AlignsCharacterToView => IsFirstPerson;

    public void SetTarget(Transform newTarget) => target = newTarget;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetKeyDown(toggleKey))
        {
            IsFirstPerson = !IsFirstPerson;
            ViewModeChanged?.Invoke(IsFirstPerson);
        }

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 head = target.position + headOffset;

        if (IsFirstPerson)
        {
            transform.SetPositionAndRotation(head, rotation);
            return;
        }

        Vector3 direction = rotation * Vector3.back;
        float distance = thirdPersonDistance;
        if (Physics.SphereCast(head, collisionRadius, direction, out RaycastHit hit,
                thirdPersonDistance, obstructionLayers, QueryTriggerInteraction.Ignore))
            distance = hit.distance;

        transform.SetPositionAndRotation(head + direction * distance, rotation);
    }
}