using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistantInfo : MonoBehaviour
{
    public static PersistantInfo Instance { get; private set; }

    private string playerName;
    public string PlayerName => playerName;

    public void SetPlayerName(string name)
    {
        playerName = name;
    }

    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}
