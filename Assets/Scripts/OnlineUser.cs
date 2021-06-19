using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using Network.Client;
using Network.CommonData;

public class OnlineUser : MonoBehaviour
{
    public RectTransform onlinePanel;
    public Text onlineListUI;

    void Start()
    {
        NetClient.Instance.Action.AddListener(ActionTypeEnum.OnlineAction, UserList);
        // 发送上线请求
        StartCoroutine(SyncOnlineListTask());
    }

    void Update()
    {
        if (NetClient.Instance.Session.State != SessionState.Run && onlinePanel.gameObject.activeSelf)
        {
            onlinePanel.gameObject.SetActive(false);
        }
        else if (NetClient.Instance.Session.State == SessionState.Run && !onlinePanel.gameObject.activeSelf)
        {
            onlinePanel.gameObject.SetActive(true);
        }
    }

    private void UserList(ActionParameter parameter)
    {
        List<string> onlineList = parameter.GetValue<List<string>>(NetConfig.OnlineList);
        string localAddress = NetClient.Instance.Session.GetLocalAddress();
        onlineListUI.text = localAddress + "\r\n";
        for (int i = 0; i < onlineList.Count; i++)
        {
            if (localAddress != onlineList[i])
            {
                onlineListUI.text += onlineList[i] + "\r\n";
            }
        }
    }

    private IEnumerator SyncOnlineListTask()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (NetClient.Instance.Session.State == SessionState.Run)
            {
                try
                {
                    NetClient.Instance.Session.SendAction(ActionTypeEnum.OnlineAction, null);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }
    }
}
