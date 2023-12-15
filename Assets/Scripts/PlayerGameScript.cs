using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public int poolSize;
    public List<GameObject> poolBullets;

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
}
