using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistantInfo : MonoBehaviour
{
    public static PersistantInfo Instance { get; private set; }
    public KillSquad squad;

    private string playerName;
    public string PlayerName { get { return playerName; } set { playerName = value; } }

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
