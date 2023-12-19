using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo2Script : MonoBehaviour
{
    public float velocidad;
    private Server _server;
    public int idEnemigo;
    public bool isServer;
    public int puntosMatar;

    private SpriteRenderer miRenderer;
    private Animator miAnimator;
    private Vector3 _move;

    void Start()
    {
        if (isServer)
        {
            _server = GameObject.FindGameObjectWithTag("server").GetComponent<Server>();
        }
        _move = Vector3.left + Vector3.up;
        miRenderer = GetComponent<SpriteRenderer>();
        miAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (miRenderer.isVisible)
        {
            transform.Translate(_move * velocidad * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        switch (collider.tag)
        {
            case "bala":
                GetComponent<BoxCollider2D>().enabled = false;
                if (_server != null)
                {
                    _server.UpdatePlayerPoints(collider.gameObject.GetComponent<BulletScript>().idJugSim, puntosMatar);
                }
                collider.gameObject.SetActive(false);
                miAnimator.SetBool("Morir", true);
                break;
            case "topCol":
            case "leftCol":
                gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    public void Morir()
    {
        miAnimator.SetBool("Morir", false);
        gameObject.SetActive(false);
    }
}
