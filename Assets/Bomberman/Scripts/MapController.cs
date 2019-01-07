using MLAgents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MapController : MonoBehaviour {

    static uint countCreatedStatistics = 0;

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
    public List<Brain> brains;
    public bool randomizeNumberOfAgents = true;
    public bool randomizeIterationOfAgents = true;

    public GameObject bombPrefab;
    public GameObject monitorPrefab;
    private GameObject myMonitor;
    int iterationWhereWasCreatedBombs;
    int numberOfBombsByCreation;
    string maxIterationString = Config.MAX_STEP_PER_AGENT.ToString();

    public bool generateStatistic;
    public uint matchMaxQuant;
    private bool alreadyGenerateStatistic;

    //0 = draw, 1 = agent1, 2 = agent 2, ...
    private List<uint> matchResults;

    public bool saveReplay;
    private ReplayWriter replayWriter = null;

    public bool followReplayFile;
    private ReplayReader replayReader = null;
    public string replayFileName;
    public ReplayReader.ReplayStep currentReplayStep;
    public int stopToSendSpecialistExperienceInEpsode = 10;
    private bool stoToSendAlreadyCalled;

    // Use this for initialization
    void Start () {
        stoToSendAlreadyCalled = false;
        currentReplayStep = new ReplayReader.ReplayStep();

        alreadyGenerateStatistic = false;
        matchResults = new List<uint>();

        grid = gameObject.GetComponent<Grid>();
        scenarioId = grid.scenarioId;
        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;

        playerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        bombManager = ServiceLocator.getManager(scenarioId).GetBombManager();
        blocksManager = ServiceLocator.getManager(scenarioId).GetBlocksManager();

        playerManager.initInitialPositions(grid);

        ServiceLocator.getManager(scenarioId).GetLogManager().episodePrint(playerManager.getEpisodeCount());

        playerManager.setRandomizeIterationOfAgents(randomizeIterationOfAgents);

        //Vector3 monitorPosition = transform.position + new Vector3(-3, 4, 1); // scene sem imitação
        Vector3 monitorPosition = transform.position + new Vector3(-6.72f, 4, -4.51f);
        myMonitor = Instantiate(monitorPrefab, monitorPosition, Quaternion.identity, transform.parent);
        Monitor.Log("Ult. Vitorioso:", "draw", myMonitor.transform);
        Monitor.Log("Iteração:", "0 / " + maxIterationString, myMonitor.transform);
        Monitor.Log("Episódio:", "1", myMonitor.transform);
        Monitor.Log("Cenário:", transform.parent.gameObject.name, myMonitor.transform);

        iterationWhereWasCreatedBombs = 0;
        numberOfBombsByCreation = 1;

        // replay
        if (saveReplay)
        {
            replayWriter = new ReplayWriter(scenarioId);
        }

        if (followReplayFile)
        {
            replayReader = new ReplayReader(replayFileName);
            ReplayReader.ReplayStep rStep = replayReader.readStep(ReplayCommandLine.RCL_Episode);
            if (rStep.command == ReplayCommandLine.RCL_Episode)
                currentReplayStep.epId = rStep.epId;
            else
                Debug.Log("Nao e pra entrar aqui");
        }

        //Debug.Log("Criando MapController");
        createAgents();

        wasInitialized = true;
        reseting = false;
    }

    private void createAgents()
    {
        // sorteando para ver quantos agentes serão criados para o cenário
        int numberOfAgents = 4;
        Dictionary<string, Vector2Int> agentInitPositionMap = new Dictionary<string, Vector2Int>();

        if (!followReplayFile)
        {
            if (randomizeNumberOfAgents)
                numberOfAgents = UnityEngine.Random.Range(2, 5);
        }
        else
        {
            ReplayReader.ReplayStep replayStep = replayReader.readStep(ReplayCommandLine.RCL_NumberOfAgents);
            if (replayStep.command == ReplayCommandLine.RCL_NumberOfAgents)
                numberOfAgents = replayStep.numberOfAgents;
            else
                Debug.Log("Entrei erradamente aqui");

            replayStep = replayReader.readStep(ReplayCommandLine.RCL_InitialPositions);
            if (replayStep.command == ReplayCommandLine.RCL_InitialPositions)
                agentInitPositionMap = replayStep.agentInitPositionMap;
            else
                Debug.Log("Entrei erradamente aqui");
        }

        if (saveReplay)
        {
            replayWriter.printEpisode(playerManager.getEpisodeCount());
            replayWriter.printNumberOfAgents(numberOfAgents);
        }

        for (int i = 0; i < numberOfAgents; ++i)
        {
            if (!playerManager.containsPlayer(i + 1))
            {
                createAgent(playerPrefabs[i], i + 1, agentInitPositionMap);
            }
        }

        playerManager.createDistanceStructuresForReward();

        saveReplayInitPositions();
    }

    private void saveReplayInitPositions()
    {
        if (saveReplay)
        {
            string line = playerManager.processReplayWriteInitialPosition();

            replayWriter.printStep(line);
        }
    }

    private bool saveReplayActionsStep()
    {
        if (saveReplay)
        {
            string line = playerManager.processReplayWriteActions();

            if (line != "")
            {
                replayWriter.printStep(line);
                return true;
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    private void createAgent(GameObject agentPrefab, int playerNumber, Dictionary<string, Vector2Int> agentInitPositionMap)
    {
        GameObject AgentObj = Instantiate(agentPrefab, transform.parent);
        Player agent = AgentObj.GetComponent<Player>();

        agent.init(grid, playerNumber);

        agent.myGridViewType = brains[playerNumber-1].gameObject.GetComponent<BrainCustomData>().gridViewType;
        agent.GiveBrain(brains[playerNumber-1]);
        agent.eventsAfterGiveBrain(this);
        agent.setBCTeacherHelperTransform(myMonitor.transform);

        if (agentInitPositionMap.Count == 0)
            randomizeInitialPositionFunction(agent);
        else
        {
            grid.clearAgentOnGrid(agent);

            Vector2Int pos = agentInitPositionMap[playerNumber.ToString()];
            agent.setInitialPosition(new Vector3(pos.x, 0.5f, pos.y));
            agent.setOldLocalPosition(agent.getInitialPosition());
            agent.transform.localPosition = agent.getInitialPosition();

            grid.updateAgentOnGrid(agent);
        }

        agent.AgentReset(); // acho que não precisa chamar esse reset.

        playerManager.addPlayer(agent, playerNumber);
    }

    private void createStatisticFiles()
    {
        string folderName = "./statistics/";
        if (!Directory.Exists(folderName))
        {
            Directory.CreateDirectory(folderName);
        }

        string fileName = folderName + "cenario_" + scenarioId + "_matches_results";
        StreamWriter sw = new StreamWriter(fileName + ".csv", false);
        sw.WriteLine("chave;valor;porcentagem");

        uint[] counts = new uint[5];
        for (int i = 0; i < matchResults.Count; ++i)
        {
            if (matchResults[i] == 0)
            {
                counts[0] += 1;
            }
            else if (matchResults[i] == 1)
            {
                counts[1] += 1;
            }
            else if (matchResults[i] == 2)
            {
                counts[2] += 1;
            }
            else if (matchResults[i] == 3)
            {
                counts[3] += 1;
            }
            else if (matchResults[i] == 4)
            {
                counts[4] += 1;
            }
        }

        for (int i = 0; i < counts.Length; ++i)
        {
            if (i != 0)
            {
                float percent = (float)counts[i] / (float)matchResults.Count;
                sw.WriteLine("Agente_" + i + ";" + counts[i] + ";" + String.Format("{0:P}", percent));
            }
            else
            {
                float percent = (float)counts[i] / (float)matchResults.Count;
                sw.WriteLine("Empate;" + counts[i] + ";" + String.Format("{0:P}", percent));
            }
        }
        sw.WriteLine("Total;" + matchResults.Count + ";100%");
        sw.Close();
    }
    
    private void resetStage()
    {
        if (!reseting)
        {
            reseting = true;

            //Debug.Log("Cenário resetado");
            playerManager.clear();
            bombManager.clearBombs();

            if (followReplayFile)
            {
                ReplayReader.ReplayStep rStep = replayReader.nextReplayStepEpisodeOrReopen();
                if (rStep.command == ReplayCommandLine.RCL_Episode)
                    currentReplayStep.epId = rStep.epId;
            }

            createAgents();
            blocksManager.resetBlocks();

            // gerando informações para estatística
            if (generateStatistic == true)
            {
                //+1 porque a verificação de ultimo homem vivo acontece uma iteração após o fim do ep
                if (playerManager.getEpisodeCount() <= matchMaxQuant+1)
                {
                    matchResults.Add(playerManager.lastManAgentResult);
                }
                else
                {
                    if (!alreadyGenerateStatistic)
                    {
                        //gravar em arquivo
                        Debug.Log("Estatisticas do cenario: " + transform.parent.gameObject.name + " foram geradas");
                        createStatisticFiles();
                        MapController.countCreatedStatistics++;

                        alreadyGenerateStatistic = true;

                        if (MapController.countCreatedStatistics == 10) // 10 cenários
                        {
                            #if UNITY_EDITOR
                                  UnityEditor.EditorApplication.isPlaying = false;
                            #else
                                  Application.Quit();
                            #endif
                        }
                    }
                }
            }

            ServiceLocator.getManager(scenarioId).GetLogManager().episodePrint(playerManager.getEpisodeCount());
            Monitor.Log("Episódio:", playerManager.getEpisodeCount().ToString(), myMonitor.transform);
            Monitor.Log("Ult. Vitorioso:", playerManager.lastManAgent, myMonitor.transform);
            iterationWhereWasCreatedBombs = 0;
            numberOfBombsByCreation = 1;

            if (followReplayFile)
            {
                if (stopToSendSpecialistExperienceInEpsode == playerManager.getEpisodeCount() && !stoToSendAlreadyCalled)
                {
                    playerManager.stopToSendExperienceToAllPlayers();
                    stoToSendAlreadyCalled = true;
                }
            }

            reseting = false;
            playerManager.setIsUpdating(false);
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

    private void isToCreateBombsNormalFlux()
    {
        if (isToCreateBombs())
        {
            //grid recupera posições vazias.
            List<Vector2> freePositions = grid.listFreePositions();
            int maxBombByCreation = Mathf.Min(numberOfBombsByCreation, freePositions.Count);

            List<Vector2Int> positionOfBombs = new List<Vector2Int>();

            for (int i = 0; i < maxBombByCreation; ++i)
            {
                int randomIndex = UnityEngine.Random.Range(0, freePositions.Count);
                Vector2 ramdomPos = freePositions[randomIndex];

                freePositions.RemoveAt(randomIndex);
                //maxBombByCreation = Mathf.Min(numberOfBombsByCreation, freePositions.Count);

                GameObject bomb = Instantiate(bombPrefab,
                                              new Vector3(Mathf.RoundToInt(ramdomPos.x) + transform.parent.transform.position.x,
                                                          transform.parent.transform.position.y + 0.3f,
                                                          Mathf.RoundToInt(ramdomPos.y) + transform.parent.transform.position.z),
                                              bombPrefab.transform.rotation,
                                              transform.parent);

                bomb.GetComponent<Bomb>().grid = grid;
                bomb.GetComponent<Bomb>().scenarioId = scenarioId;
                bombManager.addBomb(bomb);
                grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
                grid.enableObjectOnGrid(StateType.ST_Danger, bomb.GetComponent<Bomb>().GetGridPosition());
                bomb.GetComponent<Bomb>().CreateDangerZone(false);

                positionOfBombs.Add(Vector2Int.FloorToInt(bomb.GetComponent<Bomb>().GetGridPosition()));
            }

            if (saveReplay)
                replayWriter.printBombs(playerManager.getIterationCount(), true, positionOfBombs);
        }
        else
        {
            if (saveReplay)
                replayWriter.printBombs(playerManager.getIterationCount(), false, new List<Vector2Int>());
        }
    }

    private void isToCreateBombsReplayFlux()
    {
        if (currentReplayStep.hasCreatedBomb)
        {
            if (currentReplayStep.bombIteration == playerManager.getIterationCount())
            {
                Debug.Log("Entrou.");
            }

            List<Vector2Int> positionOfBombs = new List<Vector2Int>();

            for (int i = 0; i < currentReplayStep.bombList.Count; ++i)
            {
                GameObject bomb = Instantiate(bombPrefab,
                                              new Vector3(Mathf.RoundToInt(currentReplayStep.bombList[i].x) + transform.parent.transform.position.x,
                                                          transform.parent.transform.position.y + 0.5f,
                                                          Mathf.RoundToInt(currentReplayStep.bombList[i].y) + transform.parent.transform.position.z),
                                              bombPrefab.transform.rotation,
                                              transform.parent);

                bomb.GetComponent<Bomb>().grid = grid;
                bomb.GetComponent<Bomb>().scenarioId = scenarioId;
                bombManager.addBomb(bomb);
                grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
                grid.enableObjectOnGrid(StateType.ST_Danger, bomb.GetComponent<Bomb>().GetGridPosition());
                bomb.GetComponent<Bomb>().CreateDangerZone(false);

                positionOfBombs.Add(Vector2Int.FloorToInt(bomb.GetComponent<Bomb>().GetGridPosition()));
            }

            if (saveReplay)
                replayWriter.printBombs(playerManager.getIterationCount(), true, positionOfBombs);
        }
        else
        {
            if (saveReplay)
                replayWriter.printBombs(playerManager.getIterationCount(), false, new List<Vector2Int>());
        }
    }

    private void verifyAndCreateBombs()
    {
        //Criando chuva de bombas após atingir limite de iterações
        if (playerManager.getIterationCount() >= Config.MAX_STEP_PER_AGENT)
        {
            if (!followReplayFile)
                isToCreateBombsNormalFlux();
            else
                isToCreateBombsReplayFlux();
        }
        else
        {
            if (saveReplay)
                replayWriter.printBombs(playerManager.getIterationCount(), false, new List<Vector2Int>());
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
                myUpdate();
            }
            else
            {
                if (timeSinceDecision >= timeBetweenDecisionsAtInference)
                {
                    timeSinceDecision = 0f;

                    myUpdate();
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

    private void myUpdate()
    {
        if (playerManager.updateAgents())
        {
            Monitor.Log("Iteração:", playerManager.getIterationCount().ToString() + " / " + maxIterationString, myMonitor.transform);
            bombManager.timeIterationUpdate();
            ServiceLocator.getManager(scenarioId).GetLogManager().globalStepPrint(playerManager.getIterationCount());

            blocksManager.checkBlocksAndDestroy();

            if (followReplayFile)
            {
                ReplayReader.ReplayStep replayStep = replayReader.readStep(ReplayCommandLine.RCL_Actions);
                if (replayStep.command == ReplayCommandLine.RCL_Actions)
                    currentReplayStep.agentActionMap = replayStep.agentActionMap;
                else
                    Debug.Log("Nao eh pra entrar aqui");

                replayStep = replayReader.readStep(ReplayCommandLine.RCL_BombPositions);
                if (replayStep.command == ReplayCommandLine.RCL_BombPositions)
                {
                    currentReplayStep.bombList = replayStep.bombList;
                    currentReplayStep.bombIteration = replayStep.bombIteration;
                    currentReplayStep.hasCreatedBomb = replayStep.hasCreatedBomb;
                }
                else
                    Debug.Log("Nao eh pra entrar aqui");
            }

            //como request decision é assincrono, temos que testar se ultima ação do agente está vazia.
            if (saveReplayActionsStep())
                verifyAndCreateBombs();

            playerManager.setIsUpdating(false);
        }
        else
        {
            //Debug.Log("Resetando cenas");
            resetStage();
        }
    }

    void OnApplicationQuit()
    {
        if (saveReplay)
            replayWriter.finish();

        if (followReplayFile)
            replayReader.finish();
    }
}
