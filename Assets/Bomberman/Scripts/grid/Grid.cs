using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    public LayerMask blockLayer;
    public LayerMask defaultLayer;

    Node[,] grid;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            printGrid();
        }
    }

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        foreach(TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;

            //int v = region.terrainMask.value;

            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.teerainPenalty);
        }


        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public int GetGridSizeX()
    {
        return gridSizeX;
    }

    public int GetGridSizeY()
    {
        return gridSizeY;
    }

    StateType getStateTypeFromHit(RaycastHit hit)
    {
        StateType nodeStateType = StateType.ST_Empty;

        if ((1 << hit.collider.gameObject.layer & blockLayer) != 0)
        {
            if (hit.collider.CompareTag("Block"))
            {
                nodeStateType = StateType.ST_Wall;
            }
            else if (hit.collider.CompareTag("Destructable"))
            {
                nodeStateType = StateType.ST_Block;
            }
        }
        else if ((1 << hit.collider.gameObject.layer & defaultLayer) != 0)
        {
            if (hit.collider.CompareTag("Player"))
            {
                //hit.collider.gameObject.GetComponent<Player>().playerNumber;
                nodeStateType = StateType.ST_Agent;
            }
            else if (hit.collider.CompareTag("Explosion"))
            {
                nodeStateType = StateType.ST_Fire;
            }
            else if (hit.collider.CompareTag("Bomb"))
            {
                nodeStateType = StateType.ST_Bomb;
            }
            else if (hit.collider.CompareTag("Danger"))
            {
                nodeStateType = StateType.ST_Danger;
            }
            else if (hit.collider.CompareTag("Target"))
            {
                nodeStateType = StateType.ST_Target;
            }
            else
            {
                nodeStateType = StateType.ST_Empty;
            }
        }

        return nodeStateType;
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                StateType nodeStateType = StateType.ST_Empty;

                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

                        nodeStateType = getStateTypeFromHit(hit);
                    }
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty, nodeStateType);

                //inicializado rhs e g para infinito de acordo com o algoritmo d star lite
                grid[x, y].InitDStarParams(double.PositiveInfinity, double.PositiveInfinity);
            }
        }
    }

    public void refreshNodesInGrid()
    {
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                StateType nodeStateType = StateType.ST_Empty;
                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    nodeStateType = getStateTypeFromHit(hit);

                    grid[x, y].movementPenalty = movementPenalty;
                    grid[x, y].stateType = nodeStateType;
                }
            }
        }
    }

    public List<Node> ListUpdateNodesInGrid()
    {
        List<Node> updatedNodes = new List<Node>();
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                StateType nodeStateType = StateType.ST_Empty;
                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                    nodeStateType = getStateTypeFromHit(hit);

                    if (grid[x, y].movementPenalty != movementPenalty)
                    {
                        updatedNodes.Add(grid[x, y]);
                    }

                    grid[x, y].movementPenalty = movementPenalty;
                    grid[x, y].stateType = nodeStateType;
                }
            }
        }

        return updatedNodes;
    }

    public void ClearDStarParams()
    {
      
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //inicializado rhs e g para infinito de acordo com o algoritmo d star lite
                grid[x, y].InitDStarParams(double.PositiveInfinity, double.PositiveInfinity);
            }
        }
    }

    public List<Node> GetSucc(Node node)
    {
        List<Node> neghbours = new List<Node>();

        if (!node.walkable || double.IsPositiveInfinity(node.cost))
            return neghbours;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (grid[checkX, checkY].walkable && !double.IsPositiveInfinity(grid[checkX, checkY].cost))
                        neghbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neghbours;
    }

    public List<Node> GetPred(Node node)
    {
        List<Node> neghbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    if (grid[checkX, checkY].walkable && !double.IsPositiveInfinity((grid[checkX, checkY].cost)))
                        neghbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neghbours;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neghbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neghbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neghbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        //minus one because first line and column are wall always
        worldPosition = worldPosition - Vector3.one;

        float percentX = (worldPosition.x) / gridWorldSize.x;
        float percentY = (worldPosition.z) / gridWorldSize.y;
    
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX) * percentX);
        int y = Mathf.RoundToInt((gridSizeY) * percentY);

        return grid[x, y];
    }

    public Node NodeFromPos(int x, int y)
    {
        return grid[x, y];
    }

    public StateType STFromPos(int x, int y)
    {
        return grid[x, y].stateType;
    }

    public void printGrid()
    {
        string saida = "";
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                saida += (int)grid[x, y].stateType + " | ";
            }
            saida += "\n";
        }

        Debug.Log(saida);
    }

    public string gridToString()
    {
        string saida = "";
        for (int x = 0; x < gridSizeX; ++x)
        {
            for (int y = 0; y < gridSizeY; ++y)
            {
                saida += (int)grid[x, y].stateType + " | ";
            }
            saida += "\n";
        }

        return saida;
    }

    public void enableObjectOnGrid(StateType stateType, Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int z = (int)gridPos.y;
        grid[x, z].stateType = stateType;
    }

    public void disableObjectOnGrid(Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int z = (int)gridPos.y;
        grid[x, z].stateType = StateType.ST_Empty;
    }

    public void updateAgentOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        if (grid[x, z].stateType == StateType.ST_Agent)
            grid[x, z].stateType = StateType.ST_Empty;

        pos = player.GetGridPosition();
        x = (int)pos.x;
        z = (int)pos.y;
        if (grid[x, z].stateType == StateType.ST_Empty)
            grid[x, z].stateType = StateType.ST_Agent;
    }

    public void clearAgentOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        grid[x, z].stateType = StateType.ST_Empty;

        pos = player.GetGridPosition();
        x = (int)pos.x;
        z = (int)pos.y;
        grid[x, z].stateType = StateType.ST_Empty;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            // Node playerNode = NodeFromWorldPoint(player.position);
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                /*if (playerNode == n)
                {
                    Gizmos.color = Color.cyan;
                }*/

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int teerainPenalty;
    }
}
