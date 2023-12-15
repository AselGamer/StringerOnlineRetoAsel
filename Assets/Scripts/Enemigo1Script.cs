using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo1Script : MonoBehaviour
{
    public float velocidad;

    private SpriteRenderer miRenderer;

    void Start()
    {
        miRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        if (miRenderer.isVisible)
        {
            transform.Translate(Vector3.left * velocidad * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag == "bala")
        {
            gameObject.SetActive(false);
        }
    }
}
