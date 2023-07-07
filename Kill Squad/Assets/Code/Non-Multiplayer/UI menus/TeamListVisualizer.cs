using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamListVisualizer : MonoBehaviour
{
    public void RemoveCharacter()
    {
        LoadoutEditor editor = FindObjectOfType<LoadoutEditor>();
        editor.RemoveCharacter(gameObject);
    }
}
