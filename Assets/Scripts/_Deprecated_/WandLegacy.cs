using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if true
public class WandLegacy : MonoBehaviour
{
    public LineRenderer line;
    public Transform wandBase;
    private List<GrainOld> collidedGrains;
    void Start()
    {
        // if (gameObject.transform.parent.name.Contains("Left"))
        //     controller = OVRInput.Controller.LTouch;
        // else
        //     controller = OVRInput.Controller.RTouch;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("grain"))
        {
            collidedGrains.Add(collision.gameObject.GetComponent<GrainOld>());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("grain"))
        {
            collidedGrains.Remove(collision.gameObject.GetComponent<GrainOld>());
        }
    }
}
#endif