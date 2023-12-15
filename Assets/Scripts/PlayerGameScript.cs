using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject heartPrefab;
    public int poolSize;
    public int poolSize2;
    public List<GameObject> poolBullets;
    public List<GameObject> poolHearts;

    void Start()
    {
        poolBullets = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < poolSize; i++)
        {
            tmp = Instantiate(bulletPrefab);
            tmp.SetActive(false);
            poolBullets.Add(tmp);
        }
        for (int i = 0; i < poolSize2; i++)
        {
            tmp = Instantiate(heartPrefab);
            tmp.SetActive(false);
            poolHearts.Add(tmp);
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
