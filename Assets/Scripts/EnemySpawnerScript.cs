using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerScript : MonoBehaviour
{
    /*
     * Enemigo 0: Lechuga
     * Enemgio 1: Rollo Primavera
     * Enemigo 2: Calabaza
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
        List<GameObject> tmpList = new List<GameObject>();
        enemiesId = 0;
        for (int i = 0; i < cantPrefabs; i++)
        {
            for (int j = 0; j < poolSize; j++)
            {
                var tmp = Instantiate(prefabsEnemigos[i], mapaJuego);
                tmpList.Add(tmp);
                tmp.GetComponent<Enemigo1Script>().idEnemigo = enemiesId;
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
