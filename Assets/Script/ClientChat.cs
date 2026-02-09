using System.Text;
using TMPro;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

public class ClientChat : MonoBehaviour
{
    [SerializeField] private TMP_Text _chatText;
    [SerializeField] private TMP_InputField _textInputField;
    [SerializeField] private GameObject _panelPseudo;
    [SerializeField] private TMP_InputField _pseudoInputField;
    [SerializeField] private Scrollbar _scrollbar;
    [SerializeField] private RectTransform _contentRect;

    private NetworkDriver driver;
    private NetworkConnection serverConnection;

    private bool isConnected = false;
    private string pseudo = "NoName";

    const ushort PORT = 7779;

    public void Connect()
    {
        driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.Parse("127.0.0.1", PORT);

        serverConnection = driver.Connect(endpoint);

        if (!serverConnection.IsCreated)
        {
            Debug.LogError("Impossible de se connecter au serveur");
            return;
        }

        Debug.Log("Tentative de connexion envoyée");
        _panelPseudo.SetActive(true);
    }

    public void SetPseudo()
    {
        if (string.IsNullOrEmpty(_pseudoInputField.text))
            return;
        pseudo = _pseudoInputField.text;
        _pseudoInputField.text = "";
        _panelPseudo.SetActive(false);
    }

    void Update()
    {
        if (!driver.IsCreated)
            return;

        driver.ScheduleUpdate().Complete();
        ClientUpdate();
    }

    void ClientUpdate()
    {
        if (!serverConnection.IsCreated)
            return;

        DataStreamReader stream;
        NetworkEvent.Type cmd;

        while ((cmd = serverConnection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                isConnected = true;
                Debug.Log("Connexion au serveur confirmée");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                int length = stream.ReadInt();
                byte[] bytes = new byte[length];
                stream.ReadBytes(bytes);

                string msg = Encoding.UTF8.GetString(bytes);
                _chatText.text += "\n" + msg;

                RecivedMessage(msg);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                isConnected = false;
                Debug.Log("Déconnecté du serveur");
                serverConnection = default;
            }
        }
    }

    public void SendMessage()
    {
        if (string.IsNullOrEmpty(_textInputField.text))
            return;

        string msg = pseudo + " : " + _textInputField.text;
        byte[] bytes = Encoding.UTF8.GetBytes(msg);

        // Le client envoie au serveur
        if (!isConnected)
        {
            Debug.LogWarning("Serveur pas prêt");
            return;
        }

        if (driver.BeginSend(serverConnection, out DataStreamWriter writer) == 0)
        {
            writer.WriteInt(bytes.Length);
            writer.WriteBytes(bytes);
            driver.EndSend(writer);
        }

        _textInputField.text = "";
    }

    private void RecivedMessage(string msg)
    {
        Canvas.ForceUpdateCanvases();

        float preferredHeight = _chatText.preferredHeight;
        _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, _contentRect.rect.height + preferredHeight);

        _scrollbar.value = 0;
    }

    void OnDestroy()
    {
        if (driver.IsCreated)
        {
            driver.Dispose();
        }
    }
}
