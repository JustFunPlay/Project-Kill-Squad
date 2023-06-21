using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class Hitman : CharacterAttacks
{
    [Header("Ult")]
    [SyncVar] [SerializeField] private int currentCrits;
    [SerializeField] private TMPro.TextMeshProUGUI ultProgress;

    [Server]
    public override void SetupCharacter(InGamePlayer player, List<int> Loadout)
    {
        equipedWeapons.Clear();
        for (int i = 0; i < 3; i++)
        {
            equipedWeapons.Add(Loadout[i]);
        }
        currentCrits = 0;
        Invoke("UpdateUltProgress", 0.5f);
        ChangeEquippedWeapon(equipedWeapons[0]);
        base.SetupCharacter(player, Loadout);
    }

    [Server]
    protected override void ReportForCombat(CombatReport report)
    {
        currentCrits += report.critHits;
        UpdateUltProgress();
        base.ReportForCombat(report);
    }
    [Server]
    protected override void OnSelectAction()
    {
        switch (selectedAction)
        {
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

    [Server]
    public override void PerformAction(RaycastHit hit, InGamePlayer player)
    {
        if (!canAct)
            return;
        CharacterBase target = null;
        switch (selectedAction)
        {
            case Action.Action1:
                if (performedActions.Contains(charInfo.weaponOptions[equipedWeapons[0]].weaponName))
                    return;
                if (charInfo.weaponOptions[equipedWeapons[0]].type == WeaponType.Precision && selectedVariant == ActionVar.Variant1 && remainingActions >= 2)
                {
                    target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[0]]);
                    if (target)
                    {
                        AimGun();
                        StartAction(2, charInfo.weaponOptions[equipedWeapons[0]].weaponName);
                        StartCoroutine(AimedFire(charInfo.weaponOptions[equipedWeapons[0]], target));
                    }
                }
                else
                {
                    target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[0]]);
                    if (target)
                    {
                        AimGun();
                        StartAction(charInfo.weaponOptions[equipedWeapons[0]].weaponName);
                        StartCoroutine(NormalFire(charInfo.weaponOptions[equipedWeapons[0]], target));
                    }
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(charInfo.weaponOptions[equipedWeapons[1]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[1]]);
                if (target)
                {
                    ChangeEquippedWeapon(equipedWeapons[1]);
                    AimPistol();
                    StartAction(charInfo.weaponOptions[equipedWeapons[1]].weaponName);
                    StartCoroutine(NormalFire(charInfo.weaponOptions[equipedWeapons[1]], target));
                }
                break;
            case Action.Action3:
                if (performedActions.Contains(charInfo.weaponOptions[equipedWeapons[2]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[2]]);
                if (target)
                {
                    ChangeEquippedWeapon(equipedWeapons[2]);
                    GrabKnife();
                    StartAction(charInfo.weaponOptions[equipedWeapons[2]].weaponName);
                    StartCoroutine(StandardMelee(charInfo.weaponOptions[equipedWeapons[2]], target));
                }
                break;
            case Action.Ultimate:
                HitmanData hitInfo = (HitmanData)charInfo;
                if (currentCrits < hitInfo.requiredCrits)
                    return;
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
                    return;
                StartAction();
                currentCrits = 0;
                ChangeEquippedWeapon(4);
                StartCoroutine(PerformUlt(target));
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }

    [Server] IEnumerator PerformUlt(CharacterBase target)
    {
        transform.LookAt(target.transform.position);
        HitmanData hitInfo = (HitmanData)charInfo;
        UltAnim();
        yield return new WaitForSeconds(1.5f);
        ShowUlt(target.transform.position);
        yield return new WaitForSeconds(.75f);
        DoAttack();
        Attack(Ranged + 10, false, -10, 19, false, hitInfo.ultDamage, target, out CombatReport newReport);
        newReport.totalAttackCount++;
        yield return new WaitForSeconds(2f);
        ReportForCombat(newReport);
    }
    [ClientRpc] void UltAnim()
    {
        animationController.SetTrigger("Railgun");
    }
    [ClientRpc] void ShowUlt(Vector3 target)
    {
        Vector3 firepos = currentFirePoint != null ? currentFirePoint.position : transform.position + Vector3.up * 1.5f;
        ParticleManager.instance.FireRailRound(firepos, target);
    }


    [ClientRpc] private void UpdateUltProgress()
    {
        HitmanData hitInfo = (HitmanData)charInfo;
        ultProgress.text = $"Progress:\n[{Mathf.Min(currentCrits, hitInfo.requiredCrits)}/{hitInfo.requiredCrits}]";
    }

    [ClientRpc]
    private void AimGun()
    {
        animationController.SetTrigger("Aim");
    }
    [ClientRpc]
    private void AimPistol()
    {
        animationController.SetTrigger("AimPistol");
    }
    [ClientRpc]
    private void GrabKnife()
    {
        animationController.SetTrigger("Knife");
    } 
}
