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
            //transform.LookAt(new Vector3(lookAt.position.x, transform.position.y, lookAt.position.z), Vector3.up);
            transform.rotation = lookAt.rotation;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
