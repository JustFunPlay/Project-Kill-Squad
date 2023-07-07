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

    [Header("Camera Movement")]
    public float edgeMoveSpeed;
    /// <summary>
    /// The maximum speed at which the camera moves in units per second while the cursor is near the edge of the window
    /// </summary>
    public float mouseMoveSpeed;
    /// <summary>
    /// The avarage speed at which the camera moves in units per second while holding the right mouse button
    /// </summary>
    public float moveMargin;
    /// <summary>
    /// The area of the window in which the camera will move
    /// </summary>
    public float rotateSpeed;
    /// <summary>
    /// The speed at which the camera rotates in degrees per second
    /// </summary>
    public float zoomSpeed;
    /// <summary>
    /// The speed at which the camera zooms in or out
    /// </summary>
    public Maplimiter camBoundary;
    public Vector2 zoomBoundary;
    /// <summary>
    /// The minimum and maximum distance from origin;
    /// </summary>
    public Camera cam;
    Vector3 camDir;
    float currentCamDistance;
    float zoomValue;
    float rotateValue;

    bool holdLeftClick;
    bool holdRightClick;
    bool holdMiddleClick;
    Vector2 mouseValue;
    float holdDuration;

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

    [Client]private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Confined;
        currentCamDistance = Vector3.Distance(transform.position, cam.transform.position);
        camDir = (cam.transform.position - transform.position).normalized;
    }
    [Client]public void RotateCamInput(InputAction.CallbackContext callbackContext)
    {
        rotateValue = callbackContext.ReadValue<float>();
    }
    [Client]public void ZoomCamInput(InputAction.CallbackContext callbackContext)
    {
        zoomValue = callbackContext.ReadValue<float>();
    }

    [Client]public void RightClick(InputAction.CallbackContext callbackContext)
    {
        if (!holdLeftClick && !holdMiddleClick && callbackContext.started)
        {
            holdRightClick = true;
            mouseValue = new Vector2();
            holdDuration = 0;
        }
        else if (holdRightClick && callbackContext.canceled && holdDuration >= 0.2f)
        {
            holdRightClick = false;
        }
    }
    [Client]public void MiddleClick(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started && !holdLeftClick && !holdRightClick)
        {
            holdMiddleClick = true;
            mouseValue = new Vector2();
        }
        else if (callbackContext.canceled)
            holdMiddleClick = false;
    }
    [Client]public void MoveMouse(InputAction.CallbackContext callbackContext)
    {
        if (holdRightClick || holdMiddleClick)
            mouseValue += callbackContext.ReadValue<Vector2>();
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
        //transform.Translate(moveVector.x * moveSpeed * Time.deltaTime, 0, moveVector.y * moveSpeed * Time.deltaTime);
        //transform.Rotate(0, rotateDir, 0);
        if (!holdLeftClick && !holdMiddleClick)
            MoveCam();
        ZoomCam();
        transform.Rotate(0, (holdMiddleClick ? -mouseValue.x * 0.4f : -rotateValue * rotateSpeed) * Time.deltaTime, 0);
        if (holdLeftClick || holdRightClick)
            holdDuration += Time.deltaTime;
    }
    void MoveCam()
    {
        Vector3 moveDir = new Vector3();
        if (holdRightClick)
        {
            if (holdDuration > 0.2f)
            {
                moveDir.x = mouseValue.x * mouseMoveSpeed * Time.deltaTime;
                moveDir.z = mouseValue.y * mouseMoveSpeed * Time.deltaTime;
            }
        }
        else
        {
            Vector2 resolution = new Vector2(Screen.width, Screen.height);
            Vector2 actualMargin = new Vector2(resolution.x * moveMargin, resolution.y * moveMargin);
            if (Application.isFocused)
            {
                if (Mouse.current.position.x.ReadValue() <= actualMargin.x && Mouse.current.position.x.ReadValue() >= 0)
                {
                    float perc = 1 - (Mouse.current.position.x.ReadValue() / actualMargin.x);
                    moveDir.x = -edgeMoveSpeed * Time.deltaTime * perc;
                }
                else if (Mouse.current.position.x.ReadValue() >= resolution.x - actualMargin.x && Mouse.current.position.x.ReadValue() <= resolution.x)
                {
                    float perc = 1 - ((resolution.x - Mouse.current.position.x.ReadValue()) / actualMargin.x);
                    moveDir.x = edgeMoveSpeed * Time.deltaTime * perc;
                }
                if (Mouse.current.position.y.ReadValue() <= actualMargin.y && Mouse.current.position.y.ReadValue() >= 0)
                {
                    float perc = 1 - (Mouse.current.position.y.ReadValue() / actualMargin.y);
                    moveDir.z = -edgeMoveSpeed * Time.deltaTime * perc;
                }
                else if (Mouse.current.position.y.ReadValue() >= resolution.y - actualMargin.y && Mouse.current.position.y.ReadValue() <= resolution.y)
                {
                    float perc = 1 - ((resolution.y - Mouse.current.position.y.ReadValue()) / actualMargin.y);
                    moveDir.z = edgeMoveSpeed * Time.deltaTime * perc;
                }
            }
        }
        transform.Translate(moveDir);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, camBoundary.xLimit.x, camBoundary.xLimit.y), transform.position.y, Mathf.Clamp(transform.position.z, camBoundary.zLimit.x, camBoundary.zLimit.y));
    }
    void ZoomCam()
    {
        currentCamDistance = Mathf.Clamp(currentCamDistance += (holdMiddleClick ? -mouseValue.y * 0.2f : zoomValue) * zoomSpeed * Time.deltaTime, zoomBoundary.x, zoomBoundary.y);
        cam.transform.localPosition = currentCamDistance * camDir;
    }
}

[System.Serializable]
public class Maplimiter
{
    public Vector2 xLimit;
    public Vector2 zLimit;
}