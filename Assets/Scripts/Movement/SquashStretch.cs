using UnityEngine;

public class SquashStretch : MonoBehaviour
{
    [SerializeField] float squashAmount;
    [SerializeField] float squashSmooth;
    [SerializeField] float timer;
    private float squashSideAmount = 1.05f;
    private float clampAmountSquash = 1.25f;

    private Vector3 startScale;
    private float squashTimer;
    private Vector3 squashTarget;
    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        if (squashTimer > 0) { squashTimer -= Time.deltaTime; transform.localScale = Vector3.Lerp(transform.localScale, squashTarget, Time.deltaTime * squashSmooth); }
        else transform.localScale = Vector3.Lerp(transform.localScale, startScale, Time.deltaTime * squashSmooth);
    }

    public void impactDir(Vector3 normal)
    {
        float Xaxis = Mathf.Abs(normal.x);
        float Yaxis = Mathf.Abs(normal.y);
        float Zaxis = Mathf.Abs(normal.z);

        if (Yaxis > Xaxis && Yaxis > Zaxis) { squashImpact(0); }
        else if (Zaxis > Xaxis) squashImpact(1);
    }

    void squashImpact(int axis)
    {
        float squash = Mathf.Clamp(squashAmount, 1f, clampAmountSquash);
        float squashSide = squash - 0.5f;
        switch (axis)
        {
            case 0:
                squashTarget = new Vector3(startScale.x * squashSideAmount, startScale.y / squash, startScale.z * squashSideAmount);

                break;

            case 1:
                squashTarget = new Vector3(startScale.x * squashSideAmount, startScale.y * squashSideAmount, startScale.z / squash);
                break;
        }

        squashTimer = timer;
    }
}
