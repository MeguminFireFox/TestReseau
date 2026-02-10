using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PoolManager : NetworkBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance => instance;

    [SerializeField] private List<PoolConfig> _poolConfig = new List<PoolConfig>();

    private Dictionary<string, Queue<NetworkObject>> pools = new Dictionary<string, Queue<NetworkObject>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var config in _poolConfig)
        {
            Queue<NetworkObject> queue = new Queue<NetworkObject>();

            for (int i = 0; i < config.PoolSize; i++)
            {
                NetworkObject obj = Instantiate(config.Prefab);
                obj.gameObject.SetActive(false);
                queue.Enqueue(obj);
            }

            pools[config.PoolName] = queue;
        }
    }

    public NetworkObject GetObject(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!IsServer || !pools.ContainsKey(poolName)) return null;

        Queue<NetworkObject> queue = pools[poolName];

        if (queue.Count == 0) return null;

        NetworkObject obj = queue.Dequeue();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        obj.Spawn(true);

        return obj;
    }

    public void ReturnToPool(string poolName, NetworkObject obj)
    {
        if (!IsServer || !pools.ContainsKey(poolName)) return;

        obj.Despawn(false);
        obj.gameObject.SetActive(false);

        pools[poolName].Enqueue(obj);
    }
}
