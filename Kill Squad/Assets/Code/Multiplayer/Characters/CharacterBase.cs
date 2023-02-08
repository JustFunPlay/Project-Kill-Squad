using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class CharacterBase : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar] [SerializeField] private int turnSpeed;
    [SyncVar] [SerializeField] private int maxHealth;
    [SyncVar] [SerializeField] private int armorSave;
    [SyncVar] [SerializeField] private int rangedSkill;
    [SyncVar] [SerializeField] private int meleeSkill;
    [SyncVar] [SerializeField] private int meleeAttacks;
    [SyncVar] [SerializeField]private float turnProgress = 0;
    [SyncVar] private int currentHealth;

    [Header("Specialized Stats")]
    [SyncVar] [SerializeField] private int dodgeChance;
    [SyncVar] [SerializeField] private int damageReduction;
    [SyncVar] [SerializeField] private LuckyRate armorLuck;
    [SyncVar] [SerializeField] private LuckyRate rangedLuck;
    [SyncVar] [SerializeField] private LuckyRate meleeLuck;
    [SyncVar] [SerializeField] private LuckyRate critLuck;

    [Header("Other Stuff")]
    public GameObject button;

    #region Getters/Setters
    public int Speed { get { return turnSpeed; } protected set { turnSpeed = value; } }
    public float Progress { get { return turnProgress; } protected set { turnProgress = value; } }
    public int Health { get { return currentHealth; } protected set { currentHealth = value; } }
    public int Armor { get { return armorSave; } protected set { armorSave = value; } }
    public int Ranged { get { return rangedSkill; } protected set { rangedSkill = value; } }
    public int Melee { get { return meleeSkill; } protected set { meleeSkill = value; } }
    public int Attacks { get { return meleeAttacks; } protected set { meleeAttacks = value; } }
    public int Dodge { get { return dodgeChance; } protected set { dodgeChance = value; } }
    public int DR { get { return damageReduction; } protected set { damageReduction = value; } }
    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        TurnTracker.instance.characters.Add(this);
    }

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

    [Server] public void ProgressTurn()
    {
        //Debug.Log("Progressing turn");
        turnProgress += Speed / (25f + Speed);
    }

    [Client] public void StartTurn()
    {
        if (isOwned)
            button.SetActive(true);
    }
    public void EndTurn()
    {
        Progress -= 1;
        button.SetActive(false);
        CmdProgressTurns();
    }
    private void CmdProgressTurns()
    {
        StartCoroutine(TurnTracker.instance.ProgressTurns());
    }

}

public enum LuckyRate
{
    Never,
    First,
    All
}
