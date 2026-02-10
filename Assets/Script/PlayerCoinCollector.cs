using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerCoinCollector : NetworkBehaviour
{
    [SerializeField] private TMP_Text _scoreText;

    private NetworkVariable<int> score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += OnScoreChanged;
        OnScoreChanged(0, score.Value);
    }

    private void OnScoreChanged(int oldValue, int newValue)
    {
        _scoreText.text = $"Score: {newValue}";
    }

    public void TakeDamage(int amount)
    {
        if (!IsServer) return;

        score.Value = Mathf.Max(0, score.Value - amount);
        GameEnd.Instance.CheckScore(score.Value, OwnerClientId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Coin"))
        {
            CollectCoinServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ServerRpc]
    private void CollectCoinServerRpc(ulong coinNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(coinNetworkId, out NetworkObject coinObj))
        {
            PoolManager.Instance.ReturnToPool("CoinPool", coinObj);
        }

        score.Value += 1;
        GameEnd.Instance.CheckScore(score.Value, OwnerClientId);
    }
}
