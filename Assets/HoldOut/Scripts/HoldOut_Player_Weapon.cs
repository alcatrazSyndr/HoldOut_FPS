using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HoldOut_Player_Weapon : MonoBehaviour
{
    [SerializeField] private float _swaySmooth = 1f;
    [SerializeField] private float _swayMultiplier = 1f;
    [SerializeField] private float _adsTimer = 0.4f;
    [SerializeField] private float _weaponSwingXOffset = 0.2f;
    [SerializeField] private float _weaponSwingYOffset = 0.2f;
    [SerializeField] private float _swingMultiplier = 1f;
    [SerializeField] private HoldOut_Player_Input _input;
    [SerializeField] private HoldOut_Player_Camera _camera;
    [SerializeField] private HoldOut_Player_Movement _movement;
    [SerializeField] private Transform _weaponRoot;
    [SerializeField] private Vector3 _weaponRootHFPosition;
    [SerializeField] private Vector3 _weaponRootADSPosition;
    [SerializeField] private Vector3 _weaponRootHFRotation;
    [SerializeField] private Vector3 _weaponRootADSRotation;
    [SerializeField] private Vector3 _weaponRootSprintPosition;
    [SerializeField] private Vector3 _weaponRootSprintRotation;
    [SerializeField] private AnimationCurve _weaponSwingCurve;

    private Vector3 _weaponSwingLeftPosition;
    private Vector3 _weaponSwingRightPosition;

    private bool _ads = false;
    private bool _inADSShift = false;
    private bool _swingLeft = true;

    public float _weaponSwingStage = 0.5f;

    private void Start()
    {
        _weaponSwingLeftPosition = _weaponRootHFPosition + new Vector3(-_weaponSwingXOffset, 0f, 0f);
        _weaponSwingRightPosition = _weaponRootHFPosition + new Vector3(_weaponSwingXOffset, 0f, 0f);
    }

    private void Update()
    {
        WeaponSway();
        ADS();
    }

    private void WeaponSway()
    {
        if (_ads || _inADSShift) return;

        float mouseX = _input.MouseInput.x * _swayMultiplier;
        float mouseY = _input.MouseInput.y * _swayMultiplier;

        Quaternion rotX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRot = rotX * rotY;

        _weaponRoot.localRotation = Quaternion.Slerp(_weaponRoot.localRotation, targetRot, _swaySmooth * Time.deltaTime);

        if (_movement.IsWalking())
        {
            if (_swingLeft)
            {
                _weaponSwingStage -= Time.deltaTime * _swingMultiplier;
                if (_weaponSwingStage <= 0f)
                {
                    _weaponSwingStage = 0f;
                    _swingLeft = false;
                }
            }
            else if (!_swingLeft)
            {
                _weaponSwingStage += Time.deltaTime * _swingMultiplier;
                if (_weaponSwingStage >= 1f)
                {
                    _weaponSwingStage = 1f;
                    _swingLeft = true;
                }
            }
            Vector3 targetPos = Vector3.Lerp((_weaponSwingLeftPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), (_weaponSwingRightPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), _weaponSwingStage);
            _weaponRoot.localPosition = targetPos;
        }
        else if (_movement.IsSprinting())
        {
            _weaponRoot.localPosition = Vector3.Lerp(_weaponRoot.localPosition, _weaponRootSprintPosition, Time.deltaTime * 5f);
            _weaponRoot.localRotation = Quaternion.Lerp(_weaponRoot.localRotation, Quaternion.Euler(_weaponRootSprintRotation.x, _weaponRootSprintRotation.y, _weaponRootSprintRotation.z), Time.deltaTime * 5f);
        }
        else if (_weaponSwingStage != 0.5f)
        {
            _weaponSwingStage = 0.5f;
            _swingLeft = true;
        }
        else
        {
            _weaponRoot.localPosition = Vector3.Lerp(_weaponRoot.localPosition, _weaponRootHFPosition, Time.deltaTime * 5f);
        }
    }

    private void ADS()
    {
        if (!_ads && _input.ADSInput && !_inADSShift)
        {
            StartCoroutine(WeaponMoveADSCRT(true));
        }
        else if (_ads && !_input.ADSInput && !_inADSShift)
        {
            StartCoroutine(WeaponMoveADSCRT(false));
        }
    }

    private IEnumerator WeaponMoveADSCRT(bool newADSActive)
    {
        _inADSShift = true;
        float timer = 0f;
        float interpolation = 0f;
        float targetFOV;
        while (timer < _adsTimer)
        {
            if (newADSActive)
            {
                _weaponRoot.localPosition = Vector3.Lerp(_weaponRootHFPosition, _weaponRootADSPosition, interpolation);
                _weaponRoot.localRotation = Quaternion.Lerp(Quaternion.Euler(_weaponRootHFRotation.x, _weaponRootHFRotation.y, _weaponRootHFRotation.z), Quaternion.Euler(_weaponRootADSRotation.x, _weaponRootADSRotation.y, _weaponRootADSRotation.z), interpolation);
                targetFOV = 10f + ((1f - interpolation) * 30f);
            }
            else
            {
                _weaponRoot.localPosition = Vector3.Lerp(_weaponRootADSPosition, _weaponRootHFPosition, interpolation);
                _weaponRoot.localRotation = Quaternion.Lerp(Quaternion.Euler(_weaponRootADSRotation.x, _weaponRootADSRotation.y, _weaponRootADSRotation.z), Quaternion.Euler(_weaponRootHFRotation.x, _weaponRootHFRotation.y, _weaponRootHFRotation.z), interpolation);
                targetFOV = 10f + (interpolation * 30f);
            }
            _camera.UpdateFrontCameraFOV(targetFOV);
            timer += Time.deltaTime;
            interpolation = timer / _adsTimer;
            yield return null;
        }
        interpolation = 1f;
        if (newADSActive)
        {
            _weaponRoot.localPosition = Vector3.Lerp(_weaponRootHFPosition, _weaponRootADSPosition, interpolation);
            _weaponRoot.localRotation = Quaternion.Lerp(Quaternion.Euler(_weaponRootHFRotation.x, _weaponRootHFRotation.y, _weaponRootHFRotation.z), Quaternion.Euler(_weaponRootADSRotation.x, _weaponRootADSRotation.y, _weaponRootADSRotation.z), interpolation);
            targetFOV = 10f + ((1f - interpolation) * 30f);
        }
        else
        {
            _weaponRoot.localPosition = Vector3.Lerp(_weaponRootADSPosition, _weaponRootHFPosition, interpolation);
            _weaponRoot.localRotation = Quaternion.Lerp(Quaternion.Euler(_weaponRootADSRotation.x, _weaponRootADSRotation.y, _weaponRootADSRotation.z), Quaternion.Euler(_weaponRootHFRotation.x, _weaponRootHFRotation.y, _weaponRootHFRotation.z), interpolation);
            targetFOV = 10f + (interpolation * 30f);
        }
        _camera.UpdateFrontCameraFOV(targetFOV);
        _ads = newADSActive;
        _inADSShift = false;
        yield break;
    }
}
