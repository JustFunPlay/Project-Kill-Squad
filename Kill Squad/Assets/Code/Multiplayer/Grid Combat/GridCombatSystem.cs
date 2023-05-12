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
    public override void OnStartClient()
    {
    }

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
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int z = 0; z < grid.GetLength(); z++)
            {
                if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.down, Vector3.up, 4, obstacleLayer))
                {
                    grid.GetGridObject(x, z).isWalkable = false;
                    grid.GetGridObject(x, z).canMoveNorth = false;
                    grid.GetGridObject(x, z).canMoveEast = false;
                    grid.GetGridObject(x, z).canMoveSouth = false;
                    grid.GetGridObject(x, z).canMoveWest = false;
                }
                else
                {
                    if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.up, Vector3.forward, 2, obstacleLayer))
                        grid.GetGridObject(x, z).canMoveNorth = false;
                    if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.up, Vector3.right, 2, obstacleLayer))
                        grid.GetGridObject(x, z).canMoveEast = false;
                    if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.up, Vector3.back, 2, obstacleLayer))
                        grid.GetGridObject(x, z).canMoveSouth = false;
                    if (Physics.Raycast(grid.GetWorldPosition(x, z) + Vector3.up, Vector3.left, 2, obstacleLayer))
                        grid.GetGridObject(x, z).canMoveWest = false;

                }
            }
        }
        Invoke("SetupGridVisualizer", 0.3f);

    }
    [ClientRpc]private void SetupGridVisualizer()
    {
        GridVisualizer.instance.SetupGridVisualizer(gridOrigin, gridSizeX, gridSizeZ);
    }
    
    [Server] public void GetRangeVisualizer(CharacterBase character, int range, bool requiresLos)
    {
        ResetVisualRange();
        Vector3 origin = character.transform.position;
        List<GridNode> validPositions = new List<GridNode> { grid.GetGridObject(origin)};
        for (int i = 0; i < range; i++)
        {
            int currentPositions = validPositions.Count;
            for (int ii = 0; ii < currentPositions; ii++)
            {
                //GetNeighborsFromServer(validPositions[ii], out List<GridNode> neigborList);
                foreach (GridNode neighborNode in GetneighborList(validPositions[ii], !requiresLos))
                {
                    if (validPositions.Contains(neighborNode)  || !neighborNode.isWalkable)
                        continue;
                     validPositions.Add(neighborNode);
                }
            }
        }
        List<Vector3> validLocations = new List<Vector3>();
        foreach (GridNode validPos in validPositions)
        {
            if (!requiresLos)
            {
                validLocations.Add(grid.GetWorldPosition(validPos.X, validPos.Z));
                continue;
            }
            bool hasLos = false;
            for (int l = 0; l < 5; l++)
            {
                Vector3 startpos = origin + Vector3.up * 1.5f;
                if (l == 1 && !Physics.Raycast(startpos, Vector3.forward, 0.95f, obstacleLayer))
                    startpos += Vector3.forward * 0.95f;
                else if (l == 2 && !Physics.Raycast(startpos, Vector3.back, 0.95f, obstacleLayer))
                    startpos += Vector3.back * 0.95f;
                else if (l == 3 && !Physics.Raycast(startpos, Vector3.left, 0.95f, obstacleLayer))
                    startpos += Vector3.left * 0.95f;
                else if (l == 4 && !Physics.Raycast(startpos, Vector3.right, 0.95f, obstacleLayer))
                    startpos += Vector3.right * 0.95f;

                if (Physics.Raycast(startpos, (grid.GetWorldPosition(validPos.X, validPos.Z) + Vector3.up * 1.5f - startpos).normalized, Vector3.Distance(startpos, grid.GetWorldPosition(validPos.X, validPos.Z)), obstacleLayer) == false)
                {
                    hasLos = true;
                    break;
                }
            }
            if (hasLos)
                validLocations.Add(grid.GetWorldPosition(validPos.X, validPos.Z));
        }
        VisualizeRange(validLocations, character);
    }
    [Server] public void GetMeleeVisualizer(CharacterBase character, int range, bool requiresLos)
    {
        ResetVisualRange();
        Vector3 origin = character.transform.position;
        List<GridNode> validPositions = new List<GridNode> { grid.GetGridObject(origin) };
        grid.GetXZ(origin, out int originX, out int originZ);
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                int xPos = originX + x;
                int zPos = originZ + z;
                if (xPos >= 0 && xPos < gridSizeX && zPos >= 0 && zPos < gridSizeZ)
                {
                    GridNode neighborNode = grid.GetGridObject(xPos, zPos);
                    if (neighborNode.isWalkable)
                    {
                        if (!requiresLos)
                        {
                            validPositions.Add(neighborNode);
                            continue;
                        }
                        bool hasLos = false;
                        for (int l = 0; l < 5; l++)
                        {
                            Vector3 startpos = origin + Vector3.up;
                            if (l == 1 && !Physics.Raycast(startpos, Vector3.forward, 0.95f, obstacleLayer))
                                startpos += Vector3.forward * 0.95f;
                            else if (l == 2 && !Physics.Raycast(startpos, Vector3.back, 0.95f, obstacleLayer))
                                startpos += Vector3.back * 0.95f;
                            else if (l == 3 && !Physics.Raycast(startpos, Vector3.left, 0.95f, obstacleLayer))
                                startpos += Vector3.left * 0.95f;
                            else if (l == 4 && !Physics.Raycast(startpos, Vector3.right, 0.95f, obstacleLayer))
                                startpos += Vector3.right * 0.95f;

                            if (Physics.Raycast(startpos, (grid.GetWorldPosition(neighborNode.X, neighborNode.Z) - startpos).normalized, Vector3.Distance(startpos, grid.GetWorldPosition(neighborNode.X, neighborNode.Z)), obstacleLayer) == false)
                            {
                                hasLos = true;
                                break;
                            }
                        }
                        if (hasLos)
                            validPositions.Add(neighborNode);
                    }
                }
            }
        }
        List<Vector3> validLocations = new List<Vector3>();
        foreach (GridNode validPos in validPositions)
        {
            validLocations.Add(grid.GetWorldPosition(validPos.X, validPos.Z));
        }
        VisualizeRange(validLocations, character);
    }
    [ClientRpc] private void VisualizeRange(List<Vector3> validPositions, CharacterBase character)
    {
        if (!character.Owner.isOwned)
            return;
        GridVisualizer.instance.VisualizeRange(validPositions);

    }
    [ClientRpc] public void ResetVisualRange()
    {
        GridVisualizer.instance.ResetVisualRange();
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
                //character.SetupCharacter(player, squad.squad[i]);
            }
            nextTeam = true;
            return;
        }
        for (int i = 0; i < squad.squad.Count; i++)
        {
            CharacterBase character = Instantiate(squad.squad[i].physicalCharacter, grid.GetWorldPosition((int)defenderGridSpawn.x - i, (int)defenderGridSpawn.y), Quaternion.identity);
            NetworkServer.Spawn(character.gameObject, player.gameObject);
            //character.SetupCharacter(player, squad.squad[i]);
        }
    }
}

//[System.Serializable]
//public class GridVisualizer
//{
//    public GameObject visualizer;
//    public Vector2 gridLocation;

//    public GridVisualizer(GameObject gameObject, Vector2 gridLocation)
//    {
//        this.visualizer = gameObject;
//        this.gridLocation = gridLocation;
//    }
//    public GridVisualizer()
//    {
//        visualizer = null;
//        gridLocation = new Vector2(-5, -5);
//    }
//}