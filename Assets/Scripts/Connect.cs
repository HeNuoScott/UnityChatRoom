using Network.Client;
using UnityEngine.UI;
using UnityEngine;

public class Connect : MonoBehaviour
{
    //服务器设置输入
    public InputField hostInput;
    public InputField portInput;

    public Text tipText;
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(ConnectToServer);
    }

    private void Update()
    {
        if (NetClient.Instance.Session.State == SessionState.Run)
        {
            this.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    private void ConnectToServer()
    {
        if (NetClient.Instance.Session.State == SessionState.Connect)
            return;

        tipText.text = "正在连接服务器...";
        button.gameObject.SetActive(false);
        NetClient.Instance.Connect(hostInput.text, int.Parse(portInput.text));
    }
}
