using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [HideInInspector] public int stateInt = 0;
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float baseAcceleration = 10f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float angleOffset = 0f;
    private bool isMoving = false;

    [Header("Turning")]
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] Transform pivot;
    [SerializeField] Transform pivotParent;
    [SerializeField] float pivotOffset = 0.5f;
    [SerializeField] float pivotSmooth = 5f;
    private Vector3 currentAnglePivot;
    private Vector3 oldAnglePivot;

    [Header("Head and Shoulder Turning")]
    [SerializeField] GameObject head;
    public float maxHeadTurn;
    [SerializeField] float headTurnSpeed;
    [SerializeField] GameObject shoulders;
    [SerializeField] float maxShoulderTurn;
    [SerializeField] float shoulderTurnSpeed;

    private Vector3 currentAngle;
    private Vector3 oldAngle;
    private Vector3 playerForward;
    private Vector3 inputDirectionChange;
    private float inputDirClamped;


    [Header("Dash")]
    [SerializeField] float duration = 1f;
    [SerializeField] float dashMultiplier = 3f;
    [SerializeField] float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashRemaining = 0f;
    private float dashRemainingState = 0f;

    [Header("Sprint")]
    [SerializeField] float sprintMultiplier = 1.5f;

    [Header("RampAlign")]
    [SerializeField] float alignSpeed = 10f;
    private float rayLength = 2f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 newInput;
    private Vector3 pivotStartPos;
    private PlayerManager playerInput;
    private Vector2 preMove;
    private float turnDir;
    private InputAction dash;
    private InputAction sprint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerManager>();
        pivotStartPos = pivot.localPosition;
        dash = playerInput.gameObject.GetComponent<PlayerInput>().actions["Dash"];
        sprint = playerInput.gameObject.GetComponent<PlayerInput>().actions["Sprint"];

        currentAngle = transform.localEulerAngles;
        oldAngle = currentAngle;

        currentAnglePivot = transform.localEulerAngles;
        oldAnglePivot = currentAngle;
    }

    private void Update()
    {
        moveInput = playerInput.moveInput;
        newInput = new Vector3(moveInput.x, 0f, moveInput.y);
        newInput = Quaternion.Euler(0f, angleOffset, 0f) * newInput;
        if (dash.triggered && !isDashing && isMoving) { dashRemaining = duration; dashRemainingState = 0.3f; deceleration = 6; }
        stateUpdate();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        //HeadRotate();
        HeadAndShoulderRotate();
        pivotSlide();
        dashing();
        aligning();
    }

    void Move()
    {
        newInput = newInput.normalized;
        float curSpeed = speed;
        if (sprint.IsPressed()) curSpeed *= sprintMultiplier;

        Vector3 curVel = rb.linearVelocity;
        Vector3 targetVel = new Vector3(newInput.x * curSpeed, rb.linearVelocity.y, newInput.z * curSpeed);

        if(newInput.sqrMagnitude > 0.001f)
        {
            curVel = Vector3.MoveTowards(curVel, targetVel, acceleration * Time.fixedDeltaTime);  //if moving move current velocity towards target velocity by acceleration value
            isMoving = true;
        }
        else 
        {
            curVel = Vector3.MoveTowards(curVel, Vector3.zero, deceleration * Time.fixedDeltaTime); //if not moving move current velocity towards zero by deceleration value
            isMoving = false;
        } 

        curVel.y = rb.linearVelocity.y; //reintroduce gravity
        rb.linearVelocity = curVel; //assign new velocity value to character
    }

    void Rotate()
    {
        if (newInput.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(newInput); //gets the movement direction of the player and converts to quaternion
        Quaternion newRot = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime); //lerp between current rotation and target rotation by smoothing value
        Quaternion rotChange = newRot * Quaternion.Inverse(transform.rotation); //how much did rotation change this frame, determining the angle
        transform.RotateAround(pivot.position, Vector3.up, rotChange.eulerAngles.y);

    }

    void HeadAndShoulderRotate()
    {
        playerForward = new Vector3(transform.forward.x, 0, transform.forward.z); //gets 2D forward direction of player (disregards up and down rotation)
        inputDirectionChange = playerForward - newInput; //gets difference between player forward and input direction
        inputDirClamped = Mathf.Clamp(inputDirectionChange.magnitude, 0, 1); //clamps difference between 0 and 1 so head and shoulders dont rotate over max amount

        currentAngle = transform.localEulerAngles;
        Quaternion shoulderRot = Quaternion.identity;
        Quaternion headRot = Quaternion.identity;

        //if turning anticlockwise, turn head and shoulders to match
        if (currentAngle.y - oldAngle.y < -0.1f) 
        { 
            shoulderRot = Quaternion.Euler(0, -maxShoulderTurn * inputDirClamped, 0); 
            headRot = Quaternion.Euler(0, -maxHeadTurn * inputDirClamped, 0); 

        }
        
        //if turning clockwise, turn head and shoulders to match
        else if (currentAngle.y - oldAngle.y > 0.1f) 
        { 
            shoulderRot = Quaternion.Euler(0, maxShoulderTurn * inputDirClamped, 0); 
            headRot = Quaternion.Euler(0, maxHeadTurn * inputDirClamped, 0); 
        }
        
        //if going straight, straighten head and shoulders
        else { shoulderRot = Quaternion.identity; headRot = Quaternion.identity; }

        //if no input and head and shoulders arent straight, straighten head and shoulders
        if (newInput.magnitude == 0 && shoulders.transform.localRotation != Quaternion.identity && head.transform.localRotation != Quaternion.identity)
        {
            shoulderRot = Quaternion.identity;
            headRot = Quaternion.identity;
        }

        shoulders.transform.localRotation = Quaternion.Slerp(shoulders.transform.localRotation, shoulderRot, Time.deltaTime * turnSpeed * shoulderTurnSpeed);
        head.transform.localRotation = Quaternion.Slerp(head.transform.localRotation, headRot, Time.deltaTime * turnSpeed * headTurnSpeed);
        oldAngle = currentAngle;
    }

    void pivotSlide()
    {
        /*
        if (moveInput.sqrMagnitude > 0.001f && preMove.sqrMagnitude > 0.001f)
        {
            float turn = Vector2.SignedAngle(preMove, moveInput);
            turnDir = turn < 0 ? 1f : -1f;
            Debug.Log(turnDir);
        }
        float targetOffset = pivotOffset * -inputDirClamped * turnDir;
        Vector3 targetPos = pivotStartPos + new Vector3(targetOffset, 0f, 0f);
        pivot.localPosition = Vector3.Lerp(pivot.localPosition,targetPos,pivotSmooth * Time.deltaTime);
        
        preMove = moveInput;
        */

        currentAnglePivot = transform.localEulerAngles;
        if (currentAnglePivot.y - oldAnglePivot.y < -0.1f) //if turning anticlockwise, move pivot left
        {
            Quaternion pivotRot = Quaternion.Euler(0, -90 * inputDirClamped, 0);
            pivotParent.localRotation = Quaternion.Slerp(pivotParent.localRotation, pivotRot, Time.deltaTime * pivotSmooth);

            //print("Im spinning left on my Y axis");
        }
        else if (currentAnglePivot.y - oldAnglePivot.y > 0.1f) //if turning clockwise,  move pivot right
        {
            Quaternion pivotRot = Quaternion.Euler(0, 90 * inputDirClamped, 0);
            pivotParent.localRotation = Quaternion.Slerp(pivotParent.localRotation, pivotRot, Time.deltaTime * pivotSmooth);

            //print("Im spinning right on my Y axis");
        }
        else //if going straight, centre pivot
        {
            pivotParent.localRotation = Quaternion.Slerp(pivotParent.localRotation, Quaternion.identity, Time.deltaTime * pivotSmooth);

            //print("Im going straight");
        }

        if (newInput.magnitude == 0 && pivotParent.localRotation != Quaternion.identity) //if no input and pivot not centred, centre pivot
        {
            pivotParent.localRotation = Quaternion.Slerp(pivotParent.localRotation, Quaternion.identity, Time.deltaTime * pivotSmooth);
        }

        oldAnglePivot = currentAngle;

    }

    void dashing()
    {
        if (dashRemaining > 0f)
        {
            isDashing = true;
            dashRemaining -= Time.fixedDeltaTime;
            //Vector3 dir = transform.forward * dashMultiplier;
            Vector3 dir = (newInput + transform.forward) * 0.5f * dashMultiplier; //gets midpoint of forward direction and input direction
            rb.linearVelocity = dir; 
            Invoke("resetDash", dashCooldown);
        }
    }

    void resetDash() { isDashing = false; }

    void aligning()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, rayLength))
        {
            Quaternion targetAlign = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetAlign, alignSpeed * Time.deltaTime);
            //acceleration = baseAcceleration * hit.normal.y; //<- experimenting with having less acceleration while going up slopes
        }
    }

    void stateUpdate()
    {
        if (dashRemainingState > 0f) { dashRemainingState -= Time.deltaTime; stateInt = 3; return; }
        if (sprint.IsPressed() && newInput.sqrMagnitude > 0.001f) { stateInt = 2; return; }
        if (newInput.sqrMagnitude > 0.001f) { stateInt = 1; return; }
        stateInt = 0;
    }
}