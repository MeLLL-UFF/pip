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
                    bombManager.timeIterationUpdate();
                    ServiceLocator.getManager(scenarioId).GetLogManager().globalStepPrint(playerManager.getIterationCount());
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
                        bombManager.timeIterationUpdate();
                        ServiceLocator.getManager(scenarioId).GetLogManager().globalStepPrint(playerManager.getIterationCount());
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
