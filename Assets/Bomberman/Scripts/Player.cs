/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MLAgents;

public class Player : Agent
{
    private int scenarioId;

    [Header("Specific to Player")]
    private BombermanAcademy academy;
    // private BombermanDecision bombermanDecision;
    private BombermanOnlyOneDecision bombermanDecision;
    private BCTeacherHelper bcTeacherHelper;
    public int numSeqCompleted = 4;
    public bool isSpecialist;

    // Variáveis usadas para se utilizar a técnica da mímica 
    public Player teacherAgent;
    private List<float> teacherAgentObservations;
    private ActionType lastTeacherAction;
    private bool wasFilledTeacherObservations;
    public bool isMimicking;
    public bool forceReward = false;

    public Transform monitorFocus;
    public GameObject hammerEffect;

    //Player parameters
    //[Range (1, 4)] //Enables a nifty slider in the editor. Indicates what player this is: P1 or P2
    private int playerNumber = 1;
    private StateType stateType;

    public bool canDropBombs;
    public bool dead = false;
    public bool lastMan = false;

    public bool isInDanger;
    public bool isReady = true;
    [HideInInspector]
    public bool wasInitialized = false;

    public GameObject bombPrefab;
    public Grid grid;

    private Animator animator;
    private int bombs = 2;

    private Vector3 initialPosition;
    private Vector3 oldLocalPosition;
    private Vector2 myGridPosition;

    private bool alreadyWasReseted = false;
    public bool randomizeResetPosition = false;
    public bool randomizeInitialPosition = false;
    public GridViewType myGridViewType;

    public static Vector3[] initialPositions = new Vector3[] {  new Vector3(1.0f, 0.5f, 9.0f),
                                                                 new Vector3(9.0f, 0.5f, 9.0f) ,
                                                                 new Vector3(9.0f, 0.5f, 1.0f) ,
                                                                 new Vector3(1.0f, 0.5f, 1.0f)};

    private float closestDistance = float.MaxValue;
    private float previousDistance = float.MaxValue;

    private int localEpisode = 1;
    private int localStep = 1;
    private int totalLocalStep = 1;

    private GameObject playerModel;

    //variaveis usadas para salvar arquivo de replay
    public bool saveReplay = true;
    private ReplayWriter replayWriter = null;
    private string observationGridString;
    private string actionIdString;

    public PlayerManager myPlayerManager;
    public BombManager myBombManager;
    Player bombermanVillain;

    int bombCount = 0;

    private MapController myMapController;

    public int getPlayerNumber()
    {
        return playerNumber;
    }

    public Vector3 getInitialPosition()
    {
        return initialPosition;
    }

    public void setInitialPosition(Vector3 p)
    {
        initialPosition = p;
    }

    public Vector3 getOldLocalPosition()
    {
        return oldLocalPosition;
    }

    public void setOldLocalPosition(Vector3 p)
    {
        oldLocalPosition = p;
    }

    static Vector3 getOppositeInitialPosition(int index)
    {
        if (index == 0)
            return initialPositions[2];
        else if (index == 1)
            return initialPositions[3];
        else if (index == 2)
            return initialPositions[0];
        else
            return initialPositions[1];
    }

    private void clearReplayVars()
    {
        observationGridString = "";
        actionIdString = "";
    }

