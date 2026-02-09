using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerCoinCollector : NetworkBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TMP_Text scoreText;

    private NetworkVariable<int> score = new NetworkVariable<int>(0);

    void Start()
    {
        // Mettre à jour le TextMeshPro dès le début
        UpdateScoreText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return; // Seul le propriétaire collecte

        if (other.CompareTag("Coin"))
        {
            // On dit au serveur de gérer la collecte
            CollectCoinServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    [ServerRpc]
    private void CollectCoinServerRpc(ulong coinNetworkId)
    {
        // Récupérer l'objet NetworkObject correspondant
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(coinNetworkId, out NetworkObject coinObj))
        {
            // Détruire le coin sur tous les clients
            coinObj.Despawn(true);
        }

        // Ajouter un point au joueur
        score.Value += 1;

        // Mettre à jour le score sur tous les clients
        UpdateScoreClientRpc(score.Value);
    }

    [ClientRpc]
    private void UpdateScoreClientRpc(int newScore)
    {
        scoreText.text = $"Score: {newScore}";
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score.Value}";
    }
}
