using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldOut_Player_Camera : MonoBehaviour
{
    [SerializeField] private HoldOut_Player_Input _input;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _camera;
    [SerializeField] private float _mouseSensitivity = 1f;

    private void Update()
    {
        _camera.localRotation = Quaternion.Euler(ClampCamera(_camera.localRotation.eulerAngles.x - (_input.MouseInput.y * _mouseSensitivity)), 0f, 0f);
        transform.Rotate(transform.up, (_input.MouseInput.x * _mouseSensitivity));
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
}
