using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeScreenRatio : MonoBehaviour
{
    [SerializeField] Vector2[] resolutions;
    [SerializeField] int resolutionIndex;
    [SerializeField] bool isFullScreen;

    public void ToggleFS(bool fs)
    {
        isFullScreen = fs;
        ChangeResolution();
    }
    public void ChangeIndex(int index)
    {
        resolutionIndex = index;
        ChangeResolution();
    }
    public void ChangeResolution()
    {
        Screen.SetResolution((int)resolutions[resolutionIndex].x, (int)resolutions[resolutionIndex].y, isFullScreen);
    }
}
