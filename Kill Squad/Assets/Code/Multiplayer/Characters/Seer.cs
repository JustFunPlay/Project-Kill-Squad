using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class Seer : CharacterAttacks
{
    [Header("Psychic stuff")]
    [SyncVar] [SerializeField] private int maxPsychicPoints;
    [SyncVar] [SerializeField] private int currentPsychicPoints;
    [SyncVar] [SerializeField] private Vector2 psychicGeneration;

    [SyncVar] [SerializeField] private bool hasRunicArmor;
    [SyncVar] [SerializeField] private bool runicArmorActve;
    [SerializeField] private TMPro.TextMeshProUGUI ppCounter;


    [Server]
    public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        equipedWeapons.Clear();
        equipedWeapons.AddRange(info.equipedWeapons);
        SeerData seerInfo = (SeerData)info;
        currentPsychicPoints = maxPsychicPoints = seerInfo.psychicPoints;
        psychicGeneration = seerInfo.psychicGeneration;
        hasRunicArmor = seerInfo.hasRunicArmor;
        UpdatePsychicPoints();
        UpdatePsychicPoints();
        base.SetupCharacter(player, info);
    }

    [Server] public override void ProgressTurn()
    {
        CheckRunicArmor();
        base.ProgressTurn();
    }
    protected override void EndTurn()
    {
        if (!canAct)
            return;
        CheckRunicArmor();
        base.EndTurn();
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
                if (performedActions.Contains(equipedWeapons[0].weaponName))
                    return;
                target = CheckValidTarget(hit, equipedWeapons[0]);
                if (target)
                {
                    StartAction(equipedWeapons[0].weaponName);
                    StartCoroutine(NormalFire(equipedWeapons[0], target));
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(equipedWeapons[1].weaponName))
                    return;
                if (equipedWeapons[1].type == WeaponType.Heavy && selectedVariant == ActionVar.Variant1 && currentPsychicPoints >= 4)
                {
                    target = CheckValidTarget(hit, equipedWeapons[1]);
                    if (target)
                    {
                        currentPsychicPoints -= 4;
                        UpdatePsychicPoints();
                        StartAction(equipedWeapons[1].weaponName);
                        StartCoroutine(HeavyMelee(equipedWeapons[1], target));
                    }
                }
                else
                {
                    target = CheckValidTarget(hit, equipedWeapons[1]);
                    if (target)
                    {
                        StartAction(equipedWeapons[1].weaponName);
                        StartCoroutine(StandardMelee(equipedWeapons[1], target));
                    }
                }
                break;
            case Action.Action3:
                if (currentPsychicPoints == maxPsychicPoints)
                    return;
                currentPsychicPoints = Mathf.Min(currentPsychicPoints + Random.Range((int)psychicGeneration.x, (int)psychicGeneration.y), maxPsychicPoints);
                UpdatePsychicPoints();
                StartAction();
                ContinueTurn();
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }
    [ClientRpc] private void UpdatePsychicPoints()
    {
        ppCounter.text = $"Psychic points:\n[{currentPsychicPoints}/{maxPsychicPoints}]";
    }

    [Server] private void CheckRunicArmor()
    {
        if (!hasRunicArmor)
            return;
        if (runicArmorActve)
        {
            armorSave -= 1;
            damageReduction -= 1;
            runicArmorActve = false;
        }
        if (currentPsychicPoints > 0)
        {
            currentPsychicPoints--;
            armorSave += 1;
            damageReduction += 1;
            runicArmorActve = true;
        }
        UpdatePsychicPoints();
    }
}
