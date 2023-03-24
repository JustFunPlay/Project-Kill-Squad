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
    public SyncList<ScriptableWeapon> equipedWeapons = new SyncList<ScriptableWeapon>();
    [SerializeField] private TMPro.TextMeshProUGUI[] equipmentSlots;


    [ClientRpc] protected override void SetEquipmentNames()
    {
        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            equipmentSlots[i].text = equipedWeapons[i].weaponName;
        }
    }
    [Server] protected override void OnSelectAction()
    {
        switch (selectedAction)
        {
            case Action.Action1:
                if (equipedWeapons.Count >= 1)
                    GetRangeVisuals(equipedWeapons[0].range, true);
                break;
            case Action.Action2:
                if (equipedWeapons.Count >= 2)
                    GetRangeVisuals(equipedWeapons[1].range, true);
                break;
            case Action.Action3:
                if (equipedWeapons.Count >= 3)
                    GetRangeVisuals(equipedWeapons[2].range, true);
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
    protected CharacterBase CheckValidTarget(RaycastHit hit, ScriptableWeapon weapon)
    {
        CharacterBase target = null;
        if (hit.collider.GetComponent<CharacterBase>() && hit.collider.GetComponent<CharacterBase>().Owner != owner)
        {
            target = hit.collider.GetComponent<CharacterBase>();
        }
        else
        {
            GridCombatSystem.instance.grid.GetXZ(hit.point, out int gridX, out int gridZ);
            foreach (CharacterBase character in TurnTracker.instance.characters)
            {
                GridCombatSystem.instance.grid.GetXZ(character.transform.position, out int characterX, out int characterZ);
                if (gridX == characterX && gridZ == characterZ)
                {
                    target = character;
                }
            }
        }
        if (target == null || target.Owner == owner)
            return null;
        bool hasLos = false;
        for (int i = 0; i < 4; i++)
        {
            Vector3 startpos = transform.position + Vector3.up;
            if (i == 1)
                startpos += Vector3.forward * 0.95f;
            else if (i == 2)
                startpos += Vector3.back * 0.95f;
            else if (i == 3)
                startpos += Vector3.left * 0.95f;
            else
                startpos += Vector3.right * 0.95f;

            if (Physics.Raycast(startpos, (target.transform.position - startpos).normalized, Vector3.Distance(startpos, target.transform.position), GridCombatSystem.instance.obstacleLayer) == false)
            {
                hasLos = true;
                break;
            }
        }
        if (!hasLos)
            return null;
        GridCombatSystem.instance.grid.GetXZ(target.transform.position, out int targetX, out int targetZ);
        GridCombatSystem.instance.grid.GetXZ(transform.position, out int x, out int z);
        if ((weapon.type == WeaponType.Melee || weapon.type == WeaponType.Heavy || weapon.type == WeaponType.Swift) && Mathf.Abs(x - targetX) <= weapon.range && Mathf.Abs(z - targetZ) <= weapon.range)
        {
            return target;
        }
        else if (weapon.type != WeaponType.Melee)
        {
            if (weapon.type != WeaponType.Pistol && Mathf.Abs(x - targetX) <= 1 && Mathf.Abs(z - targetZ) <= 1)
                return null;
            List<GridNode> path = GridCombatSystem.instance.FindPath(x, z, targetX, targetZ);
            if (path != null && path.Count <= weapon.range + 1)
                return target;
        }
        return null;
    }


    #region Atacks
    [Server]
    protected void Attack(int accuracy, bool luckyAttack, int penetration, int crit, bool luckyCrit, int damage, CharacterBase target, out CombatReport report)
    {
        int hitRoll = Random.Range(0, 10);
        bool wound = false;
        bool critConfirm = false;
        int damageDealt = 0;
        report = new CombatReport();
        if (hitRoll < accuracy - target.Dodge)
        {
            report.attacksHit++;
            target.ArmorSave(penetration + apBoost, crit + critBoost, luckyCrit, damage + damageBoost, out wound, out critConfirm, out damageDealt, out bool killingBlow); ;
            if (wound)
            {
                report.armorPierced++;
                if (critConfirm)
                    report.critHits++;
                report.damageDealt = damageDealt;
            }
            if (killingBlow)
                report.killingBlows.Add(target);
            return;
        }
        else if (luckyAttack)
        {
            hitRoll = Random.Range(0, 10);
            if (hitRoll < accuracy - target.Dodge)
            {
                report.attacksHit++;
                target.ArmorSave(penetration + apBoost, crit + critBoost, luckyCrit, damage + damageBoost, out wound, out critConfirm, out damageDealt, out bool killingBlow);
                if (wound)
                {
                    report.armorPierced++;
                    if (critConfirm)
                        report.critHits++;
                    report.damageDealt = damageDealt;
                }
                if (killingBlow)
                    report.killingBlows.Add(target);
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
            Attack(Ranged, false, weapon.armorPenetration, weapon.crit, false, weapon.damage, target, out CombatReport newReport);
            CallForGunParticle(target.transform, newReport.attacksHit > 0);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows.AddRange(newReport.killingBlows);
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
            Attack(Ranged, false, weapon.armorPenetration, weapon.crit, false, weapon.damage, target, out CombatReport newReport);
            CallForGunParticle(target.transform, newReport.attacksHit > 0);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows.AddRange(newReport.killingBlows);
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
            CallForGunParticle(target.transform, newReport.attacksHit > 0);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows.AddRange(newReport.killingBlows);
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }
    [Server]
    protected void SpreadFire(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        bool isHalfRange = (GridCombatSystem.instance.FindPath(transform.position, target.transform.position).Count - 1) * 2 <= weapon.range;
        for (int i = 0; i < (isHalfRange ? weapon.attacks * 2 : weapon.attacks); i++)
        {
            report.totalAttackCount++;
            Attack(Ranged, false, isHalfRange ? weapon.armorPenetration - 2 : weapon.armorPenetration, weapon.crit, false, weapon.damage, target, out CombatReport newReport);
            CallForGunParticle(target.transform, newReport.attacksHit > 0);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows.AddRange(newReport.killingBlows);
                break;
            }
        }
        ReportForCombat(report);
    }

    [Server]
    protected IEnumerator StandardMelee(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < (weapon.type == WeaponType.Swift ? (Attacks + weapon.attacks) * 2 : Attacks + weapon.attacks); i++)
        {
            report.totalAttackCount++;
            Attack(Melee, LuckyMeleeAttack(), weapon.armorPenetration, weapon.crit, LuckyCrit(), weapon.damage, target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows = newReport.killingBlows;
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }
    [Server]
    protected IEnumerator HeavyMelee(ScriptableWeapon weapon, CharacterBase target)
    {
        CombatReport report = new CombatReport();
        for (int i = 0; i < Attacks + weapon.attacks; i++)
        {
            report.totalAttackCount++;
            Attack(Melee, LuckyMeleeAttack(), weapon.armorPenetration - 2, weapon.crit, LuckyCrit(), (int)(weapon.damage * 1.5f), target, out CombatReport newReport);
            report.attacksHit += newReport.attacksHit;
            report.armorPierced += newReport.armorPierced;
            report.critHits += newReport.critHits;
            report.damageDealt += newReport.damageDealt;
            if (newReport.killingBlows.Count > 0)
            {
                report.killingBlows.AddRange(newReport.killingBlows);
                break;
            }
            yield return new WaitForSeconds(0.15f);
        }
        ReportForCombat(report);
    }

    [Server]
    protected void GrenadeThrow(ScriptableGrenade grenade, int xLocation, int zLocation)
    {
        CombatReport report = new CombatReport();
        CallForGrenadeParticle(GridCombatSystem.instance.grid.GetWorldPosition(xLocation, zLocation));
        foreach (CharacterBase character in TurnTracker.instance.characters)
        {
            GridCombatSystem.instance.grid.GetXZ(character.transform.position, out int characterX, out int characterZ);
            if (Mathf.Abs(characterX - xLocation) <= 1 && Mathf.Abs(characterZ - zLocation) <= 1)
            {
                int count = Random.Range((int)grenade.attacks.x, (int)grenade.attacks.y);
                for (int i = 0; i < count; i++)
                {
                    report.totalAttackCount++;
                    Attack(Ranged, false, grenade.armorPenetration, grenade.crit, LuckyCrit(), grenade.damage, character, out CombatReport newReport);
                    report.attacksHit += newReport.attacksHit;
                    report.armorPierced += newReport.armorPierced;
                    report.critHits += newReport.critHits;
                    report.damageDealt += newReport.damageDealt;
                    if (newReport.killingBlows.Count > 0)
                    {
                        report.killingBlows.AddRange(newReport.killingBlows);
                        i = count;
                    }
                }
            }
        }
        ReportForCombat(report);
    }
    #endregion

    [ClientRpc] private void CallForGunParticle(Transform target, bool hit)
    {
        ParticleManager.instance.FireBullet(transform.position + Vector3.up * 1.5f, target.position, hit);
    }
    [ClientRpc] private void CallForGrenadeParticle(Vector3 origin)
    {
        ParticleManager.instance.GrenadeBlast(origin);
    }
}

public class CombatReport
{
    public int totalAttackCount;
    public int attacksHit;
    public int armorPierced;
    public int critHits;
    public int damageDealt;
    public List<CharacterBase> killingBlows = new List<CharacterBase>();
}