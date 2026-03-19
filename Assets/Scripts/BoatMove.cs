using System.Collections;
using UnityEngine;

public class BoatMove : MonoBehaviour
{
    [SerializeField] Vector3 moveToLocation;
    GameObject boat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boat = transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine("MoveBoat");
    }

    IEnumerator MoveBoat()
    {
        while (moveToLocation.z - transform.position.z  > 0.1f)
        {
            float newZ = Mathf.Lerp(transform.position.z, moveToLocation.z, Time.deltaTime);
            transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
        }

        transform.position = moveToLocation;
        yield return null;
    }
}
