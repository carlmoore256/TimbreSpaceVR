using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrainModel : MonoBehaviour
{

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private float pos_speed = 0.5f;
    private float rot_speed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(!Vector3.Equals(transform.position, targetPosition))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * pos_speed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rot_speed);
        }
    }
    //Vector3.MoveTowards

    //moves grain model to a location, and rotates to look at a point
    void MoveLookAt()
    {

    }
}
