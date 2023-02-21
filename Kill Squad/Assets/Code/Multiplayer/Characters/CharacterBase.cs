using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;
/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class CharacterBase : NetworkBehaviour
{
    [Header("Stats")]
    [SyncVar] [SerializeField] protected int turnSpeed;
    [SyncVar] [SerializeField] protected int movement;
    [SyncVar] [SerializeField] protected int maxHealth;
    [SyncVar] [SerializeField] protected int armorSave;
    [SyncVar] [SerializeField] protected int rangedSkill;
    [SyncVar] [SerializeField] protected int meleeSkill;
    [SyncVar] [SerializeField] protected int meleeAttacks;
    [SyncVar] [SerializeField] protected float turnProgress = 0;
    [SyncVar] protected int currentHealth;

    [Header("Specialized Stats")]
    [SyncVar] [SerializeField] protected int dodgeChance;
    [SyncVar] [SerializeField] protected int damageReduction;
    [SyncVar] [SerializeField] protected LuckyRate armorLuck;
    [SyncVar] protected bool luckyArmor;
    [SyncVar] protected bool luckyShot;
    [SyncVar] protected bool luckyMelee;
    [SyncVar] protected bool luckyCrit;
    [SyncVar] [SerializeField] protected LuckyRate rangedLuck;
    [SyncVar] [SerializeField] protected LuckyRate meleeLuck;
    [SyncVar] [SerializeField] protected LuckyRate critLuck;

    [Header("Other Stuff")]
    [SyncVar] protected InGamePlayer owner;
    public GameObject[] buttons;
    public TMPro.TextMeshProUGUI speedText;
    public TMPro.TextMeshProUGUI movementText;
    public TMPro.TextMeshProUGUI hpText;
    public TMPro.TextMeshProUGUI toHitText;
    public TMPro.TextMeshProUGUI attacksText;
    public TMPro.TextMeshProUGUI damageText;
    [SyncVar] public int damage;
    public TMPro.TextMeshProUGUI armorText;
    [SyncVar] public int ap;
    public TMPro.TextMeshProUGUI apText;
    public TMPro.TextMeshProUGUI critText;
    [SyncVar] public int crit;

    [Header("Turn management")]
    [SyncVar] [SerializeField] protected bool canAct = false;
    [SyncVar] [SerializeField] protected int remainingActions;
    [SyncVar] [SerializeField] protected Action selectedAction;
    [SyncVar] [SerializeField] protected ActionVar selectedVariant;
    protected SyncList<string> performedActions = new SyncList<string>();

    #region Getters/Setters
    public int Speed { get { return turnSpeed; } protected set { turnSpeed = value; } }
    public int Movement { get { return movement; } protected set { movement = value; } }
    public float Progress { get { return turnProgress; } protected set { turnProgress = value; } }
    public int Health { get { return currentHealth; } protected set { currentHealth = value; } }
    public int Armor { get { return armorSave; } protected set { armorSave = value; } }
    public int Ranged { get { return rangedSkill; } protected set { rangedSkill = value; } }
    public int Melee { get { return meleeSkill; } protected set { meleeSkill = value; } }
    public int Attacks { get { return meleeAttacks; } protected set { meleeAttacks = value; } }
    public int Dodge { get { return dodgeChance; } protected set { dodgeChance = value; } }
    public int DR { get { return damageReduction; } protected set { damageReduction = value; } }
    public InGamePlayer Owner { get { return owner; } }
    public bool CanAct { get { return canAct; } }
    public int RemainingActions { get { return remainingActions; } set { remainingActions = value; } }
    #endregion

    #region lucky checks
    public bool LuckyRangedAttack()
    {
        if (rangedLuck == LuckyRate.All)
            return true;
        else if (rangedLuck == LuckyRate.First && luckyShot == true)
        {
            luckyShot = false;
            return true;
        }
        return false;
    }
    public bool LuckyMeleeAttack()
    {
        if (meleeLuck == LuckyRate.All)
            return true;
        else if (meleeLuck == LuckyRate.First && luckyMelee == true)
        {
            luckyMelee = false;
            return true;
        }
        return false;
    }
    public bool LuckyCrit()
    {
        if (critLuck == LuckyRate.All)
            return true;
        else if (critLuck == LuckyRate.First && luckyCrit == true)
        {
            luckyCrit = false;
            return true;
        }
        return false;
    }

    #endregion

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        
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

    [Server] public virtual void SetupCharacter(InGamePlayer player, CharacterInfoBase info)
    {
        owner = player;
        turnSpeed = info.speed;
        movement = info.movement;
        maxHealth = info.health;
        armorSave = info.armor;
        rangedSkill = info.ranged;
        meleeSkill = info.melee;
        meleeAttacks = info.attacks;
        currentHealth = maxHealth;
        TurnTracker.instance.characters.Add(this);
        if (armorLuck != LuckyRate.Never)
            luckyArmor = true;
        Invoke("UpdateUI", 0.5f);
    }
    [ClientRpc] protected void UpdateUI()
    {
        if (owner.isOwned)
        {
            speedText.text = $"Speed: {Speed}";
            movementText.text = $"Movement: {movement}";
            hpText.text = $"Health: {currentHealth}";
            toHitText.text = $"To hit: {meleeSkill}";
            attacksText.text = $"Attacks: {meleeAttacks}";
            damageText.text = $"Damage: {damage}";
            armorText.text = $"Armor: {armorSave}";
            apText.text = $"Armor penetration: {ap}";
            critText.text = $"Crit: {crit}";
        }
    }

    #region Turns and actions
    [Server] public void ProgressTurn()
    {
        turnProgress += Speed / (25f + Speed);
        if (armorLuck == LuckyRate.First)
            luckyArmor = true;
    }

    [Server] public void PrepareTurn()
    {
        remainingActions = 3;
        canAct = true;
        performedActions.Clear();
        SelectAction(Action.Movement, ActionVar.Normal);
        StartTurn();
    }
    [ClientRpc] public void StartTurn()
    {
        if (owner.isOwned)
        {
            ToggleButtons(true);
            //GetMoveRange();
        }
    }
    [Server]protected void EndTurn()
    {
        if (!canAct)
            return;
        canAct = false;
        Progress -= 1;
        StartCoroutine(TurnTracker.instance.ProgressTurns());
        DeactivateTurnUi();
    }
    [Command]public void FinishTurn()
    {
        EndTurn();
    }
    [ClientRpc]protected void DeactivateTurnUi()
    {
        ToggleButtons(false);
    }

    [Client] protected void ToggleButtons(bool setActive)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].SetActive(setActive);
        }
    }

    [Server] public void ContinueTurn()
    {
        canAct = true;
        SelectAction(Action.Movement, ActionVar.Normal);
        if (remainingActions < 1)
        {
            EndTurn();
            return;
        }
    }
    [Server] protected void StartAction(int actionCost = 1, string performedAction = null)
    {
        remainingActions -= actionCost;
        performedActions.Add(performedAction);
        canAct = false;
    }
    [Server] protected void StartAction(string performedAction)
    {
        StartAction(1, performedAction);
    }

    [Command] public void SelectAction(Action action, ActionVar variant)
    {
        this.selectedAction = action;
        this.selectedVariant = variant;
    }

    [Server] public virtual void PerformAction(RaycastHit hit, InGamePlayer player)
    {

    }

    [Server] protected virtual void ReportForCombat(CombatReport report)
    {
        Debug.Log($"Total attacks: {report.totalAttackCount}\nHits: {report.attacksHit}\nWounds: {report.armorPierced}\nCrits: {report.critHits}\nTotal Damage: {report.damageDealt}\nKilling blow: {report.killingBlow}");
        ContinueTurn();
    }
    #endregion

    #region Damage & healing
    [Server] public virtual void ArmorSave(int pen, int crit, bool luckyCrit, int damage, out bool wound, out bool critConfirm, out int damageDealt, out bool killingBlow)
    {
        int armorCheck = Random.Range(0, 10);
        critConfirm = false;
        damageDealt = 0;
        killingBlow = false;
        if (armorCheck < armorSave + pen)
        {
            wound = false;
            return;
        }
        else if (luckyArmor)
        {
            if (armorLuck == LuckyRate.First)
                luckyArmor = false;
            armorCheck = Random.Range(0, 10);
            if (armorCheck < armorSave + pen)
            {
                wound = false;
                return;
            }
        }
        wound = true;
        int critCheck = Random.Range(0, 10);
        if (critCheck < crit)
        {
            critConfirm = true;
            damage *= 2;
        }
        else if (luckyCrit)
        {
            critCheck = Random.Range(0, 10);
            if (critCheck < crit)
            {
                critConfirm = true;
                damage *= 2;
            }
        }
        TakeDamage(damage, out damageDealt, out killingBlow);
    }
    [Server] public virtual void TakeDamage(int damage, out int recievedDamage, out bool isKilled)
    {
        damage = Mathf.Max(damage - damageReduction, 1);
        currentHealth -= damage;
        recievedDamage = damage;
        isKilled = false;
        UpdateUI();
        if (currentHealth <= 0)
        {
            isKilled = true;
            OnDeath();
        }
    }
    [Server] protected virtual void OnDeath()
    {
        TurnTracker.instance.characters.Remove(this);
    }
    [Server] public void GetHealed(int healValue, out int healingDone)
    {
        healingDone = Mathf.Min(maxHealth - currentHealth, healValue);
        currentHealth = Mathf.Min(currentHealth + healValue, maxHealth);
        UpdateUI();
    }
    #endregion

    [Command] public void GetMoveRange()
    {
        GridCombatSystem.instance.VisualizeMoveDistance(this);
    }
}

public enum LuckyRate
{
    Never,
    First,
    All
}

public enum Action
{
    Movement,
    Action1,
    Action2,
    Action3,
    Action4,
    Action5,
    Action6,
    Ultimate
}
public enum ActionVar
{
    Normal,
    Variant1,
    Variant2
}