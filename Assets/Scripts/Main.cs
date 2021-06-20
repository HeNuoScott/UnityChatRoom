using System.Collections.Generic;
using Network.CommonData;
using Network.Client;
using UnityEngine.UI;
using UnityEngine;
using System;

public class Main : MonoBehaviour
{
    //消息输入
    public InputField messageInput;

    //面板消息
    public Text serverText;
    public Text clientText;
    public Text messageTextPrefab;
    public RectTransform messageContent;

    //最大显示消息数
    public int maxMessageCount = 20;

    //消息间隔
    public float messageSpace = 5.0f;

    //消息预设
    private List<Text> messagesText;

    private void Start()
    {
        NetClient.Instance.ActionAddListener(ActionTypeEnum.MessageAction, CreateMessage);
        NetClient.Instance.ActionAddListener(ActionTypeEnum.InformAction, CreateMessage);
        messagesText = new List<Text>();
    }

    private void Update()
    {
        switch (NetClient.Instance.ClientState)
        {
            case SessionState.None:
                break;
            case SessionState.Connecting:
                serverText.text = "";
                clientText.text = "正在连接中...";
                break;
            case SessionState.Connected:
                serverText.text = NetClient.Instance.RemoteAddress;
                clientText.text = NetClient.Instance.LocalAddress;
                break;
            case SessionState.Close:
                serverText.text = "";
                clientText.text = "服务器连接断开，正在重连中...";
                break;
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    public void Send()
    {
        ActionParameter parameter = new ActionParameter();
        parameter[NetConfig.MESSAGE] = messageInput.text;
        try
        {
            NetClient.Instance.SendAction(ActionTypeEnum.MessageAction, parameter);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
        messageInput.text = "";
    }

    /// <summary>
    /// 清除消息列表
    /// </summary>
    public void MessageListClear()
    {
        if (messagesText.Count > 0)
        {
            for (int i = messagesText.Count - 1; i > 0; i--)
            {
                Destroy(messagesText[i].gameObject);
                messagesText.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 创建消息UI
    /// </summary>
    private void CreateMessage(ActionParameter parameter)
    {
        if (messageContent == null || messageTextPrefab == null) return;

        Text messageText = Instantiate(messageTextPrefab);
        messageText.text = parameter.GetValue<string>(NetConfig.MESSAGE);
        messageText.rectTransform.SetParent(messageContent);
        messageText.gameObject.SetActive(true);
        if (messagesText.Count >= maxMessageCount)
        {
            Destroy(messagesText[0].gameObject);
            messagesText.RemoveAt(0);
        }
        messagesText.Add(messageText);
    }
}
