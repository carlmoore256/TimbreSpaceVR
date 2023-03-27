using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleField : MonoBehaviour
{
    public GameObject particlePrefab;
    public int numParticles = 1000;
    public float particleSize = 0.05f;
    public float positionRange = 5f;
    

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numParticles; i++) {
            GameObject particle = Instantiate(particlePrefab, transform);
            Vector3 position = new Vector3(
                Random.Range(-positionRange, positionRange), 
                Random.Range(-positionRange, positionRange), 
                Random.Range(-positionRange, positionRange)
            );
            particle.transform.localPosition = position;
            particle.GetComponent<Renderer>().material.color = new Color(
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f)
            );
            particle.transform.localScale = new Vector3(particleSize, particleSize, particleSize);
            SpringJoint springJoint = particle.GetComponent<SpringJoint>();
            // springJoint.tolerance = TsvrApplication.Settings.particleTolerance;
            springJoint.connectedAnchor = position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
