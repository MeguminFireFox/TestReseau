using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameEnd : NetworkBehaviour
{
    private static GameEnd instance = null;
    public static GameEnd Instance => instance;

    [SerializeField] private int _goal = 10;

    [SerializeField] private TMP_Text _endGameText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    public void CheckScore(int playerScore, ulong playerId)
    {
        if (!IsServer) return;

        if (playerScore >= _goal)
        {
            EndGameClientRpc(playerId);
        }
    }

    [ClientRpc]
    private void EndGameClientRpc(ulong winnerId)
    {
        if (!_endGameText) return;

        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            _endGameText.text = "Win !";
            _endGameText.color = Color.green;
        }
        else
        {
            _endGameText.text = "Loose...";
            _endGameText.color = Color.red;
        }

        _endGameText.gameObject.SetActive(true);
    }
}
