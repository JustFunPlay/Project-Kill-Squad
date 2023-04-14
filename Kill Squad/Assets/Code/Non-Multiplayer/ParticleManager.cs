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
    [Header("Grenade Particle")]
    [SerializeField] private ParticleSystem grenadeBlast;

    [Header("Commando Ult")]
    [SerializeField] private OrbitalLazer laser;

    [Header("Hitman Ult")]
    [SerializeField] private LineRenderer targetLine;
    [SerializeField] private Transform railRound;

    [Header("Arc Trooper")]
    [SerializeField] private LineRenderer teslaExample;
    private List<LineRenderer> teslaLines = new List<LineRenderer>();
    int teslaIndex;
    [SerializeField] private Transform[] empSpheres;


    void Start()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
        for (int i = 0; i < 20; i++)
        {
            bullets.Add(Instantiate(bulletExample, transform.position, Quaternion.identity, transform));
            bullets[i].SetActive(false);
            teslaLines.Add(Instantiate(teslaExample, transform.position, Quaternion.identity, transform));
            teslaLines[i].gameObject.SetActive(false);
        }
    }

    public void FireBullet(Vector3 startpos, Vector3 endpos, bool hit = true)
    {
        if (currentBulletIndex >= bullets.Count)
            currentBulletIndex = 0;
        bullets[currentBulletIndex].transform.position = startpos;
        bullets[currentBulletIndex].GetComponentInChildren<TrailRenderer>().Clear();
        bullets[currentBulletIndex].SetActive(true);
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

    public void GrenadeBlast(Vector3 origin)
    {
        grenadeBlast.transform.position = origin;
        grenadeBlast.Play();
    }

    public void FireOrbitalLaser(Vector3 origin)
    {
        StartCoroutine(laser.FiringLazer(origin));
    }
    
    public void FireRailRound(Vector3 origin, Vector3 target)
    {
        origin += Vector3.up * 1.5f;
        target += Vector3.up * 1.5f;
        targetLine.SetPosition(0, origin);
        targetLine.SetPosition(1, target);
        targetLine.gameObject.SetActive(true);
        StartCoroutine(BurnRailRound(origin, target));
    }

    IEnumerator BurnRailRound(Vector3 origin, Vector3 target)
    {
        railRound.position = origin;
        railRound.LookAt(target, Vector3.up);
        yield return new WaitForSeconds(0.8f);
        targetLine.gameObject.SetActive(false);
        railRound.gameObject.SetActive(true);
        railRound.GetComponentInChildren<TrailRenderer>().Clear();
        for (int i = 0; i < 100; i++)
        {
            railRound.Translate(Vector3.forward * 10);
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1.5f);
        railRound.gameObject.SetActive(false);
    }

    public void TeslaShock(Vector3 origin, Vector3 target)
    {
        if (teslaIndex >= teslaLines.Count)
            teslaIndex = 0;
        origin += Vector3.up * 1.5f;
        target += Vector3.up * 1.5f + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
        teslaLines[teslaIndex].SetPosition(0, origin);
        teslaLines[teslaIndex].SetPosition(1, target);
        teslaLines[teslaIndex].gameObject.SetActive(true);
        StartCoroutine(RemoveTeslaLine(teslaIndex));
        teslaIndex++;
    }
    IEnumerator RemoveTeslaLine(int index)
    {
        yield return new WaitForSeconds(0.2f);
        teslaLines[index].gameObject.SetActive(false);
    }
    public void EMP(Vector3 origin)
    {
        for (int i = 0; i < empSpheres.Length; i++)
        {
            empSpheres[i].transform.position = origin + Vector3.up * 1.5f;
            empSpheres[i].localScale = Vector3.one;
            empSpheres[i].gameObject.SetActive(true);
            StartCoroutine(EmpFired(i));
        }
    }

    IEnumerator EmpFired(int index)
    {
        yield return new WaitForSeconds(0.06f * index);
        for (int i = 0; i < 22; i++)
        {
            empSpheres[index].localScale += new Vector3(0.5f, 0.15f, 0.5f);
            yield return new WaitForSeconds(0.01f);

        }
        yield return new WaitForSeconds(0.03f);
        empSpheres[index].gameObject.SetActive(false);
    }
}
