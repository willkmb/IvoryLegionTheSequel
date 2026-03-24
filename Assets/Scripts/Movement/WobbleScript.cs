using UnityEngine;
using UnityEngine.InputSystem;

public class WobbleScript : MonoBehaviour
{
    [SerializeField] float wobbleFrequencyMultiplier;
    [SerializeField] float wobbleIntensityMultiplier;
    private float timer = 0;
    private PlayerManager playerInput;
    private Vector2 move;
    private Rigidbody rb;
    private Movement movement;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerManager>();
    }

    private void Update()
    {
        move = playerInput.moveInput;
        bool isMoving = move.sqrMagnitude > 0.001f;

        if (isMoving) wobbling();
        else Reset();
    }

    void wobbling()
    {
        float wobbleFrequency = rb.linearVelocity.magnitude * wobbleFrequencyMultiplier;
        float wobbleIntensity = rb.linearVelocity.magnitude * wobbleIntensityMultiplier;

        timer += Time.deltaTime * wobbleFrequency;
        float wobble = Mathf.Sin(timer) * wobbleIntensity;

        float newZ = Mathf.MoveTowardsAngle(transform.localEulerAngles.z, wobble, 360f * Time.deltaTime);
        Vector3 curRot = transform.localEulerAngles;
        curRot.z = newZ;
        transform.localEulerAngles = curRot;

        float scaleY = 0.95f + (Mathf.Sin(timer) + 1) * 0.025f;
        Vector3 scale = transform.localScale;
        scale.y = scaleY;
        transform.localScale = scale;
    }

    private void Reset()
    {
        Vector3 curRot = transform.localEulerAngles;
        float newZ = Mathf.MoveTowardsAngle(curRot.z, 0f, 360f * Time.deltaTime);
        curRot.z = newZ;
        transform.localEulerAngles = curRot;
        Vector3 scale = transform.localScale;
        scale.y = 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, scale, 8f * Time.deltaTime);
    }
}
