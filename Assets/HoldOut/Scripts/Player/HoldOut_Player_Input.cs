using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldOut_Player_Input : MonoBehaviour
{
    public Vector2 MovementInput = Vector2.zero;
    public Vector2 MouseInput = Vector2.zero;
    public bool JumpInput = false;
    public bool SprintInput = false;
    public bool ADSInput = false;
    public bool FireInput = false;
    public bool ReloadInput = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        MovementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        MouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        JumpInput = Input.GetKey(KeyCode.Space);
        SprintInput = Input.GetKey(KeyCode.LeftShift);
        ADSInput = Input.GetKey(KeyCode.Mouse1);
        FireInput = Input.GetKey(KeyCode.Mouse0);
        ReloadInput = Input.GetKey(KeyCode.R);
    }
}
