using Network.NetServer;
using UnityEngine;

public class ServerMain : MonoBehaviour
{
    private void Start()
    {
        Server server = new Server();
        server.StartServer("0.0.0.0", 25565);
    }

}
