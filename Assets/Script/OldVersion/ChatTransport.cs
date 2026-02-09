using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using TMPro;
using System.Text;

public class ChatTransport : MonoBehaviour
{
    [SerializeField] private bool isServer;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField _textInputField;
    [SerializeField] private GameObject _panelPseudo;
    [SerializeField] private TMP_InputField _pseudoInputField;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    private NetworkConnection serverConnection;

    private bool isConnected = false;
    private bool canChoice = true;
    private string pseudo = "NoName";


    const ushort PORT = 7777;

    public void Host()
    {
        if (!canChoice) return;

        driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = PORT;

        if (driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Impossible de bind le port");
            return;
        }

        isServer = true;
        driver.Listen();
        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        Debug.Log("SERVEUR LANCÉ");
        canChoice = false;
        _panelPseudo.SetActive(true);
    }

    public void Connect()
    {
        if (!canChoice) return;

        driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.Parse("127.0.0.1", PORT);

        if (driver.Connect(endpoint) != default)
        {
            Debug.LogError("Impossible de se connecter au serveur");
            return;
        }

        serverConnection = driver.Connect(endpoint);

        isServer = false;

        Debug.Log("CLIENT CONNECTÉ");
        canChoice= false;
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
        if (canChoice) return;

        driver.ScheduleUpdate().Complete();

        if (isServer)
            ServerUpdate();
        else
            ClientUpdate();
    }

    void ServerUpdate()
    {
        // Accepter connexions
        NetworkConnection c;
        while ((c = driver.Accept()) != default)
        {
            connections.Add(c);
            Debug.Log("Client connecté");
        }

        // Nettoyage
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }

        // Lecture messages
        for (int i = 0; i < connections.Length; i++)
        {
            DataStreamReader stream;
            NetworkEvent.Type cmd;

            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    int length = stream.ReadInt();
                    byte[] bytes = new byte[length];
                    stream.ReadBytes(bytes);

                    string msg = Encoding.UTF8.GetString(bytes);
                    chatText.text += "\n" + msg;

                    Broadcast(msg);
                }
            }
        }
    }

    void Broadcast(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated) continue;

            driver.BeginSend(connections[i], out DataStreamWriter writer);
            writer.WriteInt(bytes.Length);
            writer.WriteBytes(bytes);
            driver.EndSend(writer);
        }
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
                chatText.text += "\n" + msg;
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

        string msg = pseudo + ":" +_textInputField.text;
        byte[] bytes = Encoding.UTF8.GetBytes(msg);

        if (isServer)
        {
            // Le serveur envoie à tous les clients
            Broadcast(msg);
        }
        else
        {
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
        }

        chatText.text += "\n" + msg;
        _textInputField.text = "";
    }

    void OnDestroy()
    {
        if (driver.IsCreated)
            driver.Dispose();

        if (connections.IsCreated)
            connections.Dispose();
    }
}
