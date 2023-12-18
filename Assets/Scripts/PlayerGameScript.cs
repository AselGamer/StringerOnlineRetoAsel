using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameScript : MonoBehaviour
{
    public int idJugador;

    public GameObject bulletPrefab;
    public GameObject heartPrefab;
    public int poolSize;
    public int poolSize2;
    public List<GameObject> poolBullets;
    public List<GameObject> poolHearts;

    private int bulletsId = 0;
    private int heartsId = 0;

    void Start()
    {
        poolBullets = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < poolSize; i++)
        {
            tmp = Instantiate(bulletPrefab);
            tmp.SetActive(false);
            tmp.GetComponent<BulletScript>().idJugSim = idJugador;
            tmp.GetComponent<BulletScript>().idBullet = bulletsId;
            poolBullets.Add(tmp);
            bulletsId++;
        }
        for (int i = 0; i < poolSize2; i++)
        {
            tmp = Instantiate(heartPrefab);
            tmp.SetActive(false);
            tmp.GetComponent<HeartScript>().idHeart = heartsId;
            poolHearts.Add(tmp);
            heartsId++;
        }
    }

    void Update()
    {

    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!poolBullets[i].activeInHierarchy)
            {
                return poolBullets[i];
            }
        }
        return null;
    }

    public GameObject GetPooledObjectHeart()
    {
        for (int i = 0; i < poolSize2; i++)
        {
            if (!poolHearts[i].activeInHierarchy)
            {
                return poolHearts[i];
            }
        }
        return null;
    }
}
