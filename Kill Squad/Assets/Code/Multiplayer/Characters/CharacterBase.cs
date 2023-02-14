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
    [SyncVar] [SerializeField] private int turnSpeed;
    [SyncVar] [SerializeField] private int maxHealth;
    [SyncVar] [SerializeField] private int armorSave;
    [SyncVar] [SerializeField] private int rangedSkill;
    [SyncVar] [SerializeField] private int meleeSkill;
    [SyncVar] [SerializeField] private int meleeAttacks;
    [SyncVar] [SerializeField] private float turnProgress = 0;
    [SyncVar] private int currentHealth;

    [Header("Specialized Stats")]
    [SyncVar] [SerializeField] private int dodgeChance;
    [SyncVar] [SerializeField] private int damageReduction;
    [SyncVar] [SerializeField] private LuckyRate armorLuck;
    [SyncVar] private bool luckyArmor;
    [SyncVar] private bool luckyShot;
    [SyncVar] private bool luckyMelee;
    [SyncVar] private bool luckyCrit;
    [SyncVar] [SerializeField] private LuckyRate rangedLuck;
    [SyncVar] [SerializeField] private LuckyRate meleeLuck;
    [SyncVar] [SerializeField] private LuckyRate critLuck;

    [Header("Other Stuff")]
    [SyncVar] private InGamePlayer owner;
    public GameObject button;
    public TMPro.TextMeshProUGUI speedText;
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
    [ClientRpc] private void UpdateUI()
    {
        if (owner.isOwned)
        {
            speedText.text = $"Speed: {Speed}";
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

    [ClientRpc] public void StartTurn()
    {
        if (owner.isOwned)
            button.SetActive(true);
    }
    [Command]public void EndTurn()
    {
        Progress -= 1;
        StartCoroutine(TurnTracker.instance.ProgressTurns());
        DeactivateTurnUi();
    }
    [ClientRpc]private void DeactivateTurnUi()
    {
        button.SetActive(false);
    }
    #endregion

    #region Damage & healing
    [Server] public virtual void ArmorSave(int pen, int crit, bool luckyCrit, int damage, out bool wound, out bool critConfirm, out int damageDealt)
    {
        int armorCheck = Random.Range(0, 10);
        critConfirm = false;
        damageDealt = 0;
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
        TakeDamage(damage, out damageDealt);
    }
    [Server] public virtual void TakeDamage(int damage, out int recievedDamage)
    {
        damage = Mathf.Max(damage - damageReduction, 1);
        currentHealth -= damage;
        recievedDamage = damage;
        UpdateUI();
        if (currentHealth <= 0)
            OnDeath();
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

    [Command] public void AttackRandomEnemy()
    {
        List<CharacterBase> availableTargets = new List<CharacterBase>();
        foreach (CharacterBase character in TurnTracker.instance.characters)
        {
            if (character.owner != owner)
                availableTargets.Add(character);
        }
        if (availableTargets.Count == 0)
            return;
        CharacterBase target = availableTargets[Random.Range(0, availableTargets.Count)];
        StartCoroutine(AttackSequence(target));
    }
    [Server] private IEnumerator AttackSequence(CharacterBase target)
    {
        for (int i = 0; i < meleeAttacks; i++)
        {
            Attack(meleeSkill, luckyMelee, ap, crit, luckyCrit, damage, target);
            yield return new WaitForSeconds(0.15f);
        }
    }

    [Server] private void Attack(int accuracy, bool luckyAttack, int penetration, int crit, bool luckyCrit, int damage, CharacterBase target)
    {
        int hitRoll = Random.Range(0, 10);
        bool wound = false;
        bool critConfirm = false;
        int damageDealt = 0; ;
        if (hitRoll < accuracy - target.dodgeChance)
        {
            target.ArmorSave(penetration, crit, luckyCrit, damage, out wound, out critConfirm, out damageDealt);
            return;
        }
        else if (luckyAttack)
        {
            hitRoll = Random.Range(0, 10);
            if (hitRoll < accuracy - target.dodgeChance)
            {
                target.ArmorSave(penetration, crit, luckyCrit, damage, out wound, out critConfirm, out damageDealt);
                return;
            }
        }
    }
}

public enum LuckyRate
{
    Never,
    First,
    All
}
