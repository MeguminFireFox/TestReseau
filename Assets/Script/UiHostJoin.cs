using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UiHostJoin : MonoBehaviour
{
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        DeactivateButtons();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        DeactivateButtons();
    }

    void DeactivateButtons()
    {
        _startHostButton.interactable = false;
        _startClientButton.interactable = false;
    }
}
