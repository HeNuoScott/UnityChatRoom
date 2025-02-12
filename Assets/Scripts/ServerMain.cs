using Network.Server;
using UnityEngine;

public class ServerMain : MonoBehaviour
{
    private void Start()
    {
        NetServer.BuildServer();
        NetServer.Instance.StartServer(25565, 50);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetServer.Instance.BroadCast("这是一条测试广播!!");
        }
    }
    private void OnDestroy()
    {
        NetServer.Instance.StopServer();
    }
}