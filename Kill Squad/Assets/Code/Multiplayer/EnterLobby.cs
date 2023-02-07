using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnterLobby : MonoBehaviour
{
    LobbyManager lobbyManager;

    private void Start()
    {
        lobbyManager = (LobbyManager)NetworkManager.singleton;
    }

    public void MakeLobby()
    {
        lobbyManager.StartHost();
    }
    public void JoinLobby()
    {
        lobbyManager.StartClient();
    }
}
