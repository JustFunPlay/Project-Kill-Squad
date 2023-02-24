using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimHpBar : MonoBehaviour
{
    Transform lookAt;
    private void Start()
    {
        StartCoroutine(LookToCam());
    }
    IEnumerator LookToCam()
    {
        yield return new WaitForSeconds(0.5f);
        lookAt = GetComponentInParent<CharacterBase>().Owner.GetComponentInChildren<Camera>().transform;
        while (true) 
        {
            transform.LookAt(lookAt, Vector3.up);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
