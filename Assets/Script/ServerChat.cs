using System.Text;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class ServerChat : MonoBehaviour
{
    private static ServerChat instance = null;
    public static ServerChat Instance => instance;

    private NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    const ushort PORT = 7779;

    private bool activated = false;

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

    public void Host()
    {
        if (activated)
        {
            Debug.LogWarning("Serveur déjà lancé");
            return;
        }

        driver = NetworkDriver.Create();

        var endpoint = NetworkEndpoint.AnyIpv4;
        endpoint.Port = PORT;

        if (driver.Bind(endpoint) != 0)
        {
            Debug.LogError("Impossible de bind le port");
            return;
        }

        driver.Listen();
        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        Debug.Log("SERVEUR LANCÉ");
        activated = true;
    }

    void Update()
    {
        if (!activated) return;

        driver.ScheduleUpdate().Complete();

        ServerUpdate();
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

    void OnDestroy()
    {
        if (driver.IsCreated)
        {
            driver.Dispose();
        }

        if (connections.IsCreated)
        {
            connections.Dispose();
        }
    }
}
