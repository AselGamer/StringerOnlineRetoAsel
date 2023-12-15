using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float velocidad;

    private Transform miTransform;

    void Start()
    {
        miTransform = transform;
    }

    void Update()
    {
        miTransform.Translate(Vector3.right * velocidad * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "rightCol":
            case "leftCol":
            case "botCol":
            case "topCol":
                gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }
}
