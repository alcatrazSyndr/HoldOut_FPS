using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldOut_Player_Movement : MonoBehaviour
{
    [SerializeField] private HoldOut_Player_Input _input;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private float _movementSpeed = 1f;
    [SerializeField] private float _jumpCooldown = 1f;
    [SerializeField] private float _jumpHeight = 1f;
    [SerializeField] private float _groundCheckRadius = 0.01f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _floorLayer;
    [SerializeField] private Camera _playerCamera;

    private bool _grounded = false;
    private bool _canJump = true;
    private bool _sprinting = false;

    private void FixedUpdate()
    {
        Gravity();
        Movement();
        Jumping();
    }

    public bool IsSprinting()
    {
        return _sprinting;
    }

    public bool IsWalking()
    {
        return _input.MovementInput != Vector2.zero && _grounded && !_sprinting;
    }

    public bool IsGrounded()
    {
        return _grounded;
    }

    public void Movement()
    {
        if (_grounded)
        {
            if (!_input.SprintInput && !_input.ADSInput)
            {
                _rb.velocity = (_input.MovementInput.y * _movementSpeed * transform.forward) + (_input.MovementInput.x * _movementSpeed * transform.right);
                _sprinting = false;
            }
            else if (_input.SprintInput && !_input.ADSInput)
            {
                if (_input.MovementInput.y > 0f && _input.MovementInput.x == 0f)
                {
                    _rb.velocity = (_input.MovementInput.y * _movementSpeed * 1.5f * transform.forward) + (_input.MovementInput.x * _movementSpeed * transform.right);
                    _sprinting = true;
                }
                else
                {
                    _rb.velocity = (_input.MovementInput.y * _movementSpeed * transform.forward) + (_input.MovementInput.x * _movementSpeed * transform.right);
                    _sprinting = false;
                }
            }
            else if (_input.ADSInput)
            {
                _rb.velocity = (_input.MovementInput.y * _movementSpeed * 0.5f * transform.forward) + (_input.MovementInput.x * _movementSpeed * 0.5f * transform.right);
                _sprinting = false;
            }
        }
    }

    private void Gravity()
    {
        _grounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _floorLayer.value);
    }

    private void Jumping()
    {
        if (_input.JumpInput && _grounded && _canJump && !_input.ADSInput)
        {
            Jump();
        }
    }

    private void Jump()
    {
        _canJump = false;
        StartCoroutine(JumpCooldownCRT());

        _rb.velocity += Vector3.up * _jumpHeight;
    }

    private IEnumerator JumpCooldownCRT()
    {
        yield return new WaitForSeconds(_jumpCooldown);
        _canJump = true;
        yield break;
    }
}
