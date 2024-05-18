using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField] TMPro.TextMeshProUGUI debugText;

    private Rigidbody rb;
    // private Collider collider;
    // [SerializeField] private InputActionAsset actionAsset;
    private Vector3 playerVelocity;
    private bool groundedLastFrame;
    private bool walledLastFrame;
    [SerializeField] private Vector2 camSens;
    [SerializeField] private Transform camParent;
    
    [SerializeField] private float gravity = -10.0f;
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float accel = 0.85f;
    [SerializeField] private float deaccel = 2.0f;
    [SerializeField] private float pSpeed = 10f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float jumpControl = 0.8f;
    [SerializeField] private float dashSpeed = 3.0f;
    // in seconds
    [SerializeField] private float dashTime = 1f;

    [SerializeField] private GameObject playerModel;
    
    private Vector3 moveDir;
    private Vector3 normalizedMoveDir;
    private Vector3 camRotateDir;
    private Vector3 dashDirection;
    private Vector3 forward;
    private bool jumpPressed = false;
    private bool dashPressedThisFrame = false;
    private bool dashPressedLastFrame = false;
    // above 0 when dashing
    private float currentDashTimer = 0f;
    private float dotVel;

    // velocity not controlled by player direction or max speed
    private Vector3 addedVel;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        debugText. text = "";
        debugText.text += "Grounded: " + IsGrounded() + "\n";
        debugText.text += "Walled: " + IsWalled() + "\n";
        debugText.text += "Dash timer: " + currentDashTimer + "\n";
        debugText.text += "Dot: " + Vector3.Dot(playerVelocity, forward) + "\n";
        debugText.text += "Add: " + addedVel;
    }

    void FixedUpdate()
    {
        playerVelocity = rb.velocity;

        // camera logic
        camParent.eulerAngles = new Vector3(camParent.eulerAngles.x + camRotateDir.y * camSens.y, camParent.eulerAngles.y + camRotateDir.x * camSens.x, 0f);
        // camParent.eulerAngles = new Vector3(camRotateDir.y * camSens.y, camRotateDir.x * camSens.x, 0f);

        // player move/run logic
        // get forward direction based on cam, temporarily remove x rotation
        Vector3 oldCamAngles = camParent.eulerAngles;
        camParent.eulerAngles = new Vector3(0f, camParent.eulerAngles.y + camRotateDir.x * -camSens.x, 0f);
        // camParent.eulerAngles = new Vector3(0f, camRotateDir.x * camSens.x, 0f);
        if (moveDir != Vector3.zero) 
        {
            normalizedMoveDir = Vector3.Normalize(moveDir);
            forward = camParent.TransformDirection(normalizedMoveDir);
            dotVel = Mathf.Max(startSpeed, dotVel + accel * (IsGrounded() ? 1f : 0.5f));
        }
        camParent.eulerAngles = oldCamAngles;

        playerVelocity.x = (forward.x * dotVel);
        playerVelocity.z = (forward.z * dotVel);

        // top speed logic
        if (dotVel > pSpeed)
        {
            dotVel -= accel;
        }

        if (moveDir != Vector3.zero) playerModel.transform.forward = forward;
        else dotVel = Mathf.Max(dotVel - deaccel * (IsGrounded() ? 1f : 0.25f), 0f);

        addedVel = new Vector3(Mathf.Max(Mathf.Abs(addedVel.x) - deaccel * (IsGrounded() ? 1f : 0.25f), 0f) * Mathf.Sign(addedVel.x), 0f, Mathf.Max(Mathf.Max(Mathf.Abs(addedVel.z) - deaccel * (IsGrounded() ? 1f : 0.25f), 0f) * Mathf.Sign(addedVel.z)));

        // gravity
        if (!IsGrounded() && !(currentDashTimer > 0) ) playerVelocity.y += gravity;
        else playerVelocity.y = 0;

        // jump logic
        if ((IsGrounded() || currentDashTimer > 0) && jumpPressed)
        {
            playerVelocity.y = jumpHeight;
            currentDashTimer = 0f;
        }

        if (playerVelocity.y > 0 && !jumpPressed)
        {
            playerVelocity.y *= jumpControl;
        }
        
        // wall logic
        if (IsWalled())
        {
            if (!walledLastFrame && !IsGrounded() && dotVel > pSpeed / 2)
            {
                addedVel = -forward * dotVel / 2;
                playerVelocity.y = dotVel / 2;
            }

            dotVel = 0;
        }

        // end of frame logic
        groundedLastFrame = IsGrounded();
        walledLastFrame = IsWalled();
        rb.velocity = playerVelocity + addedVel;
        dashPressedLastFrame = dashPressedThisFrame;
    }

    public void OnJump(InputValue value)
    {
        float v = value.Get<float>();

        // 1 when pressed, 0 when not
        jumpPressed = (v != 0f);

    }

    public void OnMove(InputValue value)
    {
        // Debug.Log("OnMove");
        Vector2 v = value.Get<Vector2>();
        moveDir = new Vector3(v.x, 0, v.y);

    }

    public void OnDash(InputValue value)
    {
        // Debug.Log("OnDash");
        float v = value.Get<float>();

        dashPressedThisFrame = (v != 0f);
    }

    public void OnCamRotate(InputValue value)
    {
        // Debug.Log("OnMove");
        Vector2 v = value.Get<Vector2>();
        camRotateDir = new Vector3(v.x, v.y, 0);
    }

    // todo have constants reflect size of collider 
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), 1.05f);
    }

    private bool IsWalled()
    {
        return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), playerModel.transform.forward, 0.55f) && Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), playerModel.transform.forward, 0.55f);
    }

}
