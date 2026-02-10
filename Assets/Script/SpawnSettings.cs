using UnityEngine;

[System.Serializable]
public class SpawnSettings
{
    public string PoolName;
    public Vector3 MinZone;
    public Vector3 MaxZone;
    public float SpawnInterval = 3f;
}