using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    [Header("Gun Particles")]
    [SerializeField] private GameObject bulletExample;
    List<GameObject> bullets = new List<GameObject>();
    int currentBulletIndex;

    [SerializeField] private OrbitalLazer laser;

    void Start()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
        for (int i = 0; i < 20; i++)
        {
            bullets.Add(Instantiate(bulletExample, transform.position, Quaternion.identity, transform));
            bullets[i].SetActive(false);
        }
    }

    public void FireBullet(Vector3 startpos, Vector3 endpos, bool hit = true)
    {
        if (currentBulletIndex >= bullets.Count)
            currentBulletIndex = 0;
        bullets[currentBulletIndex].SetActive(true);
        bullets[currentBulletIndex].transform.position = startpos;
        endpos += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(1.4f, 1.7f), Random.Range(-0.2f, 0.2f));
        if (!hit)
            endpos += new Vector3(RandomMissDir(), RandomMissDir(), RandomMissDir());
        bullets[currentBulletIndex].transform.LookAt(endpos, Vector3.up);
        StartCoroutine(ProjectBullet(currentBulletIndex, startpos, endpos));
        currentBulletIndex++;

    }
    IEnumerator ProjectBullet(int index, Vector3 startpoint, Vector3 endpoint)
    {
        //Vector3 dir = (endpoint - startpoint).normalized;
        float dst = Vector3.Distance(startpoint, endpoint);
        for (int i = 0; i < 10; i++)
        {
            bullets[index].transform.Translate(Vector3.forward * (dst / 10));
            yield return new WaitForSeconds(0.01f);
        }
        bullets[index].SetActive(false);
    }

    private int RandomMissDir()
    {
        int dir = Random.Range(0, 2);
        return dir == 1 ? dir : -1;
    }

    public void FireOrbitalLaser(Vector3 origin)
    {
        StartCoroutine(laser.FiringLazer(origin));
    }
}
