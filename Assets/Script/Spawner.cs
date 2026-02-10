using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    [SerializeField] private List<SpawnSettings> _spawnSettings = new List<SpawnSettings>();
    [SerializeField] private int _minPlayersToStart = 2;

    private bool spawningStarted = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        StartCoroutine(CheckPlayersRoutine());
    }

    IEnumerator CheckPlayersRoutine()
    {
        while (!spawningStarted)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count >= _minPlayersToStart)
            {
                StartSpawning();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void StartSpawning()
    {
        if (spawningStarted) return;

        spawningStarted = true;

        foreach (var setting in _spawnSettings)
        {
            StartCoroutine(SpawnRoutine(setting));
        }
    }

    IEnumerator SpawnRoutine(SpawnSettings setting)
    {
        while (true)
        {
            SpawnObject(setting);
            yield return new WaitForSeconds(setting.SpawnInterval);
        }
    }

    void SpawnObject(SpawnSettings setting)
    {
        Vector3 spawnPos = new Vector3(Random.Range(setting.MinZone.x, setting.MaxZone.x), setting.MinZone.y, Random.Range(setting.MinZone.z, setting.MaxZone.z));

        PoolManager.Instance.GetObject(setting.PoolName, spawnPos, Quaternion.identity);
    }
}
