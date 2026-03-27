using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Movement : MonoBehaviour
{
    [HideInInspector] public int stateInt = 0;
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float angleOffset = 0f;

    [Header("Turning")]
    [SerializeField] float turnSpeed = 10f;
    [SerializeField] Transform pivot;
    [SerializeField] float pivotOffset = 0.5f;
    [SerializeField] float pivotSmooth = 5f;

    [Header("Head and Shoulder Turning")]
    [SerializeField] GameObject head;
    [SerializeField] float maxHeadTurn;

    [SerializeField] GameObject shoulders;
    [SerializeField] float maxShoulderTurn;
    private Vector3 currentAngle;
    private Vector3 oldAngle;


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
    }

    private void Update()
    {
        moveInput = playerInput.moveInput;
        newInput = new Vector3(moveInput.x, 0f, moveInput.y);
        newInput = Quaternion.Euler(0f, angleOffset, 0f) * newInput;
        if (dash.triggered && !isDashing) { dashRemaining = duration; dashRemainingState = 0.3f; deceleration = 6; }
        stateUpdate();
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        //HeadRotate();
        ShoulderRotate();
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

        }
        else { curVel = Vector3.MoveTowards(curVel, Vector3.zero, deceleration * Time.fixedDeltaTime); } //if not moving move current velocity towards zero by deceleration value

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

    void HeadRotate()
    {

    }

    void ShoulderRotate()
    {
        if (newInput.sqrMagnitude < 0.001f) return;
        Vector3 playerForward = new Vector3(0, transform.forward.y, transform.forward.z);
        float inputDirectionChange = newInput.magnitude - playerForward.magnitude;
        //Debug.Log(inputDirectionChange.magnitude);

        currentAngle = transform.eulerAngles;
        if (currentAngle.y < oldAngle.y) 
        {
            Quaternion shoulderRot = Quaternion.Euler(0, -maxShoulderTurn * inputDirectionChange, 0);

            shoulders.transform.localRotation = Quaternion.Slerp(shoulders.transform.localRotation, shoulderRot, Time.deltaTime * turnSpeed);

            print("Im spinning left on my Y axis"); 

        }
        else if (currentAngle.y > oldAngle.y) 
        {
            Quaternion shoulderRot = Quaternion.Euler(0, maxShoulderTurn * inputDirectionChange, 0);

            shoulders.transform.localRotation = Quaternion.Slerp(shoulders.transform.localRotation, shoulderRot, Time.deltaTime * turnSpeed);

            print("Im spinning right on my Y axis"); 
        }
        Debug.Log(oldAngle.magnitude);
        Debug.Log(currentAngle.magnitude);
        oldAngle = currentAngle;
        

    }

    void pivotSlide()
    {
        if (moveInput.sqrMagnitude > 0.001f && preMove.sqrMagnitude > 0.001f)
        {
            float turn = Vector2.SignedAngle(preMove, moveInput);
            turnDir = turn < 0 ? 1f : -1f;
        }

        preMove = moveInput;

        float targetOffset = turnDir * pivotOffset;
        Vector3 targetPos = pivotStartPos + new Vector3(targetOffset, 0f, 0f);
        pivot.localPosition = Vector3.Lerp(pivot.localPosition,targetPos,pivotSmooth * Time.deltaTime);
    }

    void dashing()
    {
        if (dashRemaining > 0f)
        {
            isDashing = true;
            dashRemaining -= Time.fixedDeltaTime;
            Vector3 dir = transform.forward * dashMultiplier;
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