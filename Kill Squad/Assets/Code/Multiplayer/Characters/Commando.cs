using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class Commando : CharacterAttacks
{
    [Header("Equipment")]
    [SyncVar] [SerializeField] private ScriptableGrenade grenade;
    [SyncVar] [SerializeField] private int remainingGrenades;
    [SerializeField] private TMPro.TextMeshProUGUI grenadeName;
    [SerializeField] private TMPro.TextMeshProUGUI grenadeCount;

    [Header("Ult")]
    [SyncVar] [SerializeField] private int minUltHits;
    [SyncVar] [SerializeField] private int maxUltHits;
    [SyncVar] [SerializeField] private int ultAp;
    [SyncVar] [SerializeField] private int ultDamage;
    [SyncVar] [SerializeField] private int requiredDamageDealt;
    [SyncVar] [SerializeField] private int damageDealt;
    [SerializeField] private TMPro.TextMeshProUGUI ultProgress;

    [Server] public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        equipedWeapons.Clear();
        equipedWeapons.AddRange(info.equipedWeapons);
        CommandoData comInfo = (CommandoData)info;
        grenade = comInfo.grenade;
        remainingGrenades = comInfo.extraGrenades ? comInfo.grenade.count : comInfo.grenade.increasedCount;
        minUltHits = comInfo.minUltHits;
        maxUltHits = comInfo.maxUltHits;
        ultAp = comInfo.ultAp;
        ultDamage = comInfo.ultDamage;
        requiredDamageDealt = comInfo.requiredDamageDealt;
        damageDealt = 40;
        Invoke("UpdateUltProgress", 0.5f);
        Invoke("UpdateGrenadeCount", 0.5f);
        base.SetupCharacter(player, info);
    }

    [ClientRpc] protected override void SetEquipmentNames()
    {
        grenadeName.text = grenade.weaponName;
        base.SetEquipmentNames();
    }


    protected override void ReportForCombat(CombatReport report)
    {
        damageDealt += report.damageDealt;
        UpdateUltProgress();
        base.ReportForCombat(report);
    }
    [Server]
    protected override void OnSelectAction()
    {
        switch (selectedAction)
        {
            case Action.Action4:
                GetRangeVisuals(grenade.range, false);
                break;
            case Action.Ultimate:
                ClearRangeVisuals();
                break;
            default:
                base.OnSelectAction();
                break;
        }
    }

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// Called when the local player object is being stopped.
    /// <para>This happens before OnStopClient(), as it may be triggered by an ownership message from the server, or because the player object is being destroyed. This is an appropriate place to deactivate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStopLocalPlayer() {}

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnectionToClient parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion

    [Server] public override void PerformAction(RaycastHit hit, InGamePlayer player)
    {
        if (!canAct)
            return;
        CharacterBase target = null;
        switch (selectedAction)
        {
            case Action.Action1:
                if (performedActions.Contains(equipedWeapons[0].weaponName))
                    return;
                if (equipedWeapons[0].type == WeaponType.RapidFire && selectedVariant == ActionVar.Variant1)
                {
                    target = CheckValidTarget(hit, equipedWeapons[0]);
                    if (target)
                    {
                        StartAction(2, equipedWeapons[0].weaponName);
                        StartCoroutine(DoubleFire(equipedWeapons[0], target));
                    }
                }
                else
                {
                    target = CheckValidTarget(hit, equipedWeapons[0]);
                    if (target)
                    {
                        StartAction(equipedWeapons[0].weaponName);
                        StartCoroutine(NormalFire(equipedWeapons[0], target));
                    }
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(equipedWeapons[1].weaponName))
                    return;
                target = CheckValidTarget(hit, equipedWeapons[1]);
                if (target)
                {
                    StartAction(equipedWeapons[1].weaponName);
                    StartCoroutine(NormalFire(equipedWeapons[1], target));
                }
                break;
            case Action.Action3:
                if (performedActions.Contains(equipedWeapons[2].weaponName))
                    return;
                target = CheckValidTarget(hit, equipedWeapons[2]);
                if (target)
                {
                    StartAction(equipedWeapons[2].weaponName);
                    StartCoroutine(StandardMelee(equipedWeapons[2], target));
                }
                break;
            case Action.Action4:
                if (performedActions.Contains(grenade.weaponName) || remainingGrenades <= 0)
                    return;
                List<Vector3> grenadePath = GridCombatSystem.instance.FindPath(transform.position, hit.point);
                if (grenadePath != null && grenadePath.Count <= grenade.range + 1)
                {
                    remainingGrenades -= 1;
                    GridCombatSystem.instance.grid.GetXZ(hit.point, out int x, out int z);
                    StartAction(grenade.weaponName);
                    GrenadeThrow(grenade, x, z);
                    UpdateGrenadeCount();
                }
                break;
            case Action.Ultimate:
                if (damageDealt < requiredDamageDealt)
                    return;
                List<CharacterBase> targetsInUlt = new List<CharacterBase>();
                Vector3 ultOrigin = hit.point;
                foreach (CharacterBase character in TurnTracker.instance.characters)
                {
                    List<Vector3> ultpath = GridCombatSystem.instance.FindPath(ultOrigin, character.transform.position);
                    if (character.Owner != owner && ultpath != null && ultpath.Count <= 4)
                        targetsInUlt.Add(character);
                }
                StartAction();
                damageDealt = 0;
                UpdateUltProgress();
                GridCombatSystem.instance.grid.GetXZ(ultOrigin, out int laserX, out int laserZ);
                FireLaser(GridCombatSystem.instance.grid.GetWorldPosition(laserX, laserZ));
                StartCoroutine(UltBeam(targetsInUlt));
                break; 
            default:
                base.PerformAction(hit, player);
                break;
        }

    }

    [Server] private IEnumerator UltBeam(List<CharacterBase> targets)
    {

        yield return new WaitForSeconds(0.8f);
        int ultHits = Random.Range(minUltHits, maxUltHits + 1);
        CombatReport report = new CombatReport();
        for (int hit = 0; hit < ultHits; hit++)
        {
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].ArmorSave(ultAp, 0, false, ultDamage, out bool wound, out bool critConfirm, out int damageDealt, out bool killingBlow);
                report.totalAttackCount++;
                report.attacksHit++;
                if (wound)
                {
                    report.armorPierced++;
                    if (critConfirm)
                        report.critHits++;
                    report.damageDealt += damageDealt;
                }
                if (killingBlow)
                {
                    report.killingBlows.Add(targets[i]);
                    hasKilled = true;
                    targets.RemoveAt(i);
                    i--;
                }
            }
        }
        Debug.Log($"Total attacks: {report.totalAttackCount}\nHits: {report.attacksHit}\nWounds: {report.armorPierced}\nCrits: {report.critHits}\nTotal Damage: {report.damageDealt}\nKilling blow: {report.killingBlows.Count}");
        ContinueTurn();
    }

    [ClientRpc]
    private void UpdateUltProgress()
    {
        ultProgress.text = $"Progress:\n[{Mathf.Min(damageDealt, requiredDamageDealt)}/{requiredDamageDealt}]";
    }
    [ClientRpc]
    private void UpdateGrenadeCount()
    {
        grenadeCount.text = $"Remaining Grenades:\n{remainingGrenades}";
    }
    [ClientRpc] private void FireLaser(Vector3 target)
    {
        ParticleManager.instance.FireOrbitalLaser(target);
    }
}
