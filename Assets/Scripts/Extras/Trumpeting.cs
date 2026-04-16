using UnityEngine;
using UnityEngine.InputSystem;

public class Trumpeting : MonoBehaviour
{
    [SerializeField] ParticleSystem spray;
    private AudioSource sound;
    private PlayerInput input;
    private bool canPlay = true;
    private void Start()
    {
        input = GetComponentInParent<PlayerInput>();
        sound = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (input.actions["Trumpet"].triggered && canPlay)
        {
            canPlay = false;
            if (!input.gameObject.GetComponentInChildren<Swimming>().isSwimming)
            {
                float pitch = Random.Range(1.05f, 1.25f);
                sound.pitch = pitch;
                sound.Play();
                GetComponent<Animation>().Play();
            }
            else spray.Play();

            Invoke("Reset", GetComponent<Animation>().clip.length);
        }
    }

    private void Reset() { canPlay = true; }
}
