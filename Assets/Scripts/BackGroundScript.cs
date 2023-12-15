using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundScript : MonoBehaviour
{
    public float velocidad;
    public Server _server;

    private Transform miTransform;
    private float[] arrPos;
    void Start()
    {
        miTransform = transform;
        arrPos = new float[2];
    }

    
    void Update()
    {
        miTransform.Translate(Vector3.right * velocidad * Time.deltaTime);
        arrPos[0] = miTransform.localPosition.x;
        arrPos[1] = miTransform.localPosition.y;
        _server.SendBackground(arrPos);
    }
}
