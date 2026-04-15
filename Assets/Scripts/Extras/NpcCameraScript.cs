using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class NpcCameraScript : MonoBehaviour
{
    [Header("Active")]
    [SerializeField] GameObject player;
    [SerializeField] float dis = 10f;
    [SerializeField] float zoom = 10f;
    [SerializeField] SpriteRenderer img;
    [SerializeField] GameObject diag;

    [Header("Camera")]
    [SerializeField] PlayerInput input;
    [SerializeField] CinemachineVirtualCamera virtualCam;

    private bool active = false;
    private Color startCol;
    private float targetFOV;
    private Movement move;
    private bool isSpeaking = false;
    float targetA;

    private void Start()
    {
        startCol = img.color;
        targetFOV = virtualCam.m_Lens.FieldOfView;
        move = player.GetComponent<Movement>();
    }

    private void Update()
    {
        float curDis = Vector3.Distance(player.transform.position, transform.position);
        active = curDis < dis;
        ActiveLogic();

        virtualCam.m_Lens.FieldOfView = Mathf.Lerp(virtualCam.m_Lens.FieldOfView, targetFOV, 5f * Time.deltaTime);
    }

    void ActiveLogic()
    {
        if (!isSpeaking) targetA = active ? 0.6f : 0f;
        Color colour = img.color;
        colour.a = Mathf.Lerp(colour.a, targetA, 10f * Time.deltaTime);
        img.color = colour;

        Animation anim = diag.GetComponent<Animation>();

        if (active && input.actions.FindAction("Player/Talk").triggered && !isSpeaking)
        {
            virtualCam.LookAt = this.transform;
            anim["DiagBoxAnimIn"].time = 0f;
            anim["DiagBoxAnimIn"].speed = 1f;
            anim.Play();
            isSpeaking = true;
            targetFOV = zoom;
            targetA = 0f;
        }

        if(!active && isSpeaking)
        {
            virtualCam.LookAt = player.transform;
            anim["DiagBoxAnimIn"].time = anim["DiagBoxAnimIn"].length;
            anim["DiagBoxAnimIn"].speed = -1f;
            anim.Play();
            isSpeaking = false;
            targetFOV = 26.5f;
            targetA = 6f;
        }

        //For other version, use input ["Back"] and disable move
    }
}
