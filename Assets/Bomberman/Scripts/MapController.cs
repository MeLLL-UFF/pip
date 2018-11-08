using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

    private bool wasInitialized = false;
    private bool reseting = false;
    private int scenarioId;
    private Grid grid;
    private PlayerManager playerManager;
    private BombManager bombManager;
    private BlocksManager blocksManager;
    private BombermanAcademy academy;
    private float timeSinceDecision = 0;

    public float timeBetweenDecisionsAtInference;
    public List<GameObject> playerPrefabs;
    public Brain brain;
    public Brain staticBrain;
    public bool randomizeNumberOfAgents = true;
    public bool randomizeIterationOfAgents = true;

    public GameObject bombPrefab;
    public GameObject monitorPrefab;
    private GameObject myMonitor;
    int iterationWhereWasCreatedBombs;
    int numberOfBombsByCreation;
    string maxIterationString = Config.MAX_STEP_PER_AGENT.ToString();

    // Use this for initialization
    void Start () {
        grid = gameObject.GetComponent<Grid>();
        scenarioId = grid.scenarioId;
        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;

        playerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        bombManager = ServiceLocator.getManager(scenarioId).GetBombManager();
        blocksManager = ServiceLocator.getManager(scenarioId).GetBlocksManager();

        ServiceLocator.getManager(scenarioId).GetLogManager().episodePrint(playerManager.getEpisodeCount());

        playerManager.setRandomizeIterationOfAgents(randomizeIterationOfAgents);

        Vector3 monitorPosition = transform.position + new Vector3(-3, 4, 1);
        myMonitor = Instantiate(monitorPrefab, monitorPosition, Quaternion.identity, transform.parent);
        Monitor.Log("Ult. Vitorioso:", "draw", myMonitor.transform);
        Monitor.Log("Iteração:", "0 / " + maxIterationString, myMonitor.transform);
        Monitor.Log("Episódio:", "1", myMonitor.transform);
        Monitor.Log("Cenário:", transform.parent.gameObject.name, myMonitor.transform);

        iterationWhereWasCreatedBombs = 0;
        numberOfBombsByCreation = 1;

        //Debug.Log("Criando MapController");
        createAgents();

        wasInitialized = true;
        reseting = false;
    }

    private void createAgents()
    {
        // sorteando para ver quantos agentes serão criados para o cenário
        int numberOfAgents = 4;
        if (randomizeNumberOfAgents)
            numberOfAgents = UnityEngine.Random.Range(2, 5);

        for (int i = 0; i < numberOfAgents; ++i)
        {
            if (!playerManager.containsPlayer(i + 1))
            {
                createAgent(playerPrefabs[i], brain, i + 1);
            }
        }

        playerManager.createDistanceStructuresForReward();
    }

    private void createAgent(GameObject agentPrefab, Brain brain, int playerNumber)
    {
        
        GameObject AgentObj = Instantiate(agentPrefab, transform.parent);
        Player agent = AgentObj.GetComponent<Player>();

        if (playerNumber == 1)
            agent.GiveBrain(brain);
        else
            agent.GiveBrain(staticBrain);

        agent.init(grid, playerNumber, this);

        randomizeInitialPositionFunction(agent);

        agent.AgentReset(); // acho que não precisa chamar esse reset.

        playerManager.addPlayer(agent, playerNumber);
    }

    
    private void resetStage()
    {
        if (!reseting)
        {
            reseting = true;
            //Debug.Log("Cenário resetado");
            playerManager.clear();
            bombManager.clearBombs();
            createAgents();
            blocksManager.resetBlocks();
            ServiceLocator.getManager(scenarioId).GetLogManager().episodePrint(playerManager.getEpisodeCount());
            Monitor.Log("Episódio:", playerManager.getEpisodeCount().ToString(), myMonitor.transform);
            Monitor.Log("Ult. Vitorioso:", playerManager.lastManAgent, myMonitor.transform);
            iterationWhereWasCreatedBombs = 0;
            numberOfBombsByCreation = 1;
            reseting = false;
        }
    }

    public void randomizeInitialPositionFunction(Player player)
    {
        if (player.randomizeInitialPosition)
        {
            grid.clearAgentOnGrid(player);

            player.setInitialPosition(playerManager.getNextRandomInitPosition());
            player.setOldLocalPosition(player.getInitialPosition());
            player.transform.localPosition = player.getInitialPosition();

            grid.updateAgentOnGrid(player);
        }
    }

    private void randomizeResetPositionFunction(Player player)
    {
        if (player.randomizeResetPosition)
        {
            grid.clearAgentOnGrid(player);

            player.setInitialPosition(playerManager.getNextRandomInitPosition());
            player.setOldLocalPosition(player.getInitialPosition());
            player.transform.localPosition = player.getInitialPosition();

            grid.updateAgentOnGrid(player);
        }
        else
        {
            grid.updateAgentOnGrid(player);
        }
    }

    // call this function only after test if iterationCount is greater or equal to MAX_STEP_PER_AGENT
    private bool isToCreateBombs()
    {
        bool isToCreateBombs = false;
        if (iterationWhereWasCreatedBombs == 0)
        {
            iterationWhereWasCreatedBombs = playerManager.getIterationCount();
            isToCreateBombs = true;
        }
        else
        {
            if ((playerManager.getIterationCount() - iterationWhereWasCreatedBombs) == (Config.BOMB_TIMER_DISCRETE + 1))
            {
                iterationWhereWasCreatedBombs = playerManager.getIterationCount();
                isToCreateBombs = true;
                numberOfBombsByCreation++;
            }
        }

        return isToCreateBombs;
    }

    private void verifyAndCreateBombs()
    {
        //Criando chuva de bombas após atingir limite de iterações
        if (playerManager.getIterationCount() >= Config.MAX_STEP_PER_AGENT)
        {
            if (isToCreateBombs())
            {
                //grid recupera posições vazias.
                List<Vector2> freePositions = grid.listFreePositions();
                int maxBombByCreation = Mathf.Min(numberOfBombsByCreation, freePositions.Count);

                for (int i = 0; i < maxBombByCreation; ++i)
                {
                    int randomIndex = UnityEngine.Random.Range(0, freePositions.Count);
                    Vector2 ramdomPos = freePositions[randomIndex];

                    freePositions.RemoveAt(randomIndex);
                    maxBombByCreation = Mathf.Min(numberOfBombsByCreation, freePositions.Count);

                    GameObject bomb = Instantiate(bombPrefab,
                                                  new Vector3(Mathf.RoundToInt(ramdomPos.x),
                                                              transform.parent.transform.position.y + 0.5f,
                                                              Mathf.RoundToInt(ramdomPos.y)),
                                                  bombPrefab.transform.rotation,
                                                  transform.parent);

                    bomb.GetComponent<Bomb>().grid = grid;
                    bomb.GetComponent<Bomb>().scenarioId = scenarioId;
                    bombManager.addBomb(bomb);
                    grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
                    grid.enableObjectOnGrid(StateType.ST_Danger, bomb.GetComponent<Bomb>().GetGridPosition());
                    bomb.GetComponent<Bomb>().CreateDangerZone();
                }
            }
        }
    }

    // Aqui terei o controle de todo fluxo dos agentes: RequestAction e RequestDecision
    private void FixedUpdate()
    {
        if (wasInitialized && !reseting && !playerManager.isUpdating())
        {
            // se está em treinamento
            if (!academy.GetIsInference())
            {
                if (playerManager.updateAgents())
                {
                    Monitor.Log("Iteração:", playerManager.getIterationCount().ToString() + " / " + maxIterationString, myMonitor.transform);
                    bombManager.timeIterationUpdate();
                    ServiceLocator.getManager(scenarioId).GetLogManager().globalStepPrint(playerManager.getIterationCount());

                    verifyAndCreateBombs();
                }
                else
                {
                    resetStage();
                }
            }
            else
            {
                if (timeSinceDecision >= timeBetweenDecisionsAtInference)
                {
                    timeSinceDecision = 0f;

                    bool updateFlag = playerManager.updateAgents();
                    if (updateFlag)
                    {
                        Monitor.Log("Iteração:", playerManager.getIterationCount().ToString() + " / " + maxIterationString, myMonitor.transform);
                        bombManager.timeIterationUpdate();
                        ServiceLocator.getManager(scenarioId).GetLogManager().globalStepPrint(playerManager.getIterationCount());

                        verifyAndCreateBombs();
                    }
                    else
                    {
                        //Debug.Log("Resetando cenas");
                        resetStage();
                    }
                }
                else
                {
                    timeSinceDecision += Time.fixedDeltaTime;
                }
            }
        }
        else
        {
            Debug.Log("Esta atualizando");
        }
    }
}
