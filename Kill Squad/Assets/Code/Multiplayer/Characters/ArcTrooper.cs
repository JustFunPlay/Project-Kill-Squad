using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class ArcTrooper : CharacterAttacks
{
    [Header("Tesla Coil")]
    [SyncVar] [SerializeField] int storedPower;

    public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        equipedWeapons.Clear();
        equipedWeapons.AddRange(info.equipedWeapons);
        //ArcTrooperData arcInfo = (ArcTrooperData)info;
        base.SetupCharacter(player, info);
    }
    [Server]
    public override void ProgressTurn()
    {
        TeslaCoilDamage();
        base.ProgressTurn();
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
    public override void OnStopLocalPlayer() { }

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

    [Server]
    public override void PerformAction(RaycastHit hit, InGamePlayer player)
    {
        if (!canAct)
            return;
        CharacterBase target = null;
        switch (selectedAction)
        {
            case Action.Action1:
                if (performedActions.Contains(equipedWeapons[0].weaponName))
                    return;
                target = CheckValidTarget(hit, equipedWeapons[0]);
                if (target)
                {
                    StartAction(equipedWeapons[0].weaponName);
                    StartCoroutine(TeslaFire(equipedWeapons[0], target));
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(equipedWeapons[1].weaponName))
                    return;
                target = CheckValidTarget(hit, equipedWeapons[1]);
                if (target)
                {
                    StartAction(equipedWeapons[1].weaponName);
                    StartCoroutine(TeslaMelee(equipedWeapons[1], target));
                }
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }

    [Server] private void TeslaCharge(int chargeAmmount)
    {
        storedPower += chargeAmmount;
        int healvalue = 0;
        while (storedPower >= 2)
        {
            storedPower -= 2;
            healvalue += 1;
        }
        if (healvalue > 0)
            GetHealed(healvalue, out int healingDone);
    }

    [Server] private void TeslaCoilDamage()
    {
        GridCombatSystem.instance.grid.GetXZ(transform.position, out int currentX, out int currentZ);
        int teslaDamageDealt = 0;
        foreach (CharacterBase character in TurnTracker.instance.characters)
        {
            if (character.Owner == owner)
                continue;
            GridCombatSystem.instance.grid.GetXZ(character.transform.position, out int characterX, out int characterZ);
            if (Mathf.Abs(characterX - currentX) <= 1 && Mathf.Abs(characterZ - currentZ) <= 1)
            {
                bool hasLos = false;
                for (int i = 0; i < 5; i++)
                {
                    Vector3 startpos = transform.position + Vector3.up * 1.5f;
                    if (i == 1 && !Physics.Raycast(startpos, Vector3.forward, 0.95f, GridCombatSystem.instance.obstacleLayer))
                        startpos += Vector3.forward * 0.95f;
                    else if (i == 2 && !Physics.Raycast(startpos, Vector3.back, 0.95f, GridCombatSystem.instance.obstacleLayer))
                        startpos += Vector3.back * 0.95f;
                    else if (i == 3 && !Physics.Raycast(startpos, Vector3.left, 0.95f, GridCombatSystem.instance.obstacleLayer))
                        startpos += Vector3.left * 0.95f;
                    else if (i == 4 && !Physics.Raycast(startpos, Vector3.right, 0.95f, GridCombatSystem.instance.obstacleLayer))
                        startpos += Vector3.right * 0.95f;

                    if (Physics.Raycast(startpos, (character.transform.position + Vector3.up * 1.5f - startpos).normalized, Vector3.Distance(startpos, character.transform.position), GridCombatSystem.instance.obstacleLayer) == false)
                        hasLos = true;
                }
                if (!hasLos)
                    continue;
                character.TakeDamage(1, true, out int damageDealt, out bool killingBlow);
                teslaDamageDealt++;
                if (killingBlow)
                    hasKilled = true;
            }
        }
        Debug.Log($"Tesla coil damage: {teslaDamageDealt}");
        if (teslaDamageDealt > 0)
            TeslaCharge(teslaDamageDealt);
    }

    [Server]
    protected IEnumerator TeslaFire(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < weapon.attacks; i++)
        {
            report.totalAttackCount++;
            //Attack(Ranged, false, weapon.armorPenetration, weapon.crit, false, weapon.damage, target, out CombatReport newReport);
            //CallForGunParticle(target.transform, newReport.attacksHit > 0);
            int hitRoll = Random.Range(0, 10);
            int damageDealt = 0;
            bool killingBlow = false;
            if (hitRoll < rangedSkill - target.Dodge)
            {
                report.attacksHit++;
                target.TakeDamage(weapon.damage, true, out damageDealt, out killingBlow);
                report.armorPierced++;
                report.damageDealt += damageDealt;
                TeslaCharge(damageDealt);
                if (killingBlow)
                    report.killingBlows.Add(target);
            }
            else if (LuckyRangedAttack())
            {
                hitRoll = Random.Range(0, 10);
                if (hitRoll < rangedSkill - target.Dodge)
                {
                    report.attacksHit++;
                    target.TakeDamage(weapon.damage, true, out damageDealt, out killingBlow);
                    TeslaCharge(damageDealt);
                    report.armorPierced ++;
                    report.damageDealt += damageDealt;
                    if (killingBlow)
                        report.killingBlows.Add(target);
                }
            }
            if (killingBlow)
            {
                i += 100;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }
    [Server]
    protected IEnumerator TeslaMelee(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < (weapon.type == WeaponType.Swift ? (Attacks + weapon.attacks) * 2 : Attacks + weapon.attacks); i++)
        {
            report.totalAttackCount++;
            Attack(Melee, LuckyMeleeAttack(), weapon.armorPenetration, weapon.crit, LuckyCrit(), weapon.damage, target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            if (newReport.attacksHit >0 && newReport.killingBlows.Count == 0)
            {
                target.TakeDamage(2, true, out int teslaDamage, out bool teslaKill);
                report.damageDealt += teslaDamage;
                TeslaCharge(teslaDamage);
                if (teslaKill)
                    newReport.killingBlows.Add(target);
            }
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows = newReport.killingBlows;
                break;
            }
            yield return new WaitForSeconds(0.2f);
        }
        ReportForCombat(report);
    }
}