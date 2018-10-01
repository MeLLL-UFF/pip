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
    public int scenarioId;

    [Header("Specific to Player")]
    private BombermanAcademy academy;
    // private BombermanDecision bombermanDecision;
    private BombermanOnlyOneDecision bombermanDecision;

    private BCTeacherHelper bcTeacherHelper;
    public int numSeqCompleted = 4;
    private int countSeq = 0;
    public bool isSpecialist;
    public Player teacherAgent;
    private List<float> teacherAgentObservations;
    private ActionType lastTeacherAction;
    private bool wasFilledTeacherObservations;
    public bool isMimicking;
    public bool forceReward = false;

    public Transform monitorFocus;
    public GameObject hammerEffect;

    //Player parameters
    [Range (1, 2)] //Enables a nifty slider in the editor. Indicates what player this is: P1 or P2
    public int playerNumber = 1;
    private StateType stateType;

    public float moveSpeed = 5f;
    public bool canDropBombs;
    public bool dead = false;
    public bool targetReached = false;
    public bool canMove = true;

    public GameObject bombPrefab;
    public Transform Target;
    public Grid grid;

    private Rigidbody rigidBody;
    private Animator animator;
    private int bombs = 2;
    private Vector3 initialPosition;
    
    private Vector3 oldLocalPosition;
    private bool hasPlacedBomb = false;
    public bool isInDanger;

    private Vector2 myGridPosition;
    private Vector2 targetGridPosition = new Vector2(7, 0);
    private float closestDistance = float.MaxValue;
    private float previousDistance = float.MaxValue;

    private int localEpisode = 1;
    private int localStep = 1;
    private int totalLocalStep = 1;
    
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    public bool isReady = true;
    [HideInInspector]
    public bool wasInitialized = false;

    private GameObject playerModel;

    //variaveis usadas para salvar arquivo de replay
    public bool saveReplay = true;
    private ReplayWriter replayWriter = null;
    private string observationGridString;
    private string actionIdString;

    public bool myIterationActionWasExecuted;
    public PlayerManager myPlayerManager;
    public BombManager myBombManager;
    Player bombermanVillain;

    bool is_received_subgoal_distance1 = false;
    bool is_received_subgoal_distance2 = false;

    bool is_received_subgoal_step1 = false;
    bool is_received_subgoal_step2 = false;
    bool is_received_subgoal_step3 = false;
    bool is_received_subgoal_step4 = false;
    int bombCount = 0;

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

    private void Start()
    {
        //Debug.Log("Start foi chamado");
        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;
        if (isSpecialist)
            bcTeacherHelper = GetComponent<BCTeacherHelper>();

        myPlayerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        myBombManager = ServiceLocator.getManager(scenarioId).GetBombManager();

        if (playerNumber == 1)
        {
            myPlayerManager.setAgent1(this);
        }
        else if (playerNumber == 2)
        {
            myPlayerManager.setAgent2(this);
        }
    }

    public override void InitializeAgent()
    {
        //Debug.Log("InitializeAgent foi chamado");
        base.InitializeAgent();

        countSeq = 0;

        isInDanger = false;
        canDropBombs = true;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;
        initialPosition = transform.position;
        oldLocalPosition = transform.localPosition;

        isReady = true;
        targetReached = false;

        is_received_subgoal_distance1 = false;
        is_received_subgoal_distance2 = false;

        is_received_subgoal_step1 = false;
        is_received_subgoal_step2 = false;
        is_received_subgoal_step3 = false;
        is_received_subgoal_step4 = false;
        bombCount = 0;

        playerModel = transform.Find("PlayerModel").gameObject;
        rigidBody = GetComponent<Rigidbody>();
        animator = transform.Find("PlayerModel").GetComponent<Animator>();

        //Vector3 gridTarget3d = Target.transform.localPosition - Vector3.one;
        Vector3 gridTarget3d = Target.transform.localPosition;
        targetGridPosition = new Vector2(Mathf.RoundToInt(gridTarget3d.x), Mathf.RoundToInt(gridTarget3d.z));

        if (isSpecialist)
        {
            bombermanDecision = brain.GetComponent<BombermanOnlyOneDecision>();
            teacherAgentObservations = new List<float>();
        }
            

        if (playerNumber == 1)
        {
            stateType = StateType.ST_Agent1;
        }
        else if (playerNumber == 2)
        {
            stateType = StateType.ST_Agent2;
        }

        myIterationActionWasExecuted = false;
        bombermanVillain = null;
        wasFilledTeacherObservations = false;

        wasInitialized = true;
    }

    /*public override void AgentOnDone()
    {
        Debug.Log("AgentOnDone foi chamado");
        base.AgentOnDone();

        //ver como recriar agente que não reseta automaticamente. https://github.com/Unity-Technologies/ml-agents/blob/master/docs/Learning-Environment-Design-Agents.md
        //Instantiating an Agent at Runtime

        Destroy(gameObject);
    }*/

    public override void AgentReset()
    {
        //Debug.Log("AgentReset foi chamado");
        this.transform.position = initialPosition;
        this.rigidBody.angularVelocity = Vector3.zero;
        this.rigidBody.velocity = Vector3.zero;
        
        canMove = true;
        targetReached = false;
        hasPlacedBomb = false;
        canDropBombs = true;
        isInDanger = false;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;
        playerModel.SetActive(true);

        is_received_subgoal_distance1 = false;
        is_received_subgoal_distance2 = false;

        is_received_subgoal_step1 = false;
        is_received_subgoal_step2 = false;
        is_received_subgoal_step3 = false;
        is_received_subgoal_step4 = false;
        bombCount = 0;

        if (saveReplay)
        {
            if (replayWriter == null)
                replayWriter = new ReplayWriter(playerNumber, scenarioId);

            replayWriter.initSeq(localEpisode);
        }
        

        localStep = 1;
        
        ServiceLocator.getManager(scenarioId).GetLogManager().localEpisodePrint(localEpisode++, this);

        if (playerNumber == 1)
        {
            //Debug.Log("Agente 1 resetou a fase");
            myBombManager.clearBombs();
            ServiceLocator.getManager(scenarioId).GetBlocksManager().resetBlocks();
            //grid.refreshNodesInGrid();
            isReady = true;
        }
        else
        {
            //Debug.Log("Agente 2 nao resetou a fase");
            isReady = true;
        }

        dead = false;
        myIterationActionWasExecuted = false;
        bombermanVillain = null;
        wasFilledTeacherObservations = false;

        grid.updateAgentOnGrid(this);
        oldLocalPosition = transform.localPosition;
    }

    private void AddVectorObsForGrid()
    {
        if (grid.gridType == GridType.GT_Hybrid)
        {
            if (grid.gridSentData == GridSentData.GSD_All)
            {
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        AddVectorObs(grid.NodeFromPos(x, y).getBinaryArray());
                    }
                }
            }
            else if (grid.gridSentData == GridSentData.GSD_Divided)
            {

            }
        }
        else if (grid.gridType == GridType.GT_Binary)
        {
            if (grid.gridSentData == GridSentData.GSD_All)
            {
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        BaseNode node = grid.NodeFromPos(x, y);
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
            else if (grid.gridSentData == GridSentData.GSD_Divided)
            {
                //enviar grid que representa posições livres, com blocos ou com paredes
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        AddVectorObs(grid.NodeFromPos(x, y).getFreeBreakableObstructedCell());
                    }
                }

                //enviar grid que representa posição do agente
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        AddVectorObs(grid.NodeFromPos(x, y).getPositionAgent(playerNumber));
                    }
                }

                //enviar grid que representa áreas de perigo
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        bool hasDanger = grid.NodeFromPos(x, y).getDangerPosition();
                        if (!hasDanger)
                            AddVectorObs(0.0f);
                        else
                        {
                            Danger danger = myBombManager.getDanger(x, y);
                            if (danger != null)
                            {
                                float dangerLevel = danger.GetDangerLevelOfPosition(this);
                                AddVectorObs(dangerLevel);
                            }
                            else
                                AddVectorObs(0.0f);
                        }
                    }   
                }

                //enviar grid que representa posição do target
                for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
                {
                    for (int x = 0; x < grid.GetGridSizeX(); ++x)
                    {
                        AddVectorObs(grid.NodeFromPos(x, y).getPositionTarget());
                    }
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
        clearReplayVars();
        myGridPosition = GetGridPosition();
        //AddVectorObs(myGridPosition); //add +2

        //adicionando posição do objetivo
        //AddVectorObs(targetGridPosition); //add +2

        //velocidade do agente
        /*float velX = rigidBody.velocity.x / moveSpeed;
        float velZ = rigidBody.velocity.z / moveSpeed;
        AddVectorObs(velX);
        AddVectorObs(velZ);*/

        //AddVectorObs(canDropBombs ? 1 : 0);
        //AddVectorObs(isInDanger ? 1 : 0);
        //AddVectorObs(ServiceLocator.GetBombManager().existsBombOrDanger() ? 1 : 0);

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
                teacherAgent.wasFilledTeacherObservations = false;
                AddVectorObsForGrid();
            }
        }

        

        //Usado na tentativa de fazer um fluxo condicional com o PPOCustom
        //SetTextObs(playerNumber.ToString());

        //primeiro ou segundo agente. Hack para recuperar arquivo de replay no BombermanDecision
        //AddVectorObs(playerNumber == 1 ? true : false);

        ServiceLocator.getManager(scenarioId).GetLogManager().statePrint("Agent " + playerNumber,
                                                    myGridPosition,
                                                    targetGridPosition,
                                                    //new Vector2(velX, velZ),
                                                    grid.gridToString(playerNumber),
                                                    canDropBombs,
                                                    isInDanger,
                                                    myBombManager.existsBombOrDanger());
        
    }

    private void penalizeInvalidWalkMovement()
    {
        if (forceReward || !isMimicking)
        {
            AddReward(Config.REWARD_INVALID_WALK_ACTION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " tentou andar sem poder", Config.REWARD_INVALID_WALK_ACTION);
        }
    }

    private void reinforceValidWalkMovement()
    {
        if (forceReward || !isMimicking)
        {
            AddReward(Config.REWARD_VALID_WALK_POSITION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " andou para um lugar livre", Config.REWARD_VALID_WALK_POSITION);
        }
    }

    private void penalizeInvalidHammerAction()
    {
        if (forceReward || !isMimicking)
        {
            AddReward(Config.REWARD_INVALID_HAMMER_ACTION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " tentou usar o marteto desnecessariamente", Config.REWARD_INVALID_HAMMER_ACTION);
        }
    }

    private void reinforceValidHammerAction()
    {
        if (forceReward || !isMimicking)
        {
            AddReward(Config.REWARD_VALID_HAMMER_ACTION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " usou o martelo corretamente", Config.REWARD_VALID_HAMMER_ACTION);
        }
    }

    private void penalizeStopAction()
    {
        if (forceReward || !isMimicking)
        {
            AddReward(Config.REWARD_STOP_ACTION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " ficou parado, e ficar parado eh perder tempo", Config.REWARD_STOP_ACTION);
        }
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

    private void verifySubGoals(Vector2 gridPos)
    {
        int distance = Math.Abs((int)(gridPos.x - targetGridPosition.x)) + Math.Abs((int)(gridPos.y - targetGridPosition.y)); 

        
        //Quanto a distância
        if (!is_received_subgoal_distance2 && bombCount >= 3 && distance <= 4f)
        {
            is_received_subgoal_distance2 = true;
            AddReward(Config.REWARD_SUB_GOAL_DISTANCE_2);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal distance 2", Config.REWARD_SUB_GOAL_DISTANCE_2);
        }
        else if (!is_received_subgoal_distance1 && bombCount >= 1 && distance <= 7f)
        {
            is_received_subgoal_distance1 = true;
            AddReward(Config.REWARD_SUB_GOAL_DISTANCE_1);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal distance 1", Config.REWARD_SUB_GOAL_DISTANCE_1);
        }



        //Quanto ao número de iterações que está vivo.
        /*if (bombCount >= 3)
        {
            if (!is_received_subgoal_step4 && getLocalStep() >= 50)
            {
                is_received_subgoal_step4 = true;
                AddReward(Config.REWARD_SUB_GOAL_NUM_STEP_4);
                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal step 4", Config.REWARD_SUB_GOAL_NUM_STEP_4);
            }
            else if (!is_received_subgoal_step3 && getLocalStep() >= 40)
            {
                is_received_subgoal_step3 = true;
                AddReward(Config.REWARD_SUB_GOAL_NUM_STEP_3);
                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal step 3", Config.REWARD_SUB_GOAL_NUM_STEP_3);
            }
            else if (!is_received_subgoal_step2 && getLocalStep() >= 20)
            {
                is_received_subgoal_step2 = true;
                AddReward(Config.REWARD_SUB_GOAL_NUM_STEP_2);
                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal step 2", Config.REWARD_SUB_GOAL_NUM_STEP_2);
            }
            else if (!is_received_subgoal_step1 && getLocalStep() >= 10)
            {
                is_received_subgoal_step1 = true;
                AddReward(Config.REWARD_SUB_GOAL_NUM_STEP_1);
                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou sub goal step 1", Config.REWARD_SUB_GOAL_NUM_STEP_1);
            }
        }*/
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (!dead)
        {

            if (!isMimicking)
            {
                if (getLocalStep() >= Config.MAX_STEP_PER_AGENT)
                {
                    AddReward(Config.REWARD_MAX_STEP_REACHED);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou max step", Config.REWARD_MAX_STEP_REACHED);

                    killAgent();
                }
            }

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
                if (!targetReached)
                {
                    //Testar objetivo final e target aqui porque foi observado que ao chegar ao destino final, o estado não é atualizado.
                    if (grid.checkTarget(myGridPosition))
                    {
                        if (forceReward || !isMimicking)
                        {
                            AddReward(Config.REWARD_GOAL);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou o objetivo", Config.REWARD_GOAL);
                        }

                        targetReached = true;
                        myPlayerManager.addTargetCount();

                        if (isSpecialist)
                        {
                            countSeq++;
                        }

                        if (myPlayerManager.getNumPlayers() <= 1)
                        {
                            targetReached = false;
                            myPlayerManager.clearTargetCount();

                            if (isSpecialist && countSeq == numSeqCompleted)
                            {
                                bcTeacherHelper.forceStopRecord();
                                bombermanDecision.finishSeqs1 = true;
                            }

                            Done();
                            //doneAnother();
                        }
                    }
                    else if (grid.checkFire(myGridPosition)) //Bomb code
                    {
                        if (forceReward || !isMimicking)
                        {
                            AddReward(Config.REWARD_DIE);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " atingido por explosao", Config.REWARD_DIE);

                            if (bombermanVillain != null)
                            {
                                if (bombermanVillain.playerNumber != playerNumber)
                                {
                                    //penalizando amigo por fogo amigo
                                    bombermanVillain.AddReward(Config.REWARD_KILL_FRIEND);
                                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + bombermanVillain.playerNumber + " matou amigo", Config.REWARD_KILL_FRIEND);
                                }
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

                    hasPlacedBomb = false;
                    animator.SetBool("Walking", false);

                    //-----------------------------------------------------------------------------------------------------
                    if (!dead && !IsDone())
                    {
                        Vector2 newPos, hammerPos;
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
                                if (canDropBombs && !dead)
                                {
                                    hasPlacedBomb = true;
                                    DropBomb();
                                }
                                else
                                {
                                    if (forceReward || !isMimicking)
                                    {
                                        AddReward(Config.REWARD_INVALID_BOMB_ACTION);
                                        ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " tentou colocar bomba sem poder", Config.REWARD_INVALID_BOMB_ACTION);
                                    }
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

                    float distanceToTarget = Vector2.Distance(myGridPosition, targetGridPosition);

                    //se aproximando ainda mais. Melhor aproximação
                    if (distanceToTarget < closestDistance)
                    {
                        closestDistance = distanceToTarget;
                        if (forceReward || !isMimicking)
                        {
                            AddReward(Config.REWARD_CLOSEST_DISTANCE);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " melhor aproximacao do objetivo", Config.REWARD_CLOSEST_DISTANCE);
                        }
                    }

                    if (distanceToTarget < previousDistance)
                    {
                        if (forceReward || !isMimicking)
                        {
                            AddReward(Config.REWARD_APPROACHED_DISTANCE);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " se aproximou", Config.REWARD_APPROACHED_DISTANCE);
                        }
                    }
                    else if (distanceToTarget > previousDistance)
                    {
                        if (forceReward || !isMimicking)
                        {
                            AddReward(Config.REWARD_FAR_DISTANCE);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " se distanciou", Config.REWARD_FAR_DISTANCE);
                        }
                    }

                    //verifySubGoals(myGridPosition);
                    if (ServiceLocator.getManager(scenarioId).GetBombManager().existsBombOrDanger())
                    {
                        if (!isInDanger)
                        {
                            AddReward(Config.REWARD_SAFE_AREA);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " esta seguro", Config.REWARD_SAFE_AREA);
                        }
                        else
                        {
                            AddReward(Config.REWARD_DANGER_AREA);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " continua em area de perigo", Config.REWARD_DANGER_AREA);
                        }
                    }



                    previousDistance = distanceToTarget;
                }
                else
                {
                    if (playerNumber == 1)
                    {
                        if (myPlayerManager.getTargetCount() >= 2)
                        {
                            //Debug.Log("Entrou");
                            if (forceReward || !isMimicking)
                            {
                                AddReward(Config.REWARD_TEAM_GOAL);
                                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + ": time alcancou o objetivo", Config.REWARD_TEAM_GOAL);

                                myPlayerManager.getAgent2().AddReward(Config.REWARD_TEAM_GOAL);
                                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + myPlayerManager.getAgent2().playerNumber + ": time alcancou o objetivo", Config.REWARD_TEAM_GOAL);
                            }
                            targetReached = false;

                            Done();
                            doneAnotherWithoutDeath();

                            if (isSpecialist && countSeq == numSeqCompleted)
                            {
                                myPlayerManager.getAgent2().bcTeacherHelper.forceStopRecord();
                                bcTeacherHelper.forceStopRecord();
                                bombermanDecision.finishSeqs1 = true;
                                //playerManager.getAgent2().bombermanDecision.finishSeqs2 = true;
                            }

                        }
                    }
                }

                if (forceReward || !isMimicking)
                {
                    //penalidade de tempo
                    AddReward(Config.REWARD_TIME_PENALTY);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " sofreu penalidade de tempo", Config.REWARD_TIME_PENALTY);
                }


                
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
            float temp = Mathf.RoundToInt(transform.position.x) - transform.position.x >= 0.0f ? -0.5f : 0.5f;

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

            AddReward(Config.REWARD_VALID_BOMB_ACTION);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " colocou uma bomba", Config.REWARD_VALID_BOMB_ACTION);

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
        /*if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                bombermanVillain = other.gameObject.GetComponent<DestroySelf>().bomberman;
            }
        }

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
        transform.position = initialPosition;

        // tentando corrigir problema da bomba explodir após agente ser reiniciado por tempo
        //Bomb code
        if (!academy.GetIsInference())
            myBombManager.clearBombs();
    }

    private void killAgentOnly()
    {
        defaultKillCode();
        //Debug.Log("Morri force - agente " + playerNumber);
    }

    private void killAgentBoth()
    {
        defaultKillCode();
        //Debug.Log("Morri both - agente " + playerNumber);

        Invoke("DoneWithDelay", 2.5f);
    }

    private void killAgent()
    {
        defaultKillCode();
        //Debug.Log("Morri - agente " + playerNumber);

        myPlayerManager.clearDeadCount();
        Done();

        /* Comentado porque começou a dar pau com apenas um agente após várias iterações
         * if (myPlayerManager.getDeadCount() == 0)
        {
            myPlayerManager.addDeadCount();
            Invoke("VerifyDeadCount", 0.5f);
        }*/

    }

    void VerifyDeadCount()
    {
        /*if (playerManager.getDeadCount() >= 2)
        {
            Debug.Log("DeadCount2");
            //playerManager.clearDeadCount();
            Invoke("DoneWithDelay", 2.5f);
        }
        else */if (myPlayerManager.getDeadCount() >= 1)
        {
            //playerManager.clearDeadCount();
            if (myPlayerManager.getAgent1() != null && myPlayerManager.getAgent1().dead)
            {
                //Debug.Log("DeadCount1 ag1");
                Invoke("DoneWithDelay", 2.5f);
            }
            else if (myPlayerManager.getAgent2() != null && myPlayerManager.getAgent2().dead)
            {
                //Debug.Log("DeadCount1 ag2");
                Invoke("DoneWithDelay", 0.0f);
            }
        }
    }

    private void doneAnother()
    {
        isReady = false;
        if (playerNumber == 1)
        {
            Player another = myPlayerManager.getAgent2();
            if (another != null)
            {
                //Debug.Log("Matei o agente2");
                another.killAgentOnly();
                another.Done();
                another.isReady = false;
                myPlayerManager.clearDeadCount();
            }

            if (myPlayerManager.getNumPlayers() == 1)
            {
                myPlayerManager.clearDeadCount();
            }
        }
        else if (playerNumber == 2)
        {
            dead = true;
            Player another = myPlayerManager.getAgent1();
            if (another != null)
            {
                //Debug.Log("Matei o agente1");
                another.killAgentBoth();
                another.isReady = false;
            }
        }
    }

    private void doneAnotherWithoutDeath()
    {
        isReady = false;
        if (playerNumber == 1)
        {
            Player another = ServiceLocator.getManager(scenarioId).GetPlayerManager().getAgent2();
            if (another != null)
            {
                another.Done();
                another.isReady = false;
                ServiceLocator.getManager(scenarioId).GetPlayerManager().clearDeadCount();
                ServiceLocator.getManager(scenarioId).GetPlayerManager().clearTargetCount();
                //another.targetReached = false;
            }

            if (ServiceLocator.getManager(scenarioId).GetPlayerManager().getNumPlayers() == 1)
            {
                ServiceLocator.getManager(scenarioId).GetPlayerManager().clearDeadCount();
            }
        }
    }

    private void DoneWithDelay()
    {
        if (playerNumber == 1)
            Done();

        doneAnother();
    }

    public void FixedUpdate()
    {
        //WaitTimeInference();
        WaitIterationActions();
    }

    private void internalUpdate()
    {
        if (myPlayerManager.isReadyForNewIteration())
        {
            myPlayerManager.addIterationCount();
            myBombManager.timeIterationUpdate();
        }

        if (!myIterationActionWasExecuted)
        {
            if (isMimicking)
            {
                if (teacherAgent != null && teacherAgent.wasFilledTeacherObservations)
                {
                    RequestDecision();
                    myIterationActionWasExecuted = true;
                }
            }
            else
            {
                if (isSpecialist)
                {
                    if (!wasFilledTeacherObservations)
                    {
                        RequestDecision();
                        myIterationActionWasExecuted = true;
                    }
                }
                else
                {
                    RequestDecision();
                    myIterationActionWasExecuted = true;
                }
            }
        }
    }

    private void WaitIterationActions()
    {
        if (wasInitialized)
        {
            if (!dead && myPlayerManager.getDeadCount() == 0)
            {
                if (!academy.GetIsInference())
                {
                    internalUpdate();
                }
                else
                {
                    if (timeSinceDecision >= timeBetweenDecisionsAtInference)
                    {
                        timeSinceDecision = 0f;
                        internalUpdate();
                    }
                    else
                    {
                        timeSinceDecision += Time.fixedDeltaTime;
                    }
                }
                
            }
        }
    }

    private void WaitTimeInference()
    {
        if (wasInitialized)
        {
            if (!dead && myPlayerManager.getDeadCount() == 0)
            {
                if (!academy.GetIsInference())
                {
                    RequestDecision();
                }
                else
                {
                    if (timeSinceDecision >= timeBetweenDecisionsAtInference)
                    {
                        timeSinceDecision = 0f;      
                        RequestDecision();
                    }
                    else
                    {
                        timeSinceDecision += Time.fixedDeltaTime;
                    }
                }
            }
        }
    }

    void OnApplicationQuit()
    {
        if (saveReplay)
            replayWriter.finish();
    }
}
