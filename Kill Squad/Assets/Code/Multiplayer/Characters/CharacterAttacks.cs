using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class CharacterAttacks : CharacterMovement
{
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
    #region Atacks
    [Server]
    private void Attack(int accuracy, bool luckyAttack, int penetration, int crit, bool luckyCrit, int damage, CharacterBase target, out CombatReport report)
    {
        int hitRoll = Random.Range(0, 10);
        bool wound = false;
        bool critConfirm = false;
        int damageDealt = 0;
        report = new CombatReport();
        if (hitRoll < accuracy - target.Dodge)
        {
            report.attacksHit++;
            target.ArmorSave(penetration, crit, luckyCrit, damage, out wound, out critConfirm, out damageDealt, out report.killingBlow);
            if (wound)
            {
                report.armorPierced++;
                if (critConfirm)
                    report.critHits++;
                report.damageDealt = damageDealt;
            }
            return;
        }
        else if (luckyAttack)
        {
            hitRoll = Random.Range(0, 10);
            if (hitRoll < accuracy - target.Dodge)
            {
                report.attacksHit++;
                target.ArmorSave(penetration, crit, luckyCrit, damage, out wound, out critConfirm, out damageDealt, out report.killingBlow);
                if (wound)
                {
                    report.armorPierced++;
                    if (critConfirm)
                        report.critHits++;
                    report.damageDealt = damageDealt;
                }
                return;
            }
        }
    }
    [Server]
    protected IEnumerator NormalFire(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < weapon.attacks; i++)
        {
            report.totalAttackCount++;
            Attack(Ranged, LuckyRangedAttack(), weapon.armorPenetration, weapon.crit, LuckyCrit(), weapon.damage, target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlow)
            {
                report.killingBlow = true;
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }
    [Server]
    protected IEnumerator DoubleFire(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < weapon.attacks * 2; i++)
        {
            report.totalAttackCount++;
            Attack(Ranged, LuckyRangedAttack(), weapon.armorPenetration, weapon.crit, LuckyCrit(), weapon.damage, target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlow)
            {
                report.killingBlow = true;
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }
    [Server]
    protected IEnumerator AimedFire(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < weapon.attacks; i++)
        {
            report.totalAttackCount++;
            Attack(Ranged, true, weapon.armorPenetration, weapon.crit, true, weapon.damage, target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlow)
            {
                report.killingBlow = true;
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }

    #endregion
}

public class CombatReport
{
    public int totalAttackCount;
    public int attacksHit;
    public int armorPierced;
    public int critHits;
    public int damageDealt;
    public bool killingBlow;
}