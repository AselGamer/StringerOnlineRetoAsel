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
    public bool isKilled;

    private SpriteRenderer miRenderer;
    private Animator miAnimator;
    private Vector3 _move;

    public Vector3 posOriginal;

    void Start()
    {
        if (isServer)
        {
            _server = GameObject.FindGameObjectWithTag("server").GetComponent<Server>();
        }
        isKilled = false;
        miRenderer = GetComponent<SpriteRenderer>();
        miAnimator = GetComponent<Animator>();
        _move = Vector3.left + Vector3.up;
        
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
                if (_server != null && !isKilled)
                {
                    _server.UpdatePlayerPoints(collider.gameObject.GetComponent<BulletScript>().idJugSim, puntosMatar);
                }
                isKilled = true;
                collider.gameObject.SetActive(false);
                miAnimator.SetBool("Morir", true);
                break;
            case "topCol":
            case "leftCol":
                miAnimator.SetBool("Morir", false);
                gameObject.SetActive(false);
                CheckLastGroupObject();
                break;
            default:
                break;
        }
    }

    public void Morir()
    {
        miAnimator.SetBool("Morir", false);
        gameObject.SetActive(false);
        CheckLastGroupObject();
    }

    private void CheckLastGroupObject()
    {
        foreach (Transform groupChild in transform.parent)
        {
            if (groupChild.gameObject.activeSelf)
            {
                return;
            }
        }
        transform.parent.gameObject.SetActive(false);
    }
}
