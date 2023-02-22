using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class Apothecary : CharacterAttacks
{
    [SyncVar] [SerializeField] private ScriptableWeapon primaryWeapon;
    [SyncVar] [SerializeField] private ScriptableWeapon secondaryWeapon;
    [SyncVar] [SerializeField] private ScriptableWeapon meleeWeapon;
    [SyncVar] [SerializeField] private Vector2 healRange;
    [SyncVar] [SerializeField] private int remainingHealCharges;

    [Server]
    public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        ApothecaryData medicInfo = (ApothecaryData)info;
        primaryWeapon = medicInfo.primary;
        secondaryWeapon = medicInfo.sideArm;
        meleeWeapon = medicInfo.meleeWeapon;
        healRange = medicInfo.healValue;
        remainingHealCharges = medicInfo.healCharges;
        base.SetupCharacter(player, info);
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

    public override void PerformAction(RaycastHit hit, InGamePlayer player)
    {
        if (!canAct)
            return;
        CharacterBase target = null;
        switch (selectedAction)
        {
            case Action.Action1:
                if (performedActions.Contains(primaryWeapon.weaponName))
                    return;
                if (primaryWeapon.type == WeaponType.Combat && selectedVariant == ActionVar.Variant1)
                {
                    target = CheckValidTarget(hit, primaryWeapon);
                    if (target)
                    {
                        StartCoroutine(DoubleFire(primaryWeapon, target));
                        StartAction(2, primaryWeapon.weaponName);
                    }
                }
                else if (primaryWeapon.type == WeaponType.Combat && selectedVariant == ActionVar.Variant2)
                {
                    target = CheckValidTarget(hit, primaryWeapon);
                    if (target)
                    {
                        StartCoroutine(AimedFire(primaryWeapon, target));
                        StartAction(primaryWeapon.weaponName);
                    }
                }
                else
                {
                    target = CheckValidTarget(hit, primaryWeapon);
                    if (target)
                    {
                        StartCoroutine(NormalFire(primaryWeapon, target));
                        StartAction(primaryWeapon.weaponName);
                    }
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(secondaryWeapon.weaponName))
                    return;
                target = CheckValidTarget(hit, secondaryWeapon);
                if (target)
                {
                    StartAction(secondaryWeapon.weaponName);
                    SpreadFire(secondaryWeapon, target);
                }
                break;
            case Action.Action3:
                if (performedActions.Contains(meleeWeapon.weaponName))
                    return;
                target = CheckValidTarget(hit, meleeWeapon);
                if (target)
                {
                    StartCoroutine(StandardMelee(meleeWeapon, target));
                    StartAction(meleeWeapon.weaponName);
                }
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }
}
