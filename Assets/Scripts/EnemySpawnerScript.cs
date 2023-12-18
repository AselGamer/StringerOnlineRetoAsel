using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    /*
     * Enemigo 0: Lechuga
     * Enemigo 1: Nube
     * Enemgio 2: Rollo Primavera
     * Enemigo 3: Calabaza
     */
    public GameObject[] prefabsEnemigos;
    public int poolSize;

    public Transform mapaJuego;

    private Dictionary<string, List<GameObject>> poolpoolEnemigos;
    public List<GameObject> spawnPoints;

    private int enemiesId;

    void Start()
    {
        int cantPrefabs = prefabsEnemigos.Length;
        spawnPoints = new List<GameObject>();
        poolpoolEnemigos = new Dictionary<string, List<GameObject>>();
        enemiesId = 0;
        for (int i = 0; i < cantPrefabs; i++)
        {
            List<GameObject> tmpList = new List<GameObject>();
            for (int j = 0; j < poolSize; j++)
            {
                var tmp = Instantiate(prefabsEnemigos[i], mapaJuego);
                tmpList.Add(tmp);
                switch (i)
                {
                    case 1:
                        tmp.GetComponent<NubeScript>().idNube = enemiesId;
                        break;
                    case 0:
                        tmp.GetComponent<Enemigo1Script>().idEnemigo = enemiesId;
                        break;
                    default:
                        break;
                }
                tmp.SetActive(false);
                enemiesId++;
            }
            poolpoolEnemigos.Add("spawnEnemigo" + i, tmpList);
        }
    }

    void Update()
    {
        foreach (GameObject p in spawnPoints)
        {
            if (p.GetComponent<SpriteRenderer>().isVisible)
            {
                var tmpEnemigo = GetPooledObject(p.tag);
                tmpEnemigo.transform.localPosition= p.transform.localPosition;
                tmpEnemigo.SetActive(true);
                p.SetActive(false);
            }
        }
    }

    private GameObject GetPooledObject(string idEnemigo)
    {
        var poolId = poolpoolEnemigos[idEnemigo];
        int poolIdSize = poolId.Count;
        for (int i = 0; i < poolIdSize; i++)
        {
            if (!poolId[i].activeInHierarchy)
            {
                return poolId[i];
            }
        }
        return null;
    }

    public void findSpawners()
    {
        int cantPrefabs = prefabsEnemigos.Length;
        for (int i = 0; i < cantPrefabs; i++)
        {
            spawnPoints.AddRange(GameObject.FindGameObjectsWithTag("spawnEnemigo" + i));
        }
    }

}
