using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkZoneSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public NetworkObject objectToSpawn;
    public Vector3 minZone;
    public Vector3 maxZone;

    public float spawnInterval = 3f;
    public int minPlayersToStart = 2;

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
            if (NetworkManager.Singleton.ConnectedClients.Count >= minPlayersToStart)
            {
                StartSpawning();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void StartSpawning()
    {
        if (!IsServer || spawningStarted) return;

        spawningStarted = true;
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObject()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(minZone.x, maxZone.x),
            minZone.y,
            Random.Range(minZone.z, maxZone.z)
        );

        NetworkObject obj = Instantiate(objectToSpawn, spawnPos, Quaternion.identity);
        obj.Spawn();
    }
}