    public StateType getStateType()
    {
        return stateType;
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public Vector2 GetOldGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(oldLocalPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public int getLocalStep()
    {
        return localStep;
    }

    private StateType convertPlayerNumberToStateType(int pNumber)
    {
        StateType stateType = 0;

        if (pNumber == 1)
            stateType = StateType.ST_Agent1;
        else if (pNumber == 2)
            stateType = StateType.ST_Agent2;
        else if (pNumber == 3)
            stateType = StateType.ST_Agent3;
        else if (pNumber == 4)
            stateType = StateType.ST_Agent4;

        return stateType;
    }

    //Função foi criada para inicializar atributos do agente após a instanciação. Porque ao usar a instanciação, automaticamente é chamado InitializeAgent e Start.
    public void init(Grid g, int pNumber, MapController mapController)
    {
        //Debug.Log("Init foi chamado");
        myMapController = mapController;
        grid = g;
        scenarioId = grid.scenarioId;
        playerNumber = pNumber;
        stateType = convertPlayerNumberToStateType(playerNumber);
        myGridViewType = grid.getGridViewType();

        bombCount = 0;
        isInDanger = false;
        canDropBombs = true;
        isReady = true;
        lastMan = false;
        alreadyWasReseted = false;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;

        if (saveReplay)
        {
            if (replayWriter == null)
                replayWriter = new ReplayWriter(playerNumber, scenarioId);

            replayWriter.initSeq(localEpisode);
        }


        localStep = 1;

        ServiceLocator.getManager(scenarioId).GetLogManager().localEpisodePrint(localEpisode++, this);

        //------------------------------------------------------ São atualizadas no Start, que é chamado depois
        initialPosition = transform.localPosition;
        oldLocalPosition = transform.localPosition;
        // -----------------------------------------------------------------------------------------------------

        playerModel = transform.Find("PlayerModel").gameObject;
        animator = transform.Find("PlayerModel").GetComponent<Animator>();

        if (isSpecialist)
        {
            bombermanDecision = brain.GetComponent<BombermanOnlyOneDecision>();
            teacherAgentObservations = new List<float>();
        }

        bombermanVillain = null;
        wasFilledTeacherObservations = false;

        wasInitialized = true;

        myPlayerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        myBombManager = ServiceLocator.getManager(scenarioId).GetBombManager();
    }

    private void Start()
    {
        //Debug.Log("Start foi chamado");
        
        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;
        if (isSpecialist)
            bcTeacherHelper = GetComponent<BCTeacherHelper>();

    }


    public override void InitializeAgent()
    {
        // Debug.Log("InitializeAgent foi chamado");
        // ---------------------------------------------- Observação
        //    Todo código que era colocado aqui agora está na função init().
        //
    }

    public override void AgentOnDone()
    {
        //Debug.Log("AgentOnDone foi chamado");
        //base.AgentOnDone();

        //ver como recriar agente que não reseta automaticamente. https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Learning-Environment-Design-Agents.md
        //Instantiating an Agent at Runtime

        grid.clearAgentOnGrid(this);
        myPlayerManager.removePlayer(playerNumber);

        Destroy(gameObject);
    }

    public override void AgentReset()
    {
        // Não precisamos usar essa função, pois o agente não é mais resetado quando entra em estado Done. 
    }

    private void AddVectorObsForGrid()
    {
        if (myGridViewType == GridViewType.GVT_Hybrid)
        {
            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BaseNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);
                    float[] temp = StateTypeExtension.convertStateTypeToHybrid(nodeStateType);
                    AddVectorObs(temp);
                }
            }
        }
        else if (myGridViewType == GridViewType.GVT_Binary)
        {
            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BaseNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);
                    int cell = (int)nodeStateType;

                    AddVectorObs(cell);

                    if (isSpecialist)
                        teacherAgentObservations.Add(cell);

