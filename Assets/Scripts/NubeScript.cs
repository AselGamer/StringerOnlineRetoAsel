using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NubeScript : MonoBehaviour
{
    public int idNube;
    public float velocidad;
    public GameObject bellPrefab;
    public bool hasBell;

    private SpriteRenderer miRenderer;

    void Start()
    {
        hasBell = true;
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
                if (hasBell)
                {
                    collider.gameObject.SetActive(false);
                    if (!bellPrefab.activeInHierarchy)
                    {
                        SpawnBell();
                    }
                }
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
        bellPrefab.GetComponent<BellScript>().idCampana = idNube;
        bellPrefab.GetComponent<BellScript>().isActive = true;
        bellPrefab.GetComponent<BellScript>().bellStage = 0;
        bellPrefab.transform.parent = null;
        bellPrefab.transform.position = gameObject.transform.position;
        bellPrefab.SetActive(true);
        hasBell = false;
    }
}
