using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DustParticlesScript : MonoBehaviour
{
    [SerializeField] ParticleSystem[] walkParticles;
    [SerializeField] ParticleSystem hitParticle;
    [SerializeField] float rate = 17.5f;
    [SerializeField] float easeSpeed = 5f;

    [Header("Particles Emission")]
    [SerializeField] float walkEmission;
    [SerializeField] float sprintEmission;
    [SerializeField] float dashEmission;
    private float targetRate = 0f;
    private float curRate = 0f;
    private Movement move;

    private void Start()
    {
        foreach (ParticleSystem particle in walkParticles) particle.Play();
        move = GetComponentInChildren<Movement>();
    }

    private void Update()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.25f)) targetRate = 0f;

        targetRate = (move.moveInput.sqrMagnitude > 0.001f || move.stateInt == 3) ? rate : 0f;
        curRate = Mathf.Lerp(curRate, targetRate, easeSpeed * Time.deltaTime);
        foreach (ParticleSystem particle in walkParticles)
        {
            ParticleSystem.EmissionModule emission = particle.emission;
            emission.rateOverTime = curRate;
        }

        emissionChange();
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude;
        if (impact < 3.5f) return;

        Vector3 normal = collision.contacts[0].normal;
        float Xaxis = Mathf.Abs(normal.x);
        float Yaxis = Mathf.Abs(normal.y);
        float Zaxis = Mathf.Abs(normal.z);

        if (Yaxis > Xaxis && Yaxis > Zaxis) hitParticle.Play();
    }

    private void emissionChange()
    {
        float emissionVal;

        switch (move.stateInt)
        {
            case 3:
                emissionVal = dashEmission;
                break;

            case 2:
                emissionVal = sprintEmission;
                break;

            case 1:
                emissionVal = walkEmission;
                break;

            default:
                emissionVal = 0f;
                break;
        }

        foreach (ParticleSystem particle in walkParticles)
        {
            var emission = particle.emission;
            emission.rateOverTime = emissionVal;
        }
    }
}