                    if (saveReplay)
                    {
                        if (observationGridString.Length > 0)
                            observationGridString += "," + cell;
                        else
                            observationGridString = cell.ToString();
                    }
                }
            }
        }
        else if (myGridViewType == GridViewType.GVT_ICAART)
        {
            List<float> freeBreakableObstructedCells = new List<float>();
            List<float> positionAgentCells = new List<float>();
            List<float> positionEnemyCells = new List<float>();
            List<float> dangerLevelOfPositionsCells = new List<float>();

            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BaseNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);

                    //enviar grid que representa posições livres, com blocos ou com paredes
                    freeBreakableObstructedCells.Add(node.getFreeBreakableObstructedCell());

                    //enviar grid que representa posição do agente
                    int hasAgent = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Agent) ? 1 : 0;
                    positionAgentCells.Add(hasAgent);

                    //enviar grid que representa posição dos inimigos
                    hasAgent = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_EnemyAgent) ? 1 : 0;
                    positionEnemyCells.Add(hasAgent);

                    //enviar grid que representa áreas de perigo
                    bool hasDanger = grid.NodeFromPos(x, y).getDangerPosition();
                    if (!hasDanger)
                        dangerLevelOfPositionsCells.Add(0.0f);
                    else
                    {
                        Danger danger = myBombManager.getDanger(x, y);
                        if (danger != null)
                        {
                            float dangerLevel = danger.GetDangerLevelOfPosition(this);
                            dangerLevelOfPositionsCells.Add(dangerLevel);
                        }
                        else
                            dangerLevelOfPositionsCells.Add(0.0f);
                    }
                }
            }

            AddVectorObs(freeBreakableObstructedCells);
            AddVectorObs(positionAgentCells);
            AddVectorObs(positionEnemyCells);
            AddVectorObs(dangerLevelOfPositionsCells);
        }
        else if (myGridViewType == GridViewType.GVT_BinaryDecimal)
        {
            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BaseNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);

                    string cellString = StateTypeExtension.getIntBinaryString(nodeStateType);
                    int cell = Convert.ToInt32(cellString);

                    AddVectorObs(cell);
                }
            }
        }
        else if (myGridViewType == GridViewType.GVT_BinaryNormalized)
        {
            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BaseNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);

                    float cell = StateTypeExtension.normalizeBinaryFlag(nodeStateType);
                    AddVectorObs(cell);
                }
            }
        }
    }

    //função usada apenas com teacher.getTeacherAgentObservations()
    private List<float> getTeacherAgentObservations()
    {
        if (teacherAgentObservations.Count > 0)
        {
            wasFilledTeacherObservations = false;
            return teacherAgentObservations;
        }
        else
        {
            teacherAgentObservations = new List<float>(90);
            return teacherAgentObservations;
        }
    }

    public override void CollectObservations()
    {
        //Debug.Log("agent" + playerNumber + " observacoes");
        clearReplayVars();
        myGridPosition = GetGridPosition();

        Vector2 normalizedGridPosition = (myGridPosition) / (grid.GetGridMaxValue());
        AddVectorObs(normalizedGridPosition);

        //adicionando grid de observação da posição dos agentes
        if (isSpecialist)
        {
            teacherAgentObservations.Clear();
            AddVectorObsForGrid();

            if (teacherAgentObservations.Count > 0)
                wasFilledTeacherObservations = true;
        }
        else
        {
            if (isMimicking)
            {
                List<float> observations = teacherAgent.getTeacherAgentObservations();
                AddVectorObs(observations);
            }
            else
            {
                //Debug.Log("Nao era pra eu ter entrado aqui.");
                //teacherAgent.wasFilledTeacherObservations = false;
                AddVectorObsForGrid();
            }
        }
        
    }

    public static void AddRewardToAgent(Player player, float reward, string message)
    {
        if (player.forceReward || !player.isMimicking)
        {
            player.AddReward(reward);
            ServiceLocator.getManager(player.scenarioId).GetLogManager().rewardPrint(message, reward);
        }
    }

    private void penalizeInvalidWalkMovement()
    {
        AddRewardToAgent(this, Config.REWARD_INVALID_WALK_ACTION, "Agente" + playerNumber + " tentou andar sem poder");
    }

    private void reinforceValidWalkMovement()
    {
        AddRewardToAgent(this, Config.REWARD_VALID_WALK_POSITION, "Agente" + playerNumber + " andou para um lugar livre");
    }

    private void penalizeInvalidHammerAction()
    {
        AddRewardToAgent(this, Config.REWARD_INVALID_HAMMER_ACTION, "Agente" + playerNumber + " tentou usar o martelo desnecessariamente");
    }

    private void reinforceValidHammerAction()
    {
        AddRewardToAgent(this, Config.REWARD_VALID_HAMMER_ACTION, "Agente" + playerNumber + " usou o martelo corretamente");
    }

    private void penalizeStopAction()
    {
        AddRewardToAgent(this, Config.REWARD_STOP_ACTION, "Agente" + playerNumber + " ficou parado, e ficar parado eh perder tempo");
    }

    private float calculateMimickRewards(ActionType action)
    {
        float reward = Config.REWARD_CORRECT_TEACHER_ACTION;

        //para ações contínuas há sentido em fazer a diferença absoluta entre ações
        //reward = (-Abs(lastTeacherAction - action));

        if (teacherAgent.lastTeacherAction != action)
        {
            reward = Config.REWARD_WRONG_TEACHER_ACTION;
        }

        return reward;
    }

    private void tryHammerAttack(Vector2 hammerPos)
    {
        if (grid.checkDestructible(hammerPos))
        {
            //destrua o bloca e pegue a recompensa
            Destructable destructible = grid.getDestructibleInPosition(hammerPos);
            destructible.attackByHammer(this);
            Instantiate(hammerEffect, destructible.transform.position, Quaternion.identity, destructible.transform.parent);
            reinforceValidHammerAction();
        }
        else
        {
            Instantiate(hammerEffect, transform.position, Quaternion.identity, transform.parent);
            penalizeInvalidHammerAction();
        }
            
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log("agent" + playerNumber + " acoes");
        if (!dead)
        {
            ServiceLocator.getManager(scenarioId).GetLogManager().statePrint("Agent " + playerNumber,
                                                    myGridPosition,

                                                    grid.gridToString(playerNumber),
                                                    canDropBombs,
                                                    isInDanger,
                                                    myBombManager.existsBombOrDanger());


            /*if (!isMimicking)
            {
                // talvez seria melhor passar esse código para o playerManager.
                if (getLocalStep() >= Config.MAX_STEP_PER_AGENT)
                {
                    AddRewardToAgent(this, Config.REWARD_MAX_STEP_REACHED, "Agente" + playerNumber + " alcancou max step");
                    killAgent();
                }
            }*/

            ServiceLocator.getManager(scenarioId).GetLogManager().localStepPrint(this);
            localStep++;
            totalLocalStep++;

            /*if (isSpecialist)
                Debug.Log("LocalStep: " + localStep);*/

            ActionType action = ActionTypeExtension.convert((int)vectorAction[0]);
            ActionType myAction = action;

            if (isSpecialist)
                lastTeacherAction = action;

            if (isMimicking)
                action = teacherAgent.lastTeacherAction;

            if (monitorFocus != null)
            {
                Monitor.Log("Episode: ", (localEpisode-1).ToString(), monitorFocus);
                Monitor.Log("RT Step: ", (totalLocalStep - 1).ToString(), monitorFocus);
                Monitor.Log("RL Step: ", (getLocalStep()-1).ToString(), monitorFocus);
                //Monitor.Log(" T Step: ", GetTotalStepCount().ToString(), monitorFocus);
                Monitor.Log(" L Step: ", GetStepCount().ToString(), monitorFocus);
                Monitor.Log("ActionP" + playerNumber + ": ", Convert.ToString((int)action), monitorFocus);
            }

            if (saveReplay)
            {
                actionIdString = ((int)vectorAction[0]).ToString();
                replayWriter.printStep(observationGridString, actionIdString);
            }

            if (!IsDone())
            {
                if (!lastMan)
                {
                    if (grid.checkFire(myGridPosition)) //Bomb code
                    {
                        AddRewardToAgent(this, Config.REWARD_DIE, "Agente" + playerNumber + " atingido por explosao");

                        if (bombermanVillain != null)
                        {
                            if (bombermanVillain.playerNumber != playerNumber)
                            {
                                AddRewardToAgent(bombermanVillain, Config.REWARD_KILL_ENEMY, "Agente" + bombermanVillain.playerNumber + " matou inimigo");
                            }
                        }

                        killAgent();
                    }

                    //----------
                    if (grid.checkDanger(myGridPosition))
                    {
                        isInDanger = true;
                    }
                    else
                    {
                        isInDanger = false;
                    }

                    animator.SetBool("Walking", false);

                    //-----------------------------------------------------------------------------------------------------
                    if (!dead && !IsDone())
                    {
                        Vector2 newPos;// hammerPos;
                        switch (action)
                        {
                            //cima
                            case ActionType.AT_Up:
                                //rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
                                newPos = myGridPosition + new Vector2(0, 1);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(0, 0, 1);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 0, 0);
                                animator.SetBool("Walking", true);
                                break;
                            //baixo
                            case ActionType.AT_Down:
                                //rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
                                newPos = myGridPosition + new Vector2(0, -1);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(0, 0, -1);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 180, 0);
                                animator.SetBool("Walking", true);
                                break;
                            //direita
                            case ActionType.AT_Right:
                                //rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                                newPos = myGridPosition + new Vector2(1, 0);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(1, 0, 0);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 90, 0);
                                animator.SetBool("Walking", true);
                                break;
                            //esquerda
                            case ActionType.AT_Left:
                                //rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                                newPos = myGridPosition + new Vector2(-1, 0);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(-1, 0, 0);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 270, 0);
                                animator.SetBool("Walking", true);
                                break;
                            //Drop bomb
                            case ActionType.AT_Bomb:
                                if (canDropBombs && !dead && !grid.checkBomb(myGridPosition))
                                {
                                    DropBomb();
                                }
                                else
                                {
                                    AddRewardToAgent(this, Config.REWARD_INVALID_BOMB_ACTION, "Agente" + playerNumber + " tentou colocar bomba sem poder");
                                }
                                break;
                            //Hammer Up
                            /*case ActionType.AT_Hammer_Up:
                                hammerPos = myGridPosition + new Vector2(0, 1);
                                tryHammerAttack(hammerPos);

                                transform.rotation = Quaternion.Euler(0, 0, 0);
                                break;
                            //Hammer Down
                            case ActionType.AT_Hammer_Down:
                                hammerPos = myGridPosition + new Vector2(0, -1);
                                tryHammerAttack(hammerPos);

                                transform.rotation = Quaternion.Euler(0, 180, 0);
                                break;
                            //Hammer Right
                            case ActionType.AT_Hammer_Right:
                                hammerPos = myGridPosition + new Vector2(1, 0);
                                tryHammerAttack(hammerPos);

                                transform.rotation = Quaternion.Euler(0, 90, 0);
                                break;
                            //Hammer Left
                            case ActionType.AT_Hammer_Left:
                                hammerPos = myGridPosition + new Vector2(-1, 0);
                                tryHammerAttack(hammerPos);

                                transform.rotation = Quaternion.Euler(0, 270, 0);
                                break;*/
                            //Wait
                            case ActionType.AT_Wait:
                                penalizeStopAction();
                                break;
                            default:
                                break;
                        }
                    }

                    // recompensa para avaliar se aluno está imitando corretamente professor
                    if (isMimicking)
                    {
                        AddReward(calculateMimickRewards(myAction));
                    }

                    //recompensas
                    myGridPosition = GetGridPosition();
                    Vector2 oldGridPosition = GetOldGridPosition();

                    // testar aproximação para cada inimigo. Recompensas só serão dadas caso o agente tenta mudado de posição
                    if (!myGridPosition.Equals(oldGridPosition))
                        myPlayerManager.calculateDistanceEnemyPosition(this);


                    if (ServiceLocator.getManager(scenarioId).GetBombManager().existsBombOrDanger())
                    {
                        if (!isInDanger)
                        {
                            AddRewardToAgent(this, Config.REWARD_SAFE_AREA, "Agente" + playerNumber + " esta seguro");
                        }
                        else
                        {
                            AddRewardToAgent(this, Config.REWARD_DANGER_AREA, "Agente" + playerNumber + " continua em area de perigo");
                        }
                    }
                }

                AddRewardToAgent(this, Config.REWARD_TIME_PENALTY, "Agente" + playerNumber + " sofreu penalidade de tempo");
            }

            ServiceLocator.getManager(scenarioId).GetLogManager().rewardResumePrint(GetReward(), GetCumulativeReward());
            ServiceLocator.getManager(scenarioId).GetLogManager().actionPrint("Agent" + playerNumber, action);

            grid.updateAgentOnGrid(this);
            oldLocalPosition = transform.localPosition;
        }
        else
        {
            Debug.Log("Estou morto " + GetStepCount());
        }
    }

    private void DropBomb ()
    {
        if (bombPrefab)
        { 
            float temp = Mathf.RoundToInt(transform.position.x) - transform.position.x >= 0.0f ? -0.0f : 0.0f;

            GameObject bomb = Instantiate(bombPrefab, 
                                        new Vector3(Mathf.RoundToInt(transform.position.x) + temp,
                                                    Mathf.RoundToInt(transform.position.y),
                                                    Mathf.RoundToInt(transform.position.z)),
                                          bombPrefab.transform.rotation,
                                          transform.parent);
            bomb.GetComponent<Bomb>().bomberman = this;
            bomb.GetComponent<Bomb>().grid = grid;
            bomb.GetComponent<Bomb>().scenarioId = scenarioId;

            myBombManager.addBomb(bomb);
            grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
            grid.enableObjectOnGrid(StateType.ST_Danger, bomb.GetComponent<Bomb>().GetGridPosition());
            bomb.GetComponent<Bomb>().CreateDangerZone();


            AddRewardToAgent(this, Config.REWARD_VALID_BOMB_ACTION, "Agente" + playerNumber + " colocou uma bomba");

            bombCount++;
            canDropBombs = false;
            isInDanger = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //talvez na movimentação contínua não funcione esse código. Uma boa solução seria usar um contador(int) de perigo ao invés de um bool
        /*if (other.CompareTag("Danger"))
        {
            isInDanger = false;
        }*/
    }

    public void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                bombermanVillain = other.gameObject.GetComponent<DestroySelf>().bomberman;
            }
        }

        /*
        //Agente toma muita recompensa negativa e desiste de colocar bomba muito rápido
        if (other.CompareTag("Danger"))
        {
            isInDanger = true;
            /*float dangerLevel = Mathf.Abs(other.gameObject.GetComponent<Danger>().GetDangerLevelOfPosition(this));
            dangerLevel *= -0.1f;
            AddReward(dangerLevel);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " em area de perigo", dangerLevel);
        }*/
    }

    private void defaultKillCode()
    {
        dead = true;
        grid.clearAgentOnGrid(this);
        playerModel.SetActive(false);
        transform.localPosition = initialPosition;

        // tentando corrigir problema da bomba explodir após agente ser reiniciado por tempo
        //Bomb code
        /*if (!academy.GetIsInference())
            myBombManager.clearBombs();*/
    }

    private void killAgentOnly()
    {
        defaultKillCode();
        //Debug.Log("Morri force - agente " + playerNumber);
    }

    private void killAgent()
    {
        defaultKillCode();
        //Debug.Log("Morri - agente " + playerNumber);

        myPlayerManager.addDeadCount();
        Done();

        /* Comentado porque começou a dar pau com apenas um agente após várias iterações
         * if (myPlayerManager.getDeadCount() == 0)
        {
            myPlayerManager.addDeadCount();
            Invoke("VerifyDeadCount", 0.5f);
        }*/
    }

    private void internalUpdate()
    {
        if (isMimicking)
        {
            if (teacherAgent != null && teacherAgent.wasFilledTeacherObservations)
            {
                RequestDecision();
            }
        }
        else
        {
            if (isSpecialist)
            {
                if (!wasFilledTeacherObservations)
                {
                    RequestDecision();
                }
            }
            else
            {
                RequestDecision();
            }
        }
    }

    public bool WaitIterationActions()
    {
        if (wasInitialized && !dead)
        {
            internalUpdate();
            return true;
        }

        return false;
    }

    void OnApplicationQuit()
    {
        if (saveReplay)
            replayWriter.finish();
    }
}
