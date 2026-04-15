using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamLook : MonoBehaviour
{    
    [SerializeField] PlayerInput input;

    [Header("Camera Movement")]
    [SerializeField] CinemachineVirtualCamera virtualCam;
    [SerializeField] float yaw = 0.1f;
    [SerializeField] float pitch = 0.5f;
    [SerializeField] float camSmooth = 3f;

    [SerializeField] GameObject lookAtObj;
    [SerializeField] float camMoveValue;

    private CinemachineComposer composer;

    [Header("Head Movement")]
    [SerializeField] Transform head;
    [SerializeField] float headSmooth = 3f;

    private Vector2 lookInput;

    private float startX;
    private float startY;
    private Quaternion startRot;

    void Start()
    {
        composer = virtualCam.GetCinemachineComponent<CinemachineComposer>();
        startX = composer.m_ScreenX;
        startY = composer.m_ScreenY;
        startRot = head.localRotation;
    }

    void Update()
    {

    }

    void LateUpdate()
    {
    }

    private void FixedUpdate()
    {
        lookInput = input.actions["Look"].ReadValue<Vector2>();
        RotateHead();
        MoveCamera();

    }

    void MoveCamera()
    {
        //rotates camera
        float yawOffset = lookInput.x * -yaw;
        float pitchOffset = lookInput.y * pitch;

        composer.m_ScreenX = Mathf.Lerp(composer.m_ScreenX, startX + yawOffset, Time.fixedDeltaTime * camSmooth);
        composer.m_ScreenY = Mathf.Lerp(composer.m_ScreenY, startY + pitchOffset, Time.fixedDeltaTime * camSmooth);

        //moves object that the camera is looking at up down left and right
        if (Vector3.Angle(virtualCam.gameObject.transform.forward, transform.forward) <= 90)
        {
            lookAtObj.transform.localPosition = new Vector3(Mathf.Lerp(lookAtObj.transform.localPosition.x, camMoveValue * lookInput.x, Time.fixedDeltaTime),
                Mathf.Lerp(lookAtObj.transform.localPosition.y, camMoveValue * lookInput.y, Time.fixedDeltaTime), lookAtObj.transform.localPosition.z);
        }
        else
        {
            lookAtObj.transform.localPosition = new Vector3(Mathf.Lerp(lookAtObj.transform.localPosition.x, camMoveValue * -lookInput.x, Time.fixedDeltaTime),
                Mathf.Lerp(lookAtObj.transform.localPosition.y, camMoveValue * lookInput.y, Time.fixedDeltaTime), lookAtObj.transform.localPosition.z);
        }


    }
    void RotateHead()
    {
        /*
        float yawOffset = lookInput.x * -headYaw;
        float pitchOffset = lookInput.y * -headPitch;
        Quaternion targetRot = startRot * Quaternion.Euler(pitchOffset, yawOffset, 0f);
        head.localRotation = Quaternion.Slerp(head.localRotation, targetRot, Time.deltaTime * headSmooth);
        */

        //rotates head
        float headTurnY;
        float headTurnZ;

        if (Vector3.Angle(virtualCam.gameObject.transform.forward, transform.forward) <= 90)
        {
            headTurnY = this.GetComponent<Movement>().maxHeadTurn * lookInput.x * 1.5f;

        }
        //inverts left and right rotation if player is facing camera
        else
        {
            headTurnY = this.GetComponent<Movement>().maxHeadTurn * -lookInput.x * 1.5f;
        }
        headTurnZ = this.GetComponent<Movement>().maxHeadTurn * lookInput.y;

        Quaternion headRot = Quaternion.Euler(-headTurnZ, headTurnY, 0);
        head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, headRot, Time.deltaTime * headSmooth);
    }
}
