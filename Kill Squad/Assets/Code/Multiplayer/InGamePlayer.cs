using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class InGamePlayer : NetworkBehaviour
{
    [SyncVar] private string playerName;
    //public KillSquad killSquad;

    Vector2 moveVector;
    [SerializeField] float moveSpeed;
    float rotateDir;

    public string PlayerName { get { return playerName; } }

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
    public override void OnStartLocalPlayer()
    {
        Invoke("GetSquadForServer", 0.1f);
        CmdSetName();
    }

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

    [Client] private void GetSquadForServer()
    {
        //List<CharacterLoadout> playerSquad = new List<CharacterLoadout>();
        //for (int i = 0; i < PersistantInfo.Instance.squad.Count; i++)
        //{
        //    playerSquad.Add(PersistantInfo.Instance.squad[i]);
        //}
        CmdSetUpSquad(PersistantInfo.Instance.characters);
        //CmdSetUpSquad();
    }
    //[Command] private void CmdSetUpSquad(List<CharacterLoadout> squad)
    [Command] private void CmdSetUpSquad(List<CharacterLoadout> characters)
    {
        GridCombatSystem.instance.SetupTeam(characters, this);
        //for (int i = 0; i < PersistantInfo.Instance.squad; i++)
        //{
        //    GridCombatSystem.instance.SetupCharacter(PersistantInfo.Instance.squad[i].Character, PersistantInfo.Instance.squad[i].SelectedLoadoutOptions, this);
        //}
    }
    [Command] private void CmdSetName()
    {
        playerName = PersistantInfo.Instance.PlayerName;
    }

    [Client] public void LeftClick(InputAction.CallbackContext callbackContext)
    {
        if (isOwned && callbackContext.started && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray newRay = GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            TryPerformAction(newRay, this);
        }
    }
    [Command]public void TryPerformAction(Ray ray, InGamePlayer player)
    {
        if (TurnTracker.instance.activeCharacter && TurnTracker.instance.activeCharacter.Owner == player)
        {
            CharacterBase activeCharacter = TurnTracker.instance.activeCharacter;
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                activeCharacter.PerformAction(hit, player);
            }
        }
    }

    [Client] public void MovePlayer(InputAction.CallbackContext callbackContext)
    {
        moveVector = callbackContext.ReadValue<Vector2>();
    }
    [Client] public void RotatePlayer(InputAction.CallbackContext callbackContext)
    {
        rotateDir = callbackContext.ReadValue<float>();
    }
    private void Update()
    {
        transform.Translate(moveVector.x * moveSpeed * Time.deltaTime, 0, moveVector.y * moveSpeed * Time.deltaTime);
        transform.Rotate(0, rotateDir, 0);
    }
}
