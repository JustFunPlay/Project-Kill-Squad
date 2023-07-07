using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideCeilings : MonoBehaviour
{
    private Transform player;
    [SerializeField] private Transform[] areaPoints;
    [SerializeField] private float checkRadius;
    [SerializeField] private GameObject[] stuffToHide;

    private void Start()
    {
        StartCoroutine(CheckForRevealing());
    }

    IEnumerator CheckForRevealing()
    {
        yield return new WaitForSeconds(0.5f);
        player = FindObjectOfType<InGamePlayer>().transform;
        while (true)
        {
            bool needToHide = false;
            for (int i = 0; i < areaPoints.Length; i++)
            {
                if (Vector3.Distance(areaPoints[i].position, player.position) <= checkRadius)
                    needToHide = true;
            }
            if (needToHide)
                HideStuff();
            else
                ShowStuff();
            yield return new WaitForFixedUpdate();
        }
    }

    void HideStuff()
    {
        for (int i = 0; i < stuffToHide.Length; i++)
        {
            stuffToHide[i].SetActive(false);
        }
    }
    void ShowStuff()
    {
        for (int i = 0; i < stuffToHide.Length; i++)
        {
            stuffToHide[i].SetActive(true);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < areaPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(areaPoints[i].position, checkRadius);
        }
    }
}
