using System.Collections;
using UnityEngine;

public class BoatMove : MonoBehaviour
{
    [SerializeField] Vector3 moveToLocation;
    GameObject boat;
    GameObject player;
    bool isMoving = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boat = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            if (Mathf.Abs(boat.transform.localPosition.z - moveToLocation.z) > 0.1f)
            {
                boat.transform.localPosition = Vector3.MoveTowards(boat.transform.localPosition, moveToLocation, 0.01f);
            }
            else
            {
                transform.localPosition = moveToLocation;
                player.transform.parent = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player = other.gameObject;
            this.GetComponent<BoxCollider>().enabled = false;
            isMoving = true;
            player.transform.parent = this.transform;
        }

    }

}
