using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellScript : MonoBehaviour
{
    public float jumpStrength;

    private Rigidbody2D miRigidBody;
    void Start()
    {
        miRigidBody = GetComponent<Rigidbody2D>();
        miRigidBody.AddForce(transform.up * jumpStrength);
    }

    
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.tag);
        switch (collision.tag)
        {
            case "bullet":
            case "heart":
                collision.gameObject.SetActive(false);
                miRigidBody.velocity = Vector3.zero;
                miRigidBody.AddForce(Vector3.up * jumpStrength / 2);
                break;
            case "topCol":
                //The original game doesen't prevent you from throwing this out of the level and neither will i
            default:
                break;
        }
    }
}
