using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellScript : MonoBehaviour
{
    public int idCampana;
    public float jumpStrength;
    public float gravStr;
    public int bellStage;
    public bool isActive;

    private Rigidbody2D miRigidBody;
    private Animator miAnimator;
    private Server _server;
    void Start()
    {
        miRigidBody = GetComponent<Rigidbody2D>();
        miAnimator = GetComponent<Animator>();
        miRigidBody.AddForce(transform.up * jumpStrength);
        isActive = true;
        var tmpServer = GameObject.FindGameObjectsWithTag("server");
        if (tmpServer.Length > 0)
        {
            _server = tmpServer[0].GetComponent<Server>();
        }
        else 
        {
            _server = null;
        }
    }

    void FixedUpdate()
    {
        miRigidBody.AddForce(Vector3.down * gravStr);
        if (_server != null)
        {
            _server.SendBellPos(transform.position.x, transform.position.y, idCampana, bellStage, isActive);
        }
        miAnimator.SetInteger("BellStage", bellStage);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int idHitter = -1;
        switch (collision.tag)
        {
            case "bala":
                idHitter = collision.gameObject.GetComponent<BulletScript>().idBullet;
                BellIsHit(collision, idHitter);
                break;
            case "heart":
                idHitter = collision.gameObject.GetComponent<HeartScript>().idHeart;
                BellIsHit(collision, idHitter);
                break;
            case "jugador":
                isActive = false;
                gameObject.SetActive(false);
                if (_server != null)
                {
                    int idJugador = collision.gameObject.GetComponent<PlayerGameScript>().idJugador;
                    if (bellStage >= 0 && bellStage <= 5)
                    {
                        _server.UpdatePlayerPoints(idJugador, 1000);
                    }
                    else if (bellStage >= 5 && bellStage <= 10)
                    {
                        _server.UpdatePlayerPoints(idJugador, 1500);
                    }
                    else if (bellStage >= 10 && bellStage <= 15)
                    {
                        _server.UpdatePlayerPoints(idJugador, 2000);
                    }
                    else if (bellStage >= 15)
                    {
                        _server.UpdatePlayerPoints(idJugador, 5000);
                    }
                    _server.SendBellPos(transform.position.x, transform.position.y, idCampana, bellStage, isActive);
                }
                break;
            case "topCol":
                //The original game doesen't prevent you from throwing this out of the level and neither will i
                break;
            case "botCol":
                isActive = false;
                gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }

    private void BellIsHit(Collider2D collision, int idHitter)
    {
        collision.gameObject.SetActive(false);
        if (_server != null)
        {
            _server.DestroyPorjectile(idHitter, collision.tag);
        }
        miRigidBody.velocity = Vector3.zero;
        miRigidBody.AddForce(Vector3.up * jumpStrength / 2);
        bellStage++;
    }
}
