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
    [SyncVar] [SerializeField] private int currentPsychicPoints;
    [SerializeField] private TMPro.TextMeshProUGUI ppCounter;


    [SyncVar] [SerializeField] private bool hasRunicArmor;
    [SyncVar] [SerializeField] private bool runicArmorActve;
    [SyncVar] [SerializeField] private int discipline1;
    [SerializeField] private TMPro.TextMeshProUGUI[] disipline1Text;
    [SyncVar] [SerializeField] private int discipline2;
    [SerializeField] private TMPro.TextMeshProUGUI[] disipline2Text;


    [Header("Ult")]
    [SyncVar] [SerializeField] private int pointsSpent;
    [SerializeField] private TMPro.TextMeshProUGUI ultCounter;

    [Server]
    public override void SetupCharacter(InGamePlayer player, CharacterInfoBase info, int[] selectedEquipmentIndexes)
    {
        equipedIndexes = new SyncList<int>(new List<int>(2));
        //equipedIndexes.AddRange(new List<int>(2));
        for (int i = 0; i < equipedIndexes.Count; i++)
        {
            equipedIndexes[i] = selectedEquipmentIndexes[i];
        }
        discipline1 = selectedEquipmentIndexes[2];
        discipline2 = selectedEquipmentIndexes[3];
        if (selectedEquipmentIndexes[4] == 1)
            hasRunicArmor = true;
        currentPsychicPoints = ((SeerData)info).psychicPoints;
        pointsSpent = 0;
        Invoke("UpdatePsychicPoints", 0.5f);
        Invoke("UpdateUltPoints", 0.5f);
        base.SetupCharacter(player, info, selectedEquipmentIndexes);
    }
    [ClientRpc]
    protected override void SetEquipmentNames()
    {
        disipline1Text[0].text = ((SeerData)charInfo).disciplines[discipline1].power1Name;
        disipline1Text[1].text = ((SeerData)charInfo).disciplines[discipline1].power2Name;
        disipline1Text[2].text = ((SeerData)charInfo).disciplines[discipline1].power3Name;
        disipline2Text[0].text = ((SeerData)charInfo).disciplines[discipline2].power1Name;
        disipline2Text[1].text = ((SeerData)charInfo).disciplines[discipline2].power2Name;
        disipline2Text[2].text = ((SeerData)charInfo).disciplines[discipline2].power3Name;
        base.SetEquipmentNames();
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

    [Server]
    protected override void OnSelectAction()
    {
        switch (selectedAction)
        {
            case Action.Action3:
                if (currentPsychicPoints == ((SeerData)charInfo).psychicPoints)
                    return;
                currentPsychicPoints = Mathf.Min(currentPsychicPoints + Random.Range((int)((SeerData)charInfo).psychicGeneration.x, (int)((SeerData)charInfo).psychicGeneration.y), ((SeerData)charInfo).psychicPoints);
                UpdatePsychicPoints();
                StartAction();
                ContinueTurn();
                break;
            case Action.Action4:
                GetRangeVisuals(((SeerData)charInfo).psychicRange, true);
                break;
            case Action.Action5:
                GetRangeVisuals(((SeerData)charInfo).psychicRange, true);
                break;
            case Action.Ultimate:
                GetRangeVisuals(((SeerData)charInfo).ultRange, true);
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
                if (performedActions.Contains(charInfo.weaponOptions[equipedIndexes[0]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedIndexes[0]]);
                if (target)
                {
                    StartAction(charInfo.weaponOptions[equipedIndexes[0]].weaponName);
                    StartCoroutine(NormalFire(charInfo.weaponOptions[equipedIndexes[0]], target));
                }
                break;
            case Action.Action2:
                if (performedActions.Contains(charInfo.weaponOptions[equipedIndexes[1]].weaponName))
                    return;
                target = CheckValidTarget(hit, charInfo.weaponOptions[equipedIndexes[1]]);
                if (target == null)
                    return;
                if (charInfo.weaponOptions[equipedIndexes[1]].type == WeaponType.Heavy && selectedVariant == ActionVar.Variant1 && currentPsychicPoints >= 4)
                {
                    currentPsychicPoints -= 4;
                    pointsSpent += 4;
                    UpdatePsychicPoints();
                    UpdateUltPoints();
                    StartAction(charInfo.weaponOptions[equipedIndexes[1]].weaponName);
                    StartCoroutine(HeavyMelee(charInfo.weaponOptions[equipedIndexes[1]], target));
                }
                else
                {
                    StartAction(charInfo.weaponOptions[equipedIndexes[1]].weaponName);
                    StartCoroutine(StandardMelee(charInfo.weaponOptions[equipedIndexes[1]], target));
                }
                break;
            case Action.Action4:
                target = FindPsychicTarget(hit, ((SeerData)charInfo).psychicRange);
                if (target == null)
                    return;
                if (selectedVariant == ActionVar.Normal)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline1].power1Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline1].power1Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline1].PerformPsychicPower1(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline1].power1Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline1].power1Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline1].power1Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline1].power1Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();
                    }
                }
                else if (selectedVariant == ActionVar.Variant1)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline1].power2Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline1].power2Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline1].PerformPsychicPower2(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline1].power2Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline1].power2Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline1].power2Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline1].power2Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();
                    }
                }
                else if (selectedVariant == ActionVar.Variant2)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline1].power3Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline1].power3Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline1].PerformPsychicPower3(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline1].power3Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline1].power3Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline1].power3Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline1].power3Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();
                    }
                }
                break;
            case Action.Action5:
                target = FindPsychicTarget(hit, ((SeerData)charInfo).psychicRange);
                if (target == null)
                    return;
                if (selectedVariant == ActionVar.Normal)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline2].power1Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline2].power1Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline2].PerformPsychicPower1(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline2].power1Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline2].power1Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline2].power1Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline2].power1Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();
                    }
                }
                else if (selectedVariant == ActionVar.Variant1)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline2].power2Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline2].power2Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline2].PerformPsychicPower2(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline2].power2Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline2].power2Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline2].power2Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline2].power2Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();
                    }
                }
                else if (selectedVariant == ActionVar.Variant2)
                {
                    if (currentPsychicPoints < ((SeerData)charInfo).disciplines[discipline2].power3Cost || performedActions.Contains(((SeerData)charInfo).disciplines[discipline2].power3Name))
                        return;
                    if (((SeerData)charInfo).disciplines[discipline2].PerformPsychicPower3(this, target))
                    {
                        StartAction(((SeerData)charInfo).disciplines[discipline2].power3Name);
                        currentPsychicPoints -= ((SeerData)charInfo).disciplines[discipline2].power3Cost;
                        pointsSpent += ((SeerData)charInfo).disciplines[discipline2].power3Cost;
                        Debug.Log($"Cast {((SeerData)charInfo).disciplines[discipline2].power3Name} on {target.name}");
                        UpdatePsychicPoints();
                        UpdateUltPoints();
                        ContinueTurn();

                    }
                }
                break;
            case Action.Ultimate:
                if (pointsSpent < ((SeerData)charInfo).requiredUltPoints)
                    return;
                target = FindPsychicTarget(hit, ((SeerData)charInfo).ultRange);
                if (target == null || target.Owner != owner)
                    return;
                StartAction();
                target.AddTurn();
                pointsSpent = 0;
                ContinueTurn();
                break;
            default:
                base.PerformAction(hit, player);
                break;
        }
    }
    [ClientRpc] private void UpdatePsychicPoints()
    {
        ppCounter.text = $"Psychic points:\n[{currentPsychicPoints}/{((SeerData)charInfo).psychicPoints}]";
    }
    [ClientRpc] private void UpdateUltPoints()
    {
        ultCounter.text = $"Ult progress:\n[{Mathf.Min(pointsSpent, ((SeerData)charInfo).requiredUltPoints)}/{((SeerData)charInfo).requiredUltPoints}]";
    }


    [Server] private void CheckRunicArmor()
    {
        if (!hasRunicArmor)
            return;
        if (runicArmorActve)
        {
            armorModifier -= 1;
            damageReduction -= 1;
            runicArmorActve = false;
        }
        if (currentPsychicPoints > 0)
        {
            currentPsychicPoints--;
            armorModifier += 1;
            damageReduction += 1;
            runicArmorActve = true;
        }
        UpdatePsychicPoints();
    }

    [Server] private CharacterBase FindPsychicTarget(RaycastHit hit, int range)
    {
        CharacterBase target = null;
        if (hit.collider.GetComponent<CharacterBase>())
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
        if (target == null || target == this)
            return null;
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
            return null;
        List<Vector3> path = GridCombatSystem.instance.FindPath(transform.position, target.transform.position, false);
        if (path != null && path.Count <= range + 1)
            return target;
        return null;
    }
}
