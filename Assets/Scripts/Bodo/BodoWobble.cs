using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class BodoWobble : MonoBehaviour
{
    [SerializeField] float wobbleFrequencyMultiplier;
    [SerializeField] float wobbleIntensityMultiplier;
    [SerializeField] float wobbleDashMultiplier;
    [SerializeField] Transform player;
    [SerializeField] float minDis = 0.61f;
    private float timer = 0;
    private Movement move;

    private float wobblefrequency;
    private float wobbleintensity;

    private float distance;

    private void Start() { move = player.gameObject.GetComponentInParent<Movement>(); }

    private void Update()
    {
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;
        transform.LookAt(targetPosition);

        distance = Vector3.Distance(transform.position, player.position);

        if (move.moveInput.magnitude > 0.1f) { wobblefrequency = wobbleFrequencyMultiplier; wobbleintensity = wobbleIntensityMultiplier; }
        else { wobblefrequency = wobbleFrequencyMultiplier * distance; wobbleintensity = wobbleIntensityMultiplier * distance; }
        if (distance > minDis) wobbling();
    }

    void wobbling()
    {
        timer += Time.deltaTime * wobblefrequency;
        float wobble = Mathf.Sin(timer) * wobbleintensity;
        Vector3 curRot = transform.localEulerAngles;
        curRot.z = wobble;
        transform.localEulerAngles = curRot;
    }
}
