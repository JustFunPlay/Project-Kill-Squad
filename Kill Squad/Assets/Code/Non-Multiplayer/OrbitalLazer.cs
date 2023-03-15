using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalLazer : MonoBehaviour
{
    [SerializeField] private Transform[] rotTargetingRight;
    [SerializeField] private Transform[] rotTargetingLeft;
    [SerializeField] private Transform bigLazer;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < rotTargetingLeft.Length; i++)
        {
            rotTargetingLeft[i].Rotate(0, -Random.Range(50, 180) * Time.deltaTime, 0);
        }
        for (int i = 0; i < rotTargetingRight.Length; i++)
        {
            rotTargetingRight[i].Rotate(0, Random.Range(50, 180) * Time.deltaTime, 0);
        }
        bigLazer.Rotate(0, 180 * Time.deltaTime, 0);
    }

    public IEnumerator FiringLazer(Vector3 origin)
    {
        transform.position = origin;
        for (int i = 0; i < rotTargetingLeft.Length; i++)
        {
            rotTargetingLeft[i].gameObject.SetActive(true);
        }
        for (int i = 0; i < rotTargetingRight.Length; i++)
        {
            rotTargetingRight[i].gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(0.8f);
        for (int i = 0; i < rotTargetingLeft.Length; i++)
        {
            rotTargetingLeft[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < rotTargetingRight.Length; i++)
        {
            rotTargetingRight[i].gameObject.SetActive(false);
        }
        bigLazer.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        bigLazer.gameObject.SetActive(false);
    }
}
