using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NubeScript : MonoBehaviour
{
    public float velocidad;
    public GameObject bellPrefab;

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
        switch (collider.tag)
        {
            case "heart":
            case "bala":
                collider.gameObject.SetActive(false);
                SpawnBell();
                break;
            case "leftCol":
                gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    void SpawnBell()
    {
        print("Nube hit");
    }
}
