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
    [SerializeField] private float _weaponShootCooldown = 0.15f;
    [SerializeField] private float _weaponShootKickback = 0.15f;
    [SerializeField] private float _weaponShootKickup = 2f;
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _reloadTime = 1.2f;
    [SerializeField] private int _maxAmmo = 30;
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
    [SerializeField] private Transform _muzzleFlash;
    [SerializeField] private Transform _bulletDummy;
    [SerializeField] private Transform _lhIKTarget;

    private Vector3 _weaponSwingLeftPosition;
    private Vector3 _weaponSwingRightPosition;
    private Vector3 _weaponSwingSprintLeftPosition;
    private Vector3 _weaponSwingSprintRightPosition;

    private Vector3 _lhikTargetSprintPos = new Vector3(-0.11f, -0.023f, 0.174f);
    private Vector3 _lhikTargetNormPos = new Vector3(-0.131f, -0.023f, 0.174f);

    private Quaternion _lhikTargetSprintRot = Quaternion.Euler(-59.233f, -57.17f, -71.226f);
    private Quaternion _lhikTargetNormRot = Quaternion.Euler(-59.233f, -88.772f, -71.226f);

    private bool _ads = false;
    private bool _inADSShift = false;
    private bool _swingLeft = true;
    private bool _reloading = false;

    private float _weaponSwingStage = 0.5f;
    private float _shootCooldown;

    private int _currentAmmo = 0;

    private void Start()
    {
        _currentAmmo = _maxAmmo;
        _camera.UpdateAmmoCount(_maxAmmo, _currentAmmo);
        _shootCooldown = _weaponShootCooldown;
        _weaponSwingLeftPosition = _weaponRootHFPosition + new Vector3(-_weaponSwingXOffset, 0f, 0f);
        _weaponSwingRightPosition = _weaponRootHFPosition + new Vector3(_weaponSwingXOffset, 0f, 0f);
        _weaponSwingSprintLeftPosition = _weaponRootSprintPosition + new Vector3(-(_weaponSwingXOffset * 4f), 0f, 0f);
        _weaponSwingSprintRightPosition = _weaponRootSprintPosition + new Vector3((_weaponSwingXOffset * 4f), 0f, 0f);
    }

    private void Update()
    {
        Reload();
        WeaponSway();
        ADS();
        Shoot();

        if (_movement.IsSprinting())
        {
            _lhIKTarget.localPosition = _lhikTargetSprintPos;
            _lhIKTarget.localRotation = _lhikTargetSprintRot;
        }
        else
        {
            _lhIKTarget.localPosition = _lhikTargetNormPos;
            _lhIKTarget.localRotation = _lhikTargetNormRot;
        }
    }

    private void Shoot()
    {
        if (_shootCooldown > 0f)
        {
            _shootCooldown -= Time.deltaTime;
            return;
        }

        if (_input.FireInput && _movement.IsGrounded() && !_movement.IsSprinting() && !_reloading && _currentAmmo > 0)
        {
            _weaponRoot.localPosition -= new Vector3(0f, 0f, (_ads ? (_weaponShootKickback / 2f) : _weaponShootKickback));
            _weaponRoot.localRotation *= Quaternion.Euler(-(_ads ? (_weaponShootKickup / 2f) : _weaponShootKickup), 0f, 0f);
            _camera.Shoot();
            _shootCooldown = _weaponShootCooldown;
            RaycastHit hit = _camera.GetAimingPoint();
            StartCoroutine(MuzzleFlashCRT());
            StartCoroutine(DummyBulletCRT(hit.point));

            if (hit.transform.CompareTag("DamageColliderHead") || hit.transform.CompareTag("DamageColliderBody"))
            {
                if (hit.transform.CompareTag("DamageColliderHead"))
                {
                    _camera.Hitmarker(true);
                }
                else
                {
                    _camera.Hitmarker(false);
                }
            }

            _currentAmmo--;
            _camera.UpdateAmmoCount(_maxAmmo, _currentAmmo);
        }
    }

    private void Reload()
    {
        if (_reloading) return;
        if (_currentAmmo >= _maxAmmo) return;
        if (!_input.ReloadInput) return;

        if (_ads)
        {
            StartCoroutine(WeaponMoveADSCRT(false));
        }

        _reloading = true;
        StartCoroutine(ReloadCRT());
    }

    private IEnumerator ReloadCRT()
    {
        float timer = 0f;
        float interpolation;
        while (timer < _reloadTime)
        {
            timer += Time.deltaTime;
            interpolation = timer / _reloadTime;
            interpolation = Mathf.Clamp01(interpolation);
            _camera.UpdateCrosshairFill(interpolation);
            yield return null;
        }
        interpolation = 1f;
        _camera.UpdateCrosshairFill(interpolation);
        _reloading = false;
        _currentAmmo = _maxAmmo;
        _camera.UpdateAmmoCount(_maxAmmo, _currentAmmo);
        yield break;
    }

    private IEnumerator DummyBulletCRT(Vector3 aimPoint)
    {
        GameObject dummyBullet = Instantiate(_bulletDummy.gameObject, _muzzleFlash.position, Quaternion.LookRotation(aimPoint - _muzzleFlash.position));
        float existTimer = 2f;
        while (existTimer > 0f && dummyBullet != null)
        {
            existTimer -= Time.deltaTime;
            if (dummyBullet != null)
            {
                dummyBullet.transform.position += _bulletSpeed * Time.deltaTime * dummyBullet.transform.forward;
            }
            yield return null;
        }
        if (dummyBullet != null)
        {
            Destroy(dummyBullet);
        }
        yield break;
    }

    private IEnumerator MuzzleFlashCRT()
    {
        _muzzleFlash.gameObject.SetActive(true);
        for (int i = 0; i < _muzzleFlash.childCount; i++)
        {
            _muzzleFlash.GetChild(i).transform.localRotation = Quaternion.Euler(Random.Range(0f, 359f), -90f, 0f);
        }
        yield return new WaitForSeconds(0.1f);
        _muzzleFlash.gameObject.SetActive(false);
        yield break;
    }

    private void WeaponSway()
    {
        if (_inADSShift || _reloading) return;

        float mouseX = _input.MouseInput.x * _swayMultiplier;
        float mouseY = _input.MouseInput.y * _swayMultiplier;

        Quaternion rotX = Quaternion.AngleAxis(-mouseY + _weaponRootHFRotation.y, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(mouseX + _weaponRootHFRotation.x, Vector3.up);

        Quaternion targetRot = rotX * rotY;
        Vector3 targetPos = _weaponRootHFPosition;

        if (_movement.IsWalking() || _movement.IsSprinting() || _ads)
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

            if (_ads)
            {
                targetPos = _weaponRootADSPosition;
                targetRot = Quaternion.Euler(_weaponRootADSRotation.x, _weaponRootADSRotation.y, _weaponRootADSRotation.z);
            }
            else if (_movement.IsWalking())
            {
                targetPos = Vector3.Lerp((_weaponSwingLeftPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), (_weaponSwingRightPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), _weaponSwingStage);
            }
            else if (_movement.IsSprinting())
            {
                targetPos = Vector3.Lerp((_weaponSwingSprintLeftPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), (_weaponSwingSprintRightPosition - new Vector3(0f, _weaponSwingYOffset * _weaponSwingCurve.Evaluate(_weaponSwingStage), 0f)), _weaponSwingStage);
                targetRot = Quaternion.Euler(_weaponRootSprintRotation.x, _weaponRootSprintRotation.y, _weaponRootSprintRotation.z);
            }
            else if (_weaponSwingStage != 0.5f)
            {
                _weaponSwingStage = 0.5f;
                _swingLeft = true;
            }
        }
        else
        {
            targetPos = _weaponRootHFPosition;
        }

        _weaponRoot.localPosition = Vector3.Lerp(_weaponRoot.localPosition, targetPos, _swingMultiplier * 2f * Time.deltaTime);
        _weaponRoot.localRotation = Quaternion.Slerp(_weaponRoot.localRotation, targetRot, _swaySmooth * 2f * Time.deltaTime);
    }

    private void ADS()
    {
        if (_reloading) return;

        if (!_ads && _input.ADSInput && !_inADSShift && _movement.IsGrounded())
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
