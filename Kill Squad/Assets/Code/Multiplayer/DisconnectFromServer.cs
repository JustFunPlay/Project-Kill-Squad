using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DisconnectFromServer : NetworkBehaviour
{
    public void Disconnect()
    {
        if (isServer)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }
}
