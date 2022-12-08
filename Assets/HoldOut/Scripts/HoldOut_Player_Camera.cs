using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoldOut_Player_Camera : MonoBehaviour
{
    [SerializeField] private HoldOut_Player_Input _input;
    [SerializeField] private HoldOut_Player_Movement _movement;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _camera;
    [SerializeField] private float _mouseSensitivity = 1f;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _frontCamera;
    [SerializeField] private GameObject _crosshair;

    private void Update()
    {
        _camera.localRotation = Quaternion.Euler(ClampCamera(_camera.localRotation.eulerAngles.x - (_input.MouseInput.y * _mouseSensitivity)), 0f, 0f);
        transform.Rotate(transform.up, (_input.MouseInput.x * _mouseSensitivity));

        if (_mainCamera.fieldOfView < 90f && _movement.IsSprinting() && !_input.ADSInput)
        {
            _mainCamera.fieldOfView += Time.deltaTime * 90f;
        }
        else if (_mainCamera.fieldOfView > 85f && !_movement.IsSprinting())
        {
            _mainCamera.fieldOfView -= Time.deltaTime * 90f;
        }
    }

    private float ClampCamera(float angle)
    {
        if (angle < 85f)
        {
            return angle;
        }
        else if (angle >= 85f && angle < 200f)
        {
            return 85f;
        }
        else if (angle > 275f)
        {
            return angle;
        }
        else
        {
            return 275f;
        }
    }

    public void UpdateFrontCameraFOV(float newFov)
    {
        _frontCamera.fieldOfView = newFov;
        if (newFov < 40f && _crosshair.activeSelf)
        {
            _crosshair.SetActive(false);
        }
        else if (!_crosshair.activeSelf && newFov == 40f)
        {
            _crosshair.SetActive(true);
        }
    }
}
