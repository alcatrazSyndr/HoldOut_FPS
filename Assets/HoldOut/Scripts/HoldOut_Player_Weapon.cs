using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldOut_Player_Weapon : MonoBehaviour
{
    [SerializeField] private float _swaySmooth = 1f;
    [SerializeField] private float _swayMultiplier = 1f;
    [SerializeField] private HoldOut_Player_Input _input;
    [SerializeField] private Transform _weaponRoot;

    private void Update()
    {
        float mouseX = _input.MouseInput.x * _swayMultiplier;
        float mouseY = _input.MouseInput.y * _swayMultiplier;

        Quaternion rotX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRot = rotX * rotY;

        _weaponRoot.localRotation = Quaternion.Slerp(_weaponRoot.localRotation, targetRot, _swaySmooth * Time.deltaTime);
    }
}
