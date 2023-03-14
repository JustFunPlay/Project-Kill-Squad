using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class GridCombatSystem : Pathfinding
{
    public static GridCombatSystem instance;
    [SerializeField] private int gridSizeX, gridSizeZ;
    [SerializeField] private Vector3 gridOrigin;
    [SyncVar] private bool nextTeam;
    [SerializeField] private Vector2 attackerGridSpawn;
    [SerializeField] private Vector2 defenderGridSpawn;
    public LayerMask obstacleLayer;

    [Header("Visualisation")]
    [SerializeField] private GameObject gridCube;
    public SyncList<GridVisualizer> gridSlots = new SyncList<GridVisualizer>();

    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer()
    {
        if (!instance)
            instance = this;
        else
            Destroy(this);
        Invoke("SetupPathFinder", 0.1f);
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

    [Server] private void SetupPathFinder()
    {
        InitializeGrid(gridSizeX, gridSizeZ, gridOrigin);
        Invoke("SetupGridVisualizer", 0.1f);
    }
    [Server]private void SetupGridVisualizer()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetLength(); z++)
            {
                if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.down, Vector3.up, 4, obstacleLayer))
                    grid.GetGridObject(x, z).isWalkable = false;
                GameObject newVisualizer = Instantiate(gridCube, grid.GetWorldPosition(x, z) + new Vector3(0, 0.1f, 0), Quaternion.identity, transform);
                NetworkServer.Spawn(newVisualizer);
                gridSlots.Add(new GridVisualizer(newVisualizer, new Vector2(x, z)));
            }
        }
    }
    
    [Server] public void VisualizeMoveDistance(CharacterBase character)
    {
        //for (int i = 0; i < gridSlots.Count; i++)
        //{
        //    gridSlots[i].visualizer.SetActive(false);
        //}
        //int maxMove = character.Movement;
        //grid.GetXZ(character.transform.position, out int charX, out int charZ);
        //for (int x = 0; x < grid.GetWidth(); x++)
        //{
        //    for (int z = 0; z < grid.GetLength(); z++)
        //    {
        //        if (grid.GetGridObject(x, z).isWalkable && Findpath(charX, charZ, x, z) != null && Findpath(charX, charZ, x, z).Count <= maxMove)
        //        {
        //            GridVisualizer visualizer = GetGridVisualizer(x, z);
        //            if (visualizer != null)
        //                visualizer.visualizer.SetActive(true);
        //        }
        //    }
        //}
    }

    private GridVisualizer GetGridVisualizer(int x, int z)
    {
        int i = grid.GetWidth() * z + x;
        if (gridSlots[i].gridLocation != new Vector2(x, z))
            return null;
        return gridSlots[i];
    }

    [Server]
    public void SetupTeam(KillSquad squad, InGamePlayer player)
    {
        if (!nextTeam)
        {
            for (int i = 0; i < squad.squad.Count; i++)
            {
                CharacterBase character = Instantiate(squad.squad[i].physicalCharacter, grid.GetWorldPosition((int)attackerGridSpawn.x + i, (int)attackerGridSpawn.y), Quaternion.identity);
                NetworkServer.Spawn(character.gameObject, player.gameObject);
                character.SetupCharacter(player, squad.squad[i]);
            }
            nextTeam = true;
            return;
        }
        for (int i = 0; i < squad.squad.Count; i++)
        {
            CharacterBase character = Instantiate(squad.squad[i].physicalCharacter, grid.GetWorldPosition((int)defenderGridSpawn.x - i, (int)defenderGridSpawn.y), Quaternion.identity);
            NetworkServer.Spawn(character.gameObject, player.gameObject);
            character.SetupCharacter(player, squad.squad[i]);
        }
    }
}

[System.Serializable]
public class GridVisualizer
{
    public GameObject visualizer;
    public Vector2 gridLocation;

    public GridVisualizer(GameObject gameObject, Vector2 gridLocation)
    {
        this.visualizer = gameObject;
        this.gridLocation = gridLocation;
    }
    public GridVisualizer()
    {
        visualizer = null;
        gridLocation = new Vector2(-5, -5);
    }
}