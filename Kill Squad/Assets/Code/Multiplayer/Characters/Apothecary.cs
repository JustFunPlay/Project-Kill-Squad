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
    [SyncVar] [SerializeField] private int remainingHealCharges;
    [SerializeField] private TMPro.TextMeshProUGUI chargeCounter;

    [Header("Ult")]
    [SyncVar] [SerializeField] private bool ultCharged;
    [SerializeField] private TMPro.TextMeshProUGUI ultChargeText;

    [Server]
    public override void SetupCharacter(InGamePlayer player, List<int> Loadout)
    {
        equipedWeapons.Clear();
        for (int i = 0; i < 3; i++)
        {
            equipedWeapons.Add(Loadout[i]);
        }
        Invoke("UpdateHealCharges", 0.5f);
        Invoke("ShowUltCharge", 0.5f);
        base.SetupCharacter(player, Loadout);
    }
    [Server]
    protected override void ReportForCombat(CombatReport report)
    {
        for (int i = 0; i < report.killingBlows.Count; i++)
        {
            if (report.killingBlows[i].hasKilled)
                ultCharged = true;
        }
        ShowUltCharge();
        base.ReportForCombat(report);
    }
    [Server]protected override void OnSelectAction()
    {
        switch(selectedAction)
        {
            case Action.Action4:
                GetRangeVisuals(2, true);
                break;
            case Action.Ultimate:
                GetRangeVisuals(2, true);
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
                if (charInfo.weaponOptions[equipedWeapons[0]].type == WeaponType.Combat && selectedVariant == ActionVar.Variant1 && remainingActions >= 2)
                {
                    target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[0]]);
                    if (target)
                    {
                        StartAction(2, charInfo.weaponOptions[equipedWeapons[0]].weaponName);
                        StartCoroutine(DoubleFire(charInfo.weaponOptions[equipedWeapons[0]], target));
                    }
                }
                else if (charInfo.weaponOptions[equipedWeapons[0]].type == WeaponType.Combat && selectedVariant == ActionVar.Variant2 && remainingActions >= 2)
                {
                    target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[0]]);
                    if (target)
                    {
                        StartAction(2, charInfo.weaponOptions[equipedWeapons[0]].weaponName);
                        StartCoroutine(AimedFire(charInfo.weaponOptions[equipedWeapons[0]], target));
                    }
                }
                else
                {
                    target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[0]]);
                    if (target)
                    {
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
                    StartAction(charInfo.weaponOptions[equipedWeapons[1]].weaponName);
                    SpreadFire(charInfo.weaponOptions[equipedWeapons[1]], target);
                }
                break;
            case Action.Action3:
                if (performedActions.Contains(charInfo.weaponOptions[equipedWeapons[2]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedWeapons[2]]);
                if (target)
                {
                    StartAction(charInfo.weaponOptions[equipedWeapons[2]].weaponName);
                    StartCoroutine(StandardMelee(charInfo.weaponOptions[equipedWeapons[2]], target));
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
                List<Vector3> path = GridCombatSystem.instance.FindPath(transform.position, target.transform.position, false);
                if (path != null && path.Count <= 3)
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

                        if (Physics.Raycast(startpos, (target.transform.position + Vector3.up * 1.5f - startpos).normalized, Vector3.Distance(startpos, target.transform.position), GridCombatSystem.instance.obstacleLayer) == false)
                        {
                            hasLos = true;
                            break;
                        }
                    }
                    if (!hasLos)
                        return;
                    StartAction();
                    ApothecaryData medicInfo = (ApothecaryData)charInfo;
                    int healvalue = Random.Range((int)medicInfo.healValue.x, (int)medicInfo.healValue.y);
                    target.GetHealed(healvalue, out int healingDone);
                    remainingHealCharges--;
                    UpdateHealCharges();
                    ContinueTurn();
                }
                break;
            case Action.Ultimate:
                if (ultCharged == false)
                    return;
                if (hit.collider.GetComponent<CharacterBase>() && hit.collider.GetComponent<CharacterBase>().Owner == owner && hit.collider.GetComponent<CharacterBase>().Health < 0)
                {
                    target = hit.collider.GetComponent<CharacterBase>();
                }
                else
                {
                    GridCombatSystem.instance.grid.GetXZ(hit.point, out int gridX, out int gridZ);
                    foreach (CharacterBase character in TurnTracker.instance.deadCharacters)
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
                List<Vector3> ultPath = GridCombatSystem.instance.FindPath(transform.position, target.transform.position, false);
                if (ultPath != null && ultPath.Count <= 3)
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

                        if (Physics.Raycast(startpos, (target.transform.position + Vector3.up * 1.5f - startpos).normalized, Vector3.Distance(startpos, target.transform.position), GridCombatSystem.instance.obstacleLayer) == false)
                        {
                            hasLos = true;
                            break;
                        }
                    }
                    if (!hasLos)
                        return;
                    StartAction();
                    target.GetRessurected();
                    ultCharged = false;
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
    [ClientRpc]
    private void ShowUltCharge()
    {
        if (ultCharged)
            ultChargeText.text = "Ult Charged";
        else
            ultChargeText.text = "Requires Charging";
    }
}
