using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePlayerName : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField text;
    [SerializeField] private Button playButton;

    private const string PlayerPrefsNameKey = "PlayerName";

    private void Start()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
            return;
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        text.text = defaultName;
        NameChanged(defaultName);
    }
    public void NameChanged(string newName)
    {
        if (string.IsNullOrEmpty(newName))
            playButton.interactable = false;
        else
            playButton.interactable = true;
            PersistantInfo.Instance.PlayerName = newName;
    }
    public void SaveName()
    {
        PlayerPrefs.SetString(PlayerPrefsNameKey, text.text);
    }
}
