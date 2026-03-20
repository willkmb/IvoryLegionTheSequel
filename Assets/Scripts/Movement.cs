using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementScript : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float acceleration = 10f;
    [SerializeField] float deceleration = 10f;
    [SerializeField] float angleOffset = 0f;

    [Header("Turning")]
    [SerializeField] float turnSmooth = 10f;
    [SerializeField] Transform pivot;
    [SerializeField] float pivotOffset = 0.5f;
    [SerializeField] float pivotSmooth = 5f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 newInput;
    private Vector3 pivotStartPos;
    private PlayerManager playerInput;
    private Vector2 preMove;
    private float turnDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerManager>();
        pivotStartPos = pivot.localPosition;
    }

    private void Update()
    {
        moveInput = playerInput.moveInput;
        newInput = new Vector3(moveInput.x, 0f, moveInput.y);
        newInput = Quaternion.Euler(0f, angleOffset, 0f) * newInput;
    }

    private void FixedUpdate()
    {
        Move();
        Rotate();
        pivotSlide();
    }

    void Move()
    {
        newInput = newInput.normalized;
        float curSpeed = speed;

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
        Quaternion newRot = Quaternion.Slerp(transform.rotation, targetRot, turnSmooth * Time.fixedDeltaTime); //lerp between current rotation and target rotation by smoothing value
        Quaternion rotChange = newRot * Quaternion.Inverse(transform.rotation); //how much did rotation change this frame, determining the angle
        transform.RotateAround(pivot.position, Vector3.up, rotChange.eulerAngles.y);
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
}