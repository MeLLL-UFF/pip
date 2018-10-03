using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grid : MonoBehaviour {

    public int scenarioId; 
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    public LayerMask blockLayer;
    public LayerMask defaultLayer;
    public LayerMask agentsLayer;

    public GridType gridType = GridType.GT_Hybrid;
    public GridSentData gridSentData = GridSentData.GSD_All;
    BaseNode[,] grid;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    private List<StateType> blockFlagsList = new List<StateType>();
    private StateType blockFlags;

    private void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.I))
        {
            printGrid();
        }*/
    }

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        blockFlagsList.Add(StateType.ST_Block);
        blockFlagsList.Add(StateType.ST_Wall);
        //Bomb code
        blockFlagsList.Add(StateType.ST_Bomb);

        blockFlags = StateType.ST_Block | StateType.ST_Wall | StateType.ST_Bomb;

        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.terrainMask.value;

            //int v = region.terrainMask.value;

            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.teerainPenalty);
        }

        CreateGrid();
    }

    private void Start()
    {
        //CreateGrid();
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

    private bool isOnGrid(int x, int y)
    {
        if (x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY)
            return true;

        return false;
    }

    public void updateStateOnNode(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return;

        BaseNode node = NodeFromPos(x, y);

        List<StateType> nodeStateTypes = new List<StateType>();
        Ray ray = new Ray(node.worldPosition + Vector3.up * 6, Vector3.down);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 10, walkableMask);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            nodeStateTypes.Add(getStateTypeFromHit(hit));
        }

        node.clearAllFlags();
        node.addFlags(nodeStateTypes);
    }

    public bool checkFreePosition(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return false;

        BaseNode node = NodeFromPos(x, y);

        if (gridType == GridType.GT_Hybrid)
        {
            if (node.hasSomeFlag(blockFlagsList))
                return false;
        }
        else if (gridType == GridType.GT_Binary)
        {
            if (node.hasSomeFlag(blockFlags))
                return false;
        }

        return true;
    }

    public bool checkTarget(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return false;

        BaseNode node = NodeFromPos(x, y);

        if (node.hasFlag(StateType.ST_Target))
            return true;
       
        return false;
    }

    // Bomb code
    public bool checkFire(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return false;

        BaseNode node = NodeFromPos(x, y);

        if (node.hasFlag(StateType.ST_Fire))
            return true;

        return false;
    }

    public bool checkDanger(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return false;

        BaseNode node = NodeFromPos(x, y);

        if (node.hasFlag(StateType.ST_Danger))
            return true;

        return false;
    }

    public bool checkDestructible(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return false;

        BaseNode node = NodeFromPos(x, y);

        if (node.hasFlag(StateType.ST_Block))
            return true;

        return false;
    }

    public Destructable getDestructibleInPosition(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        if (!isOnGrid(x, y))
            return null;

        BaseNode node = NodeFromPos(x, y);

        Ray ray = new Ray(node.worldPosition + Vector3.up * 6, Vector3.down);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, 10, walkableMask);
        GameObject gb = null;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];

            StateType st = getStateTypeFromHit(hit);
            if (st == StateType.ST_Block)
            {
                gb = hit.transform.gameObject;
                break;
            }
        }

        if (gb == null)
            return null;

        return gb.GetComponent<Destructable>();
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
            //Bomb code
            if (hit.collider.CompareTag("Explosion"))
            {
                nodeStateType = StateType.ST_Fire;
            }
            else if (hit.collider.CompareTag("Bomb"))
            {
                nodeStateType = StateType.ST_Bomb;
            }
            else if (hit.collider.CompareTag("Target"))
            {
                nodeStateType = StateType.ST_Target;
            }
            else if (hit.collider.CompareTag("Danger"))
            {
                nodeStateType = StateType.ST_Danger;
            }
            else
            {
                nodeStateType = StateType.ST_Empty;
            }
        }
        else if ((1 << hit.collider.gameObject.layer & agentsLayer) != 0)
        {
            if (hit.collider.CompareTag("Player"))
            {
                int number = hit.collider.gameObject.GetComponent<Player>().playerNumber;
                if (number == 1)
                    nodeStateType = StateType.ST_Agent1;
                else if (number == 2)
                {
                    //como o agente está imitando o outro, logo é necessário que o espaço de estados seja representado da mesma forma
                    nodeStateType = StateType.ST_Agent2;
                }
                    
            }
        }

        return nodeStateType;
    }

    void CreateGridAccordingToType()
    {
        switch(gridType)
        {
            case GridType.GT_Hybrid:
                grid = new HybridNode[gridSizeX, gridSizeY];
                break;
            case GridType.GT_Binary:
                grid = new BinaryNode[gridSizeX, gridSizeY];
                break;
            /*case GridType.GT_OneHot:
                break;*/
            default:
                break;
        }
    }

    BaseNode CreateNodeAccordingToType(bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty, List<StateType> stateTypes)
    {
        BaseNode node = null;
        switch (gridType)
        {
            case GridType.GT_Hybrid:
                node = new HybridNode(walkable, worldPos, gridX, gridY, penalty, stateTypes);
                break;
            case GridType.GT_Binary:
                node = new BinaryNode(walkable, worldPos, gridX, gridY, penalty, stateTypes);
                break;
            /*case GridType.GT_OneHot:
                break;*/
            default:
                break;
        }

        return node;
    }

    void CreateGrid()
    {
        CreateGridAccordingToType();
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                List<StateType> nodeStateTypes = new List<StateType>();

                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 6, Vector3.down);
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray, 10, walkableMask);
                    //walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit hit = hits[i];

                        nodeStateTypes.Add(getStateTypeFromHit(hit));
                    }
                }

                grid[x, y] = CreateNodeAccordingToType(walkable, worldPoint, x, y, movementPenalty, nodeStateTypes);

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

                List<StateType> nodeStateTypes = new List<StateType>();
                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 6, Vector3.down);
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray, 10, walkableMask);
                    //walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit hit = hits[i];
                        nodeStateTypes.Add(getStateTypeFromHit(hit));
                    }

                    grid[x, y].movementPenalty = movementPenalty;
                    grid[x, y].clearAllFlags();
                    grid[x, y].addFlags(nodeStateTypes);
                }
            }
        }
    }

    public List<BaseNode> ListUpdateNodesInGrid()
    {
        List<BaseNode> updatedNodes = new List<BaseNode>();
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));

                int movementPenalty = 0;

                List<StateType> nodeStateTypes = new List<StateType>();
                if (walkable)
                {
                    Ray ray = new Ray(worldPoint + Vector3.up * 6, Vector3.down);
                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray, 10, walkableMask);
                    //walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    for (int i = 0; i < hits.Length; i++)
                    {
                        RaycastHit hit = hits[i];
                        nodeStateTypes.Add(getStateTypeFromHit(hit));
                    }

                    if (grid[x, y].movementPenalty != movementPenalty)
                    {
                        updatedNodes.Add(grid[x, y]);
                    }

                    grid[x, y].movementPenalty = movementPenalty;
                    grid[x, y].clearAllFlags();
                    grid[x, y].addFlags(nodeStateTypes);
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

    public List<BaseNode> GetSucc(BaseNode node)
    {
        List<BaseNode> neghbours = new List<BaseNode>();

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

    public List<BaseNode> GetPred(BaseNode node)
    {
        List<BaseNode> neghbours = new List<BaseNode>();

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

    public List<BaseNode> GetNeighbours(BaseNode node)
    {
        List<BaseNode> neghbours = new List<BaseNode>();

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

    public BaseNode NodeFromWorldPoint(Vector3 worldPosition)
    {
        //minus one because first line and column are wall always
        // worldPosition = worldPosition - Vector3.one;

        float percentX = (worldPosition.x) / gridWorldSize.x;
        float percentY = (worldPosition.z) / gridWorldSize.y;
    
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX) * percentX), 0, gridSizeX-1);
        int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY) * percentY), 0, gridSizeY-1);

        return grid[x, y];
    }

    public BaseNode NodeFromPos(int x, int y)
    {
        return grid[x, y];
    }

    public void printGrid()
    {
        string saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                saida += grid[x, y].getStringBinaryArray() + " | ";
            }
            saida += "\n";
        }

        Debug.Log(saida);
    }

    public void printGridDivided()
    {
        string saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                saida += (grid[x, y].getFreeBreakableObstructedCell()) + "\t";
            }
            saida += "\n";
        }
        Debug.Log(saida);

        saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                saida += (grid[x, y].getPositionAgent(1));
            }
            saida += "\n";
        }
        Debug.Log(saida);

        saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                saida += (grid[x, y].getPositionTarget());
            }
            saida += "\n";
        }
        Debug.Log(saida);

        saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                bool hasDanger = grid[x, y].getDangerPosition();
                if (!hasDanger)
                    saida += (0.0f).ToString("0.000") + "\t";
                else
                {
                    Danger danger = ServiceLocator.getManager(scenarioId).GetBombManager().getDanger(x, y);
                    string dangerLevel = danger.GetDangerLevelOfPositionToPrint();
                    saida += (dangerLevel) + "\t";
                }
            }
            saida += "\n";
        }
        Debug.Log(saida);
    }

    public string gridToString(int playerNumber)
    {
        string saida = "";
        for (int y = gridSizeY - 1; y >= 0; --y)
        {
            for (int x = 0; x < gridSizeX; ++x)
            {
                BaseNode node = grid[x, y];
                StateType nodeStateType = (StateType)node.getBinary();

                if (playerNumber == 2)
                {
                    //se é um nó com stateType agent
                    if (node.hasFlag(StateType.ST_Agent1))
                    {
                        nodeStateType = nodeStateType & (~StateType.ST_Agent1);
                        nodeStateType = nodeStateType | StateType.ST_Agent2;
                    }
                    else if (node.hasFlag(StateType.ST_Agent2))
                    {
                        nodeStateType = nodeStateType & (~StateType.ST_Agent2);
                        nodeStateType = nodeStateType | StateType.ST_Agent1;
                    }
                }

                saida += StateTypeExtension.getIntBinaryString(nodeStateType) + " | ";
            }
            saida += "\n";
        }

        return saida;
    }

    public void enableObjectOnGrid(StateType stateType, Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int z = (int)gridPos.y;

        grid[x, z].addFlag(stateType);
    }

    public void disableObjectOnGrid(StateType stateType, Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int z = (int)gridPos.y;

        grid[x, z].removeFlag(stateType);
    }

    public void updateAgentOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        grid[x, z].removeFlag(player.getStateType());

        pos = player.GetGridPosition();
        x = (int)pos.x;
        z = (int)pos.y;
        grid[x, z].addFlag(player.getStateType());
    }

    public void clearAgentOnGrid(Player player)
    {
        Vector2 pos = player.GetOldGridPosition();
        int x = (int)pos.x;
        int z = (int)pos.y;
        grid[x, z].removeFlag(player.getStateType());

        pos = player.GetGridPosition();
        x = (int)pos.x;
        z = (int)pos.y;
        grid[x, z].removeFlag(player.getStateType());
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            // Node playerNode = NodeFromWorldPoint(player.position);
            foreach (BaseNode n in grid)
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
