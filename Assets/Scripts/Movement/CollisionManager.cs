using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CollisionManager : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] CinemachineVirtualCamera cam;
    [SerializeField] float intensityMultiplier;
    [SerializeField] float time;
    [SerializeField] float minImpact;

    [Header("Haptic")]
    [SerializeField] float hapticMultiplier = 0.2f;
    [SerializeField] float DashMultiplier = 1f;

    private Rigidbody rb;
    private PlayerManager playerInput;
    private Movement move;

    private float timer;
    private float startInt;
    private float startTime;

    private bool dashing = false;
    private bool collided = false;
    private float colIntensity = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponentInParent<PlayerManager>();
        move = GetComponent<Movement>();
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                CinemachineBasicMultiChannelPerlin noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                noise.m_AmplitudeGain = Mathf.Lerp(startInt, 0f, (1 - (timer / startTime)));
            }
        }

        if (move.stateInt == 1) { intensityMultiplier = 0.45f; minImpact = 1.5f; }
        else if (move.stateInt == 2) { intensityMultiplier = 0.4f; minImpact = 2.25f; }
        else if (move.stateInt == 3) { intensityMultiplier = 0.325f; minImpact = 2.95f; }
        else { intensityMultiplier = 0.45f; minImpact = 2f; }

        if (Gamepad.current == null) return;
        if (!dashing && !collided) Gamepad.current.SetMotorSpeeds(0, 0);

        if (playerInput.GetComponent<PlayerInput>().actions["Dash"].triggered && !dashing)
            StartCoroutine(dashHaptic());
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (Mathf.Abs(collision.relativeVelocity.y) < collision.relativeVelocity.magnitude * 0.6f) return;
        //if (collision.contacts[0].normal.y > 0.6f) return;
        //if (collision.relativeVelocity.y > -minImpact) return;
        if (impact < minImpact) return;

        float intensity = intensityMultiplier * (impact / minImpact);
        cameraShake(intensity, time);
        colSet(impact);

        if (impact >= 4f)
        {
            Vector3 normal = collision.contacts[0].normal;
            SquashStretch squashStretch = GetComponentInChildren<SquashStretch>();
            squashStretch.impactDir(normal);
        }
    }

    void cameraShake(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = intensity;
        timer = time;
        startInt = intensity;
        startTime = time;
    }

    public void colSet(float impact)
    {
        colIntensity = impact;
        StartCoroutine(ColHaptic());
    }

    IEnumerator dashHaptic()
    {
        dashing = true;
        float intensity = Mathf.Clamp01(rb.linearVelocity.magnitude * DashMultiplier);
        Gamepad.current.SetMotorSpeeds(intensity / 2, intensity);
        yield return new WaitForSeconds(0.15f);
        Gamepad.current.SetMotorSpeeds(0, 0);
        yield return new WaitForSeconds(0.85f);
        dashing = false;
    }

    IEnumerator ColHaptic()
    {
        collided = true;
        float intensity = Mathf.Clamp01(rb.linearVelocity.magnitude * hapticMultiplier);
        Gamepad.current.SetMotorSpeeds(intensity / 2, intensity);
        yield return new WaitForSeconds(0.15f);
        Gamepad.current.SetMotorSpeeds(0, 0);
        collided = false;
    }
}