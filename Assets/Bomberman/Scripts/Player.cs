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
    
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    public bool isReady = true;
    [HideInInspector]
    public bool wasInitialized = false;

    private GameObject playerModel;

    //variaveis usadas para salvar arquivo de replay
    public bool saveReplay = true;
    private ReplayWriter replayWriter;
    private string observationGridString;
    private string actionIdString;

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
        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;
        if (playerNumber == 1)
        {
            stateType = StateType.ST_Agent1;
            ServiceLocator.getManager(scenarioId).GetPlayerManager().setAgent1(this);
        }
        else if (playerNumber == 2)
        {
            stateType = StateType.ST_Agent2;
            ServiceLocator.getManager(scenarioId).GetPlayerManager().setAgent2(this);
        }

        if (saveReplay)
            replayWriter = new ReplayWriter(playerNumber, scenarioId);
    }

    public override void InitializeAgent()
    {
        Debug.Log("InitializeAgent foi chamado");
        base.InitializeAgent();

        isInDanger = false;
        canDropBombs = true;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;
        initialPosition = transform.position;
        oldLocalPosition = transform.localPosition;

        isReady = true;
        targetReached = false;

        playerModel = transform.Find("PlayerModel").gameObject;
        rigidBody = GetComponent<Rigidbody>();
        animator = transform.Find("PlayerModel").GetComponent<Animator>();

        Vector3 gridTarget3d = Target.transform.localPosition - Vector3.one;
        targetGridPosition = new Vector2(Mathf.RoundToInt(gridTarget3d.x), Mathf.RoundToInt(gridTarget3d.z));

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
        Debug.Log("AgentReset foi chamado");
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

        localStep = 1;
        ServiceLocator.getManager(scenarioId).GetLogManager().localEpisodePrint(localEpisode++, this);

        if (playerNumber == 1)
        {
            //Debug.Log("Agente 1 resetou a fase");
            ServiceLocator.getManager(scenarioId).GetBombManager().clearBombs();
            ServiceLocator.getManager(scenarioId).GetBlocksManager().resetBlocks();
            grid.refreshNodesInGrid();
            isReady = true;
        }
        else
        {
            //Debug.Log("Agente 2 nao resetou a fase");
            isReady = true;
        }

        dead = false;
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
                        int cell = grid.NodeFromPos(x, y).getBinary();
                        AddVectorObs(cell);

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
                            Danger danger = ServiceLocator.getManager(scenarioId).GetBombManager().getDanger(x, y);
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
        AddVectorObsForGrid();

        ServiceLocator.getManager(scenarioId).GetLogManager().statePrint("Agent " + playerNumber,
                                                    myGridPosition,
                                                    targetGridPosition,
                                                    //new Vector2(velX, velZ),
                                                    grid.gridToString(),
                                                    canDropBombs,
                                                    isInDanger,
                                                    ServiceLocator.getManager(scenarioId).GetBombManager().existsBombOrDanger());
        
    }

    private void penalizeInvalidMovement()
    {
        AddReward(Config.REWARD_INVALID_ACTION);
        ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " tentou andar sem poder", Config.REWARD_INVALID_ACTION);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Monitor.Log("CumulativeReward", GetCumulativeReward(), MonitorType.text, transform);
        
        if (GetStepCount() >= Config.MAX_STEP_PER_AGENT)
        {
            AddReward(Config.REWARD_MAX_STEP_REACHED);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou max step", Config.REWARD_MAX_STEP_REACHED);

            killAgent();
        }

        if (!dead)
        {
            ServiceLocator.getManager(scenarioId).GetLogManager().localStepPrint(this);
            localStep++;

            ActionType action = ActionTypeExtension.convert((int)vectorAction[0]);

            if (saveReplay)
            {
                actionIdString = ((int)vectorAction[0]).ToString();
                replayWriter.printStep(observationGridString, actionIdString);
            }
            
            if (!targetReached)
            {
                //Testar objetivo final e target aqui porque foi observado que ao chegar ao destino final, o estado não é atualizado.
                if (grid.checkTarget(myGridPosition))
                {
                    AddReward(Config.REWARD_GOAL);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou o objetivo", Config.REWARD_GOAL);
                    targetReached = true;
                    ServiceLocator.getManager(scenarioId).GetPlayerManager().addTargetCount();

                    if (ServiceLocator.getManager(scenarioId).GetPlayerManager().getNumPlayers() <= 1)
                    {
                        targetReached = false;
                        Done();
                        //doneAnother();
                    }
                }

                hasPlacedBomb = false;
                animator.SetBool("Walking", false);

                //-----------------------------------------------------------------------------------------------------

                Vector2 newPos;
                switch (action)
                {
                    //cima
                    case ActionType.AT_Up:
                        //rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
                        newPos = myGridPosition + new Vector2(0, 1);
                        if (grid.checkFreePosition(newPos))
                        {
                            transform.position = transform.position + new Vector3(0, 0, 1);
                        }
                        else
                            penalizeInvalidMovement();

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
                        }
                        else
                            penalizeInvalidMovement();

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
                        }
                        else
                            penalizeInvalidMovement();

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
                        }
                        else
                            penalizeInvalidMovement();

                        transform.rotation = Quaternion.Euler(0, 270, 0);
                        animator.SetBool("Walking", true);
                        break;
                    //Drop bomb
                    case ActionType.AT_Bomb:
                        if (canDropBombs)
                        {
                            hasPlacedBomb = true;
                            DropBomb();
                        }
                        else
                        {
                            AddReward(Config.REWARD_INVALID_BOMB_ACTION);
                            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " tentou colocar bomba sem poder", Config.REWARD_INVALID_BOMB_ACTION);
                        }
                        break;
                    //Wait
                    case ActionType.AT_Wait:
                    default:
                        break;
                }

                //recompensas
                myGridPosition = GetGridPosition();

                float distanceToTarget = Vector2.Distance(myGridPosition, targetGridPosition);

                //se aproximando ainda mais. Melhor aproximação
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    AddReward(Config.REWARD_CLOSEST_DISTANCE);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " melhor aproximacao do objetivo", Config.REWARD_CLOSEST_DISTANCE);
                }

                if (distanceToTarget < previousDistance)
                {
                    AddReward(Config.REWARD_APPROACHED_DISTANCE);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " se aproximou", Config.REWARD_APPROACHED_DISTANCE);
                }
                else if (distanceToTarget > previousDistance)
                {
                    AddReward(Config.REWARD_FAR_DISTANCE);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " se distanciou", Config.REWARD_FAR_DISTANCE);
                }

                previousDistance = distanceToTarget;
            }
            else
            {
                if (playerNumber == 1)
                {
                    PlayerManager playerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
                    if (playerManager.getTargetCount() >= 2)
                    {
                        playerManager.clearTargetCount();

                        AddReward(Config.REWARD_TEAM_GOAL);
                        ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + ": time alcancou o objetivo", Config.REWARD_TEAM_GOAL);
                        targetReached = false;

                        playerManager.getAgent2().AddReward(Config.REWARD_TEAM_GOAL);
                        ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerManager.getAgent2().playerNumber + ": time alcancou o objetivo", Config.REWARD_TEAM_GOAL);
                        playerManager.getAgent2().targetReached = false;

                        Done();
                        doneAnother();
                    }
                }
            }

            //penalidade de tempo
            AddReward(Config.REWARD_TIME_PENALTY);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " sofreu penalidade de tempo", Config.REWARD_TIME_PENALTY);

            /*if (ServiceLocator.GetBombManager().existsBombOrDanger())
            {
                if (!isInDanger)
                {
                    AddReward(0.05f);
                    ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " esta seguro", 0.05f);
                }
                else
                {
                    AddReward(-0.05f);
                    ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " continua em area de perigo", -0.05f);
                }
            }*/

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

            ServiceLocator.getManager(scenarioId).GetBombManager().addBomb(bomb);
            grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
            bomb.GetComponent<Bomb>().CreateDangerZone();

            //AddReward(0.006f);
            //ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " colocou uma bomba", 0.006f);

            canDropBombs = false;
            isInDanger = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //talvez na movimentação contínua não funcione esse código. Uma boa solução seria usar um contador(int) de perigo ao invés de um bool
        if (other.CompareTag("Danger"))
        {
            isInDanger = false;
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                AddReward(Config.REWARD_DIE);
                ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " atingido por explosao", Config.REWARD_DIE);

                Bomb bomb = other.gameObject.GetComponent<DestroySelf>().myBomb;
                if (bomb != null)
                {
                    if (bomb.bomberman.playerNumber != playerNumber)
                    {
                        //penalizando amigo por fogo amigo
                        bomb.bomberman.AddReward(Config.REWARD_KILL_FRIEND);
                        ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + bomb.bomberman.playerNumber + " matou amigo", Config.REWARD_KILL_FRIEND);
                    }
                }

                killAgent();
            }
        }
        /*else if (other.CompareTag("Target"))
        {
            AddReward(Config.REWARD_GOAL);
            ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou o objetivo", Config.REWARD_GOAL);
            Done();
            doneAnother();
        }*/
        /* Agente toma muita recompensa negativa e desiste de colocar bomba muito rápido*/
        else if (other.CompareTag("Danger"))
        {
            isInDanger = true;
            /*float dangerLevel = Mathf.Abs(other.gameObject.GetComponent<Danger>().GetDangerLevelOfPosition(this));
            dangerLevel *= -0.1f;
            AddReward(dangerLevel);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " em area de perigo", dangerLevel);*/
        }
    }

    private void defaultKillCode()
    {
        dead = true;
        grid.clearAgentOnGrid(this);
        playerModel.SetActive(false);
        transform.position = initialPosition;
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

        if (ServiceLocator.getManager(scenarioId).GetPlayerManager().getDeadCount() == 0)
        {
            ServiceLocator.getManager(scenarioId).GetPlayerManager().addDeadCount();
            Invoke("VerifyDeadCount", 0.5f);
        }

    }

    void VerifyDeadCount()
    {
        PlayerManager playerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        /*if (playerManager.getDeadCount() >= 2)
        {
            Debug.Log("DeadCount2");
            //playerManager.clearDeadCount();
            Invoke("DoneWithDelay", 2.5f);
        }
        else */if (playerManager.getDeadCount() >= 1)
        {
            //playerManager.clearDeadCount();
            if (playerManager.getAgent1().dead)
            {
                //Debug.Log("DeadCount1 ag1");
                Invoke("DoneWithDelay", 2.5f);
            }
            else if (playerManager.getAgent2().dead)
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
            Player another = ServiceLocator.getManager(scenarioId).GetPlayerManager().getAgent2();
            if (another != null)
            {
                //Debug.Log("Matei o agente2");
                another.killAgentOnly();
                another.Done();
                another.isReady = false;
                ServiceLocator.getManager(scenarioId).GetPlayerManager().clearDeadCount();
            }

            if (ServiceLocator.getManager(scenarioId).GetPlayerManager().getNumPlayers() == 1)
            {
                ServiceLocator.getManager(scenarioId).GetPlayerManager().clearDeadCount();
            }
        }
        else if (playerNumber == 2)
        {
            dead = true;
            Player another = ServiceLocator.getManager(scenarioId).GetPlayerManager().getAgent1();
            if (another != null)
            {
                //Debug.Log("Matei o agente1");
                another.killAgentBoth();
                another.isReady = false;
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
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {
        if (!dead && ServiceLocator.getManager(scenarioId).GetPlayerManager().getDeadCount() == 0)
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

    void OnApplicationQuit()
    {
        if (saveReplay)
            replayWriter.finish();
    }
}
