using Unity.Netcode;

[System.Serializable]
public class PoolConfig
{
    public string PoolName;
    public NetworkObject Prefab;
    public int PoolSize = 10;
}