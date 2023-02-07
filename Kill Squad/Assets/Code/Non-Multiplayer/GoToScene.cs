using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToScene : MonoBehaviour
{
    [SerializeField] private int sceneToGo;

    public void MoveToScene(int scene = -1)
    {
        SceneManager.LoadScene(scene == -1 ? sceneToGo : scene);
    }
}
