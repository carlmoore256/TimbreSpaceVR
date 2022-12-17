using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandLegacy : MonoBehaviour
{
    public LineRenderer line;
    public Transform wandBase;

    private List<Grain> collidedGrains;

    // private OVRInput.Controller controller;

    void Start()
    {
        // if (gameObject.transform.parent.name.Contains("Left"))
        // {
        //     controller = OVRInput.Controller.LTouch;
        // }
        // else
        // {
        //     controller = OVRInput.Controller.RTouch;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("grain"))
        {
            collidedGrains.Add(collision.gameObject.GetComponent<Grain>());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("grain"))
        {
            collidedGrains.Remove(collision.gameObject.GetComponent<Grain>());
        }
    }
}
