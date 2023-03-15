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
    [Header("Equipment")]
    [SyncVar] [SerializeField] private Vector2 healRange;
    [SyncVar] [SerializeField] private int remainingHealCharges;
    [SerializeField] private TMPro.TextMeshProUGUI chargeCounter;

    [Server]
    public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        equipedWeapons.Clear();
        equipedWeapons.AddRange(info.equipedWeapons);
        ApothecaryData medicInfo = (ApothecaryData)info;
        healRange = medicInfo.healValue;
        remainingHealCharges = medicInfo.healCharges;
        UpdateHealCharges();
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
                if (performedActions.Contains(equipedWeapons[0].weaponName))
                    return;
                if (equipedWeapons[0].type == WeaponType.Combat && selectedVariant == ActionVar.Variant1)
                {
                    target = CheckValidTarget(hit, equipedWeapons[0]);
                    if (target)
                    {
                        StartAction(2, equipedWeapons[0].weaponName);
                        StartCoroutine(DoubleFire(equipedWeapons[0], target));
                    }
                }
                else if (equipedWeapons[0].type == WeaponType.Combat && selectedVariant == ActionVar.Variant2)
                {
                    target = CheckValidTarget(hit, equipedWeapons[0]);
                    if (target)
                    {
                        StartAction(2, equipedWeapons[0].weaponName);
                        StartCoroutine(AimedFire(equipedWeapons[0], target));
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
                    SpreadFire(equipedWeapons[1], target);
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
                if (performedActions.Contains("Medkit") || remainingHealCharges <= 0)
                    return;
                if (hit.collider.GetComponent<CharacterBase>() && hit.collider.GetComponent<CharacterBase>().Owner == owner)
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
                if (target == null || target.Owner != owner)
                    return;
                GridCombatSystem.instance.grid.GetXZ(target.transform.position, out int targetX, out int targetZ);
                GridCombatSystem.instance.grid.GetXZ(transform.position, out int x, out int z);
                List<GridNode> path = GridCombatSystem.instance.FindPath(x, z, targetX, targetZ);
                if (path != null && path.Count <= 3)
                {
                    StartAction();
                    int healvalue = Random.Range((int)healRange.x, (int)healRange.y);
                    target.GetHealed(healvalue, out int healingDone);
                    remainingHealCharges--;
                    UpdateHealCharges();
                    ContinueTurn();
                }
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }

    [ClientRpc] private void UpdateHealCharges()
    {
        chargeCounter.text = $"Charges: {remainingHealCharges}";
    }
}
