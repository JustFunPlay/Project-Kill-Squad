using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dontdestroymusic : MonoBehaviour
{
   void awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
}
