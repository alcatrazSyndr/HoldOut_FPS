using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private GameObject _hitmarker;
    [SerializeField] private LayerMask _hitScanMask;
    [SerializeField] private TextMeshProUGUI _ammoCounter;
    [SerializeField] private Image _crosshairImage;

    private Vector2 _crosshairOriginalSize;
    private Vector2 _crosshairShootSize;
    private RectTransform _crosshairRect;

    private void Start()
    {
        _crosshairRect = _crosshair.GetComponent<RectTransform>();
        _crosshairOriginalSize = _crosshairRect.sizeDelta;
        _crosshairShootSize = new Vector2(_crosshairOriginalSize.x * 2f, _crosshairOriginalSize.y * 2f);
    }

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

        if (_crosshair.activeSelf)
        {
            _crosshairRect.sizeDelta = Vector2.Lerp(_crosshairRect.sizeDelta, _crosshairOriginalSize, Time.deltaTime * 10f);
        }
    }

    public void UpdateAmmoCount(int maxAmmo, int currentAmmo)
    {
        _ammoCounter.text = currentAmmo.ToString() + "/" + maxAmmo.ToString();
    }

    public void UpdateCrosshairFill(float interpolation)
    {
        _crosshairImage.fillAmount = interpolation;
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

    public void Shoot()
    {
        if (_crosshair.activeSelf)
        {
            _crosshairRect.sizeDelta = _crosshairShootSize;
        }
    }

    public RaycastHit GetAimingPoint()
    {
        RaycastHit hit;

        if (Physics.Raycast(_camera.position, _camera.forward, out hit, 10000f, _hitScanMask.value))
        {
            return hit;
        }

        return hit;
    }

    public void Hitmarker(bool headshot)
    {
        StartCoroutine(HitmarkerCRT(headshot));
    }

    private IEnumerator HitmarkerCRT(bool headshot)
    {
        _hitmarker.SetActive(true);
        for (int i = 0; i < _hitmarker.transform.childCount; i++)
        {
            if (headshot)
            {
                _hitmarker.transform.GetChild(i).GetComponent<Image>().color = Color.red;
            }
            else
            {
                _hitmarker.transform.GetChild(i).GetComponent<Image>().color = Color.white;
            }
        }
        yield return new WaitForSeconds(0.05f);
        _hitmarker.SetActive(false);
        yield break;
    }
}
