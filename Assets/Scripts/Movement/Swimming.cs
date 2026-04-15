using System;
using System.Collections;
using UnityEngine;
using static Unity.Collections.Unicode;

public class Swimming : MonoBehaviour
{
    [Header("Swimming")]
    [SerializeField] GameObject Floor;
    [SerializeField] GameObject Ramp;
    [SerializeField] GameObject trunk;

    [Header("bodo")]
    [SerializeField] GameObject bodo;
    [SerializeField] float bodoLeaveOffset = 1;
    private bool wasOnFloor = false;
    private bool wasOnRamp = false;
    private Rigidbody rb;
    [HideInInspector] public bool isSwimming = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update() //comapres raycast hits to see if you have entered the water
    {
        bool isOnFloor = false;
        bool isOnRamp = false;
        Vector3 rayTarget = transform.position + transform.forward * 0.5f;
        if (Physics.Raycast(rayTarget, Vector3.down, out RaycastHit hit, 1f))
        {
            if (hit.collider.gameObject == Floor) isOnFloor = true;
            if (isOnFloor != wasOnFloor) StartCoroutine(swimming(isOnFloor));
            wasOnFloor = isOnFloor;
        }

        Vector3 rayTargetLeave = transform.position + transform.forward / bodoLeaveOffset;
        if (Physics.Raycast(rayTargetLeave, Vector3.down, out RaycastHit hit2, 2f))
        {
            if (hit2.collider.gameObject == Ramp) isOnRamp = true;
            if (wasOnRamp && !isOnRamp) bodoLeave();
            wasOnRamp = isOnRamp;
        }
    }

    IEnumerator swimming(bool isOnFloor) //sets trunk active/inactive based on swimming state. plus changes to bodos swimming state
    {
        if (isOnFloor)
        {
            trunk.SetActive(true);
            rb.linearDamping = 3.5f;
            isSwimming = true;

            bodo.GetComponent<BodoFollowScript>().setHeight = true;
            bodo.GetComponent<Rigidbody>().useGravity = false;
            bodo.GetComponent<BodoWobble>().enabled = false;
            bodo.transform.Find("Ring").gameObject.SetActive(true);
        }
        else
        {
            trunk.GetComponent<Animation>().Play("TrunkDownAnim");
            rb.linearDamping = 0f;
            isSwimming = false;
            yield return new WaitForSeconds(0.35f);
            trunk.SetActive(false);
        }
    }

    void bodoLeave()
    {
        Vector3 angle = this.transform.localEulerAngles;
        if (angle.x > 180f)
        {
            bodo.GetComponent<BodoFollowScript>().setHeight = false;
            bodo.GetComponent<Rigidbody>().useGravity = true;
            bodo.GetComponent<BodoWobble>().enabled = true;
            bodo.transform.Find("Ring").gameObject.SetActive(false);
        }
    }
}

