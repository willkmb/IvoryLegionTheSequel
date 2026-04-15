using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BodoFollowScript : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float radius = 3f;
    [SerializeField] float moveRate = 1f;
    [SerializeField] float speed = 3f;
    [SerializeField] float pointspeed = 3f;

    [SerializeField] float SwimmingHeight;

    private Vector3 target;
    private Movement move;
    private float curAngle;
    [HideInInspector] public bool setHeight = false;

    private void Start()
    {
        target = transform.position;
        curAngle = 180f;
        move = player.gameObject.GetComponentInChildren<Movement>();
        StartCoroutine(movePoint());
    }

    private void FixedUpdate()
    {
        Vector3 dir = Quaternion.Euler(0, curAngle, 0) * player.forward;
        Vector3 targetPoint = player.position + dir * radius;
        target = Vector3.Lerp(target, targetPoint, pointspeed * Time.fixedDeltaTime);

        Vector3 newPos = Vector3.Lerp(transform.position, target, speed * Time.fixedDeltaTime);
        if(!setHeight) transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
        else transform.position = new Vector3(newPos.x, SwimmingHeight, newPos.z);
    }

    IEnumerator movePoint()
    {
        while (true)
        {
            if (move == null) { yield return null; continue; }

            if (move.moveInput.magnitude > 0.1f || move.dash.triggered)
            {
                curAngle = Random.Range(120f, 240f); //semi circle
                yield return new WaitForSeconds(moveRate);
            }
            else yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(target, 0.1f);
    }

}
