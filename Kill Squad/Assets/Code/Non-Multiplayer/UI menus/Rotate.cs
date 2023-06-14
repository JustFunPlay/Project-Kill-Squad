using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Rotation());
    }
    IEnumerator Rotation()
    {
        while (true)
        {
            transform.Rotate(0, 4.5f, 0);
            yield return new WaitForSeconds(0.05f);
        }
    }
}
