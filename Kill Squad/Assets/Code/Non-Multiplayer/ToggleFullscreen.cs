using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleFullscreen : MonoBehaviour
{
    public void ToggleFs(bool fs)
    {
        Vector2 resolution = fs == true ? new Vector2(1280, 720) : new Vector2(1920, 1080);
        Screen.SetResolution((int)resolution.x, (int)resolution.y, fs);
    }
}
