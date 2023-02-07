using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class ServerNameChange : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField inputField;

    void Start()
    {
        inputField.text = NetworkManager.singleton.networkAddress;
    }

    public void ChangeServerName(string serverName)
    {
        NetworkManager.singleton.networkAddress = serverName;
    }
}
