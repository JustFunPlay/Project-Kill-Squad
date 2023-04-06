using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class Infiltrator : CharacterAttacks
{
    [Header("Ult")]
    [SyncVar] [SerializeField] private int ultDuration;

    [SyncVar] [SerializeField] private int invisibleDuration;
    [SyncVar] [SerializeField] private bool canGoInvisible;
    [SerializeField] private GameObject[] invisibleObjects;
    [SerializeField] private TMPro.TextMeshProUGUI ultChargeText;

    [Server]
    public override void SetupCharacter(InGamePlayer player, List<int> selectedEquipmentIndexes)
    {
        //equipedIndexes.AddRange(new List<int>(2));
        //equipedIndexes = new SyncList<int>(new List<int>(2));
        equipedIndexes.Clear();
        for (int i = 0; i < 2; i++)
        {
            equipedIndexes.Add(selectedEquipmentIndexes[i]);
        }
        canGoInvisible = true;
        Invoke("ShowUltCharge", 0.5f);
        base.SetupCharacter(player, selectedEquipmentIndexes);
    }

    [Server] public override void PrepareTurn()
    {
        if (invisibleDuration > 0)
            invisibleDuration--;
        if (invisibleDuration == 0)
            ExitInvisible();
        base.PrepareTurn();
    }

    [Server] protected override void ReportForCombat(CombatReport report)
    {
        if (report.killingBlows.Count > 0)
            canGoInvisible = true;
        ShowUltCharge();
        base.ReportForCombat(report);
    }
    [Server]
    protected override void OnSelectAction()
    {
        switch (selectedAction)
        {
            case Action.Ultimate:
                if (canGoInvisible)
                {
                    GoInvisible();
                    canGoInvisible = false;
                    ShowUltCharge();
                }
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
                if (performedActions.Contains(charInfo.weaponOptions[equipedIndexes[0]].weaponName) && (performedActions.Contains($"{ charInfo.weaponOptions[equipedIndexes[0]].weaponName}2") || charInfo.weaponOptions[equipedIndexes[0]].weaponName.Contains("Twin") == false))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedIndexes[0]]);
                if (target)
                {
                    if (performedActions.Contains(charInfo.weaponOptions[equipedIndexes[0]].weaponName) && charInfo.weaponOptions[equipedIndexes[0]].weaponName.Contains("Twin"))
                        StartAction($"{ charInfo.weaponOptions[equipedIndexes[0]].weaponName}2");
                    else
                        StartAction(charInfo.weaponOptions[equipedIndexes[0]].weaponName);
                    if (invisibleDuration > 0)
                        ExitInvisible();
                    StartCoroutine(NormalFire(charInfo.weaponOptions[equipedIndexes[0]], target));
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(charInfo.weaponOptions[equipedIndexes[1]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedIndexes[1]]);
                if (target)
                {
                    StartAction(charInfo.weaponOptions[equipedIndexes[1]].weaponName);
                    if (invisibleDuration > 0)
                        ExitInvisible();
                    StartCoroutine(StandardMelee(charInfo.weaponOptions[equipedIndexes[1]], target));
                }
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }
    [Server] private void GoInvisible()
    {
        invisibleDuration = ultDuration;
        movementModifier += 1;
        dodgeChance += 10;
        ToggleInvisible(false);

    }
    [Server] private void ExitInvisible()
    {
        invisibleDuration = -1;
        movementModifier -= 1;
        dodgeChance -= 10;
        RecieveBuff(StatChange.Melee, 1, 1, true);
        RecieveBuff(StatChange.Ranged, 1, 1, true);
        RecieveBuff(StatChange.Attacks, 1, 1, true);
        RecieveBuff(StatChange.Crit, 1, 1, true);
        RecieveBuff(StatChange.Ap, -1, 1, true);
        ToggleInvisible(true);
    }
    [ClientRpc] private void ToggleInvisible(bool active)
    {
        if (owner.isOwned)
            return;
        for (int i = 0; i < invisibleObjects.Length; i++)
        {
            invisibleObjects[i].SetActive(active);
        }
    }
    
    [ClientRpc] private void ShowUltCharge()
    {
        if (canGoInvisible)
            ultChargeText.text = "Ult Charged";
        else
            ultChargeText.text = "Requires Charging";
    }
}
