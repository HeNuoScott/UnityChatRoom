using Network.Server;
using UnityEngine;

public class ServerMain : MonoBehaviour
{
    private void Start()
    {
        NetServer.BuildServer();
        NetServer.Instance.StartServer("0.0.0.0", 25565);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetServer.Instance.BroadCast("����һ�����Թ㲥!!");
        }
    }
    private void OnDestroy()
    {
        NetServer.Instance.StopServer();
    }


}