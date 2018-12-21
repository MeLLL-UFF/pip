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
using System;
using System.Collections.Generic;
using MLAgents;

public class Player : Agent
{
    int scenarioId;

   /* [Header("Specific to Player")]
    private BombermanAcademy academy;*/
    private BombermanDecision bombermanDecision;
    private BCTeacherHelper bcTeacherHelper;


    public Transform monitorFocus;

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

    private Vector3 initialPosition;
    private Vector3 oldLocalPosition;
    private Vector2 myGridPosition;

    public bool randomizeResetPosition = false;
    public bool randomizeInitialPosition = false;
    public GridViewType myGridViewType;

    private int localEpisode = 1;
    private int localStep = 1;
    private int totalLocalStep = 1;

    private GameObject playerModel;

    //variaveis usadas para salvar arquivo de replay
    public string actionIdString;

    public PlayerManager myPlayerManager;
    public BombManager myBombManager;
    Player bombermanVillain;

    int bombCount = 0;

    class ObservationLog
    {
        public Vector2 gridPosition;
        public string gridString;
        public bool canDropBombs;
        public bool isInDanger;
        public bool existsBombOrDanger;

        public void update(Vector2 _gridPosition, string _gridString, bool _canDropBombs, bool _isInDanger, bool _existsBombOrDanger)
        {
            gridPosition= _gridPosition;
            gridString= _gridString;
            canDropBombs= _canDropBombs;
            isInDanger= _isInDanger;
            existsBombOrDanger=_existsBombOrDanger;
        }
    }
    ObservationLog lastObservationLog = new ObservationLog();

    public int getScenarioId()
    {
        return scenarioId;
    }

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

    public StateType getStateType()
    {
        return stateType;
    }

    public Vector2 GetGridPosition()
    {
        BinaryNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public Vector2 GetOldGridPosition()
    {
        BinaryNode n = grid.NodeFromWorldPoint(oldLocalPosition);
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
    public void init(Grid g, int pNumber)
    {
        //Debug.Log("Init foi chamado");
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

        localStep = 1;

        ServiceLocator.getManager(scenarioId).GetLogManager().localEpisodePrint(localEpisode++, this);

        //------------------------------------------------------ São atualizadas no Start, que é chamado depois
        initialPosition = transform.localPosition;
        oldLocalPosition = transform.localPosition;
        // -----------------------------------------------------------------------------------------------------

        playerModel = transform.Find("PlayerModel").gameObject;
        animator = transform.Find("PlayerModel").GetComponent<Animator>();

        bombermanVillain = null;

        actionIdString = "empty";

        wasInitialized = true;

        myPlayerManager = ServiceLocator.getManager(scenarioId).GetPlayerManager();
        myBombManager = ServiceLocator.getManager(scenarioId).GetBombManager();
    }

    public void  eventsAfterGiveBrain(MapController mapController)
    {
        bcTeacherHelper = GetComponent<BCTeacherHelper>();
        if (brain.GetComponent<BrainCustomData>().isTeacher)
        {
            bombermanDecision = brain.GetComponent<BombermanDecision>();
            bombermanDecision.setMapController(getScenarioId(), mapController);
        }
        else
        {
            bcTeacherHelper.enabled = false;
        }
    }

    public void setBCTeacherHelperTransform(Transform _transform)
    {
        if (brain.GetComponent<BrainCustomData>().isTeacher)
        {
            bcTeacherHelper.setMyMonitorTransform(_transform);
        }
    }

    public void stopToSendExperience()
    {
        if (brain.GetComponent<BrainCustomData>().isTeacher)
        {
            bcTeacherHelper.forceStopRecord();
        }
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
                    BinaryNode node = grid.NodeFromPos(x, y);
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
                    BinaryNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);
                    int cell = (int)nodeStateType;

                    AddVectorObs(cell);
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
                    BinaryNode node = grid.NodeFromPos(x, y);
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

            freeBreakableObstructedCells.Clear();
            positionAgentCells.Clear();
            positionEnemyCells.Clear();
            dangerLevelOfPositionsCells.Clear();
        }
        else if (myGridViewType == GridViewType.GVT_BinaryDecimal)
        {
            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BinaryNode node = grid.NodeFromPos(x, y);
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
                    BinaryNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);

                    float cell = StateTypeExtension.normalizeBinaryFlag(nodeStateType);
                    AddVectorObs(cell);
                }
            }
        }
        else if (myGridViewType == GridViewType.GVT_ZeroOrOneForeachStateType)
        {
            List<float> freeCells = new List<float>();
            List<float> destructibleCells = new List<float>();
            List<float> positionAgentCells = new List<float>();
            List<float> positionEnemyCells = new List<float>();
            List<float> bombCells = new List<float>();
            List<float> dangerCells = new List<float>();
            List<float> fireCells = new List<float>();

            for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
            {
                for (int x = 0; x < grid.GetGridSizeX(); ++x)
                {
                    BinaryNode node = grid.NodeFromPos(x, y);
                    StateType nodeStateType = grid.adjustAgentStateTypeForBinaryNode(node, playerNumber);

                    freeCells.Add(node.getFreeCell());

                    int hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Block) ? 1 : 0;
                    destructibleCells.Add(hasThis);

                    hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Agent) ? 1 : 0;
                    positionAgentCells.Add(hasThis);

                    hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_EnemyAgent) ? 1 : 0;
                    positionEnemyCells.Add(hasThis);

                    hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Bomb) ? 1 : 0;
                    bombCells.Add(hasThis);

                    hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Danger) ? 1 : 0;
                    dangerCells.Add(hasThis);

                    hasThis = StateTypeExtension.stateTypeHasFlag(nodeStateType, StateType.ST_Fire) ? 1 : 0;
                    fireCells.Add(hasThis);
                }
            }

            AddVectorObs(freeCells);
            AddVectorObs(destructibleCells);
            AddVectorObs(positionAgentCells);
            AddVectorObs(positionEnemyCells);
            AddVectorObs(bombCells);
            AddVectorObs(dangerCells);
            AddVectorObs(fireCells);

            freeCells.Clear();
            destructibleCells.Clear();
            positionAgentCells.Clear();
            positionEnemyCells.Clear();
            bombCells.Clear();
            dangerCells.Clear();
            fireCells.Clear();
        }
    }

    public override void CollectObservations()
    {
        //Debug.Log("agent" + playerNumber + " observacoes");
        actionIdString = "";
        myGridPosition = GetGridPosition();

        Vector2 normalizedGridPosition = (myGridPosition) / (grid.GetGridMaxValue());
        AddVectorObs(normalizedGridPosition);

        AddVectorObsForGrid();

        lastObservationLog.update(myGridPosition, grid.gridToString(playerNumber), canDropBombs, isInDanger, myBombManager.existsBombOrDanger());
    }

    public static void AddRewardToAgent(Player player, float reward, string message)
    {
        player.AddReward(reward);
        ServiceLocator.getManager(player.scenarioId).GetLogManager().rewardPrint(message, reward);
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

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log("agent" + playerNumber + " acoes");
        if (!dead)
        {
            ServiceLocator.getManager(scenarioId).GetLogManager().statePrint("Agent " + playerNumber,
                                                    lastObservationLog.gridPosition,
                                                    lastObservationLog.gridString,
                                                    lastObservationLog.canDropBombs,
                                                    lastObservationLog.isInDanger,
                                                    lastObservationLog.existsBombOrDanger);


            ServiceLocator.getManager(scenarioId).GetLogManager().localStepPrint(this);
            localStep++;
            totalLocalStep++;

            ActionType action = ActionTypeExtension.convert((int)vectorAction[0]);
            actionIdString = ((int)vectorAction[0]).ToString();

            if (monitorFocus != null)
            {
                Monitor.Log("Episode: ", (localEpisode-1).ToString(), monitorFocus);
                Monitor.Log("RT Step: ", (totalLocalStep - 1).ToString(), monitorFocus);
                Monitor.Log("RL Step: ", (getLocalStep()-1).ToString(), monitorFocus);
                //Monitor.Log(" T Step: ", GetTotalStepCount().ToString(), monitorFocus);
                Monitor.Log(" L Step: ", GetStepCount().ToString(), monitorFocus);
                Monitor.Log("ActionP" + playerNumber + ": ", Convert.ToString((int)action), monitorFocus);
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

                    if (animator.gameObject.activeSelf)
                    {
                        animator.SetBool("Walking", false);
                    }

                    //-----------------------------------------------------------------------------------------------------
                    if (!dead && !IsDone())
                    {
                        Vector2 newPos;
                        switch (action)
                        {
                            //cima
                            case ActionType.AT_Up:
                                newPos = myGridPosition + new Vector2(0, 1);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(0, 0, 1);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 0, 0);
                                if (animator.gameObject.activeSelf)
                                {
                                    animator.SetBool("Walking", true);
                                }
                                break;
                            //baixo
                            case ActionType.AT_Down:
                                newPos = myGridPosition + new Vector2(0, -1);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(0, 0, -1);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 180, 0);
                                if (animator.gameObject.activeSelf)
                                {
                                    animator.SetBool("Walking", true);
                                }
                                break;
                            //direita
                            case ActionType.AT_Right:
                                newPos = myGridPosition + new Vector2(1, 0);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(1, 0, 0);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 90, 0);
                                if (animator.gameObject.activeSelf)
                                {
                                    animator.SetBool("Walking", true);
                                }
                                break;
                            //esquerda
                            case ActionType.AT_Left:
                                newPos = myGridPosition + new Vector2(-1, 0);
                                if (grid.checkFreePosition(newPos))
                                {
                                    transform.position = transform.position + new Vector3(-1, 0, 0);
                                    reinforceValidWalkMovement();
                                }
                                else
                                    penalizeInvalidWalkMovement();

                                transform.rotation = Quaternion.Euler(0, 270, 0);
                                if (animator.gameObject.activeSelf)
                                {
                                    animator.SetBool("Walking", true);
                                }
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
                            //Wait
                            case ActionType.AT_Wait:
                                penalizeStopAction();
                                break;
                            default:
                                break;
                        }
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
                                                    transform.parent.position.y + 0.3f,
                                                    Mathf.RoundToInt(transform.position.z)),
                                          bombPrefab.transform.rotation,
                                          transform.parent);
            bomb.GetComponent<Bomb>().bomberman = this;
            bomb.GetComponent<Bomb>().grid = grid;
            bomb.GetComponent<Bomb>().scenarioId = scenarioId;

            myBombManager.addBomb(bomb);
            grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
            grid.enableObjectOnGrid(StateType.ST_Danger, bomb.GetComponent<Bomb>().GetGridPosition());
            bomb.GetComponent<Bomb>().CreateDangerZone(false);


            AddRewardToAgent(this, Config.REWARD_VALID_BOMB_ACTION, "Agente" + playerNumber + " colocou uma bomba");

            bombCount++;
            canDropBombs = false;
            isInDanger = true;
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                bombermanVillain = other.gameObject.GetComponent<DestroySelf>().bombermanOwner;
            }
        }
    }

    private void defaultKillCode()
    {
        dead = true;
        grid.clearAgentOnGrid(this);
        playerModel.SetActive(false);
        transform.localPosition = initialPosition;
    }

    private void killAgent()
    {
        defaultKillCode();

        myPlayerManager.addDeadCount();
        Done();
    }

    private void internalUpdate()
    {
        RequestDecision();
    }

    public bool WaitIterationActions()
    {
        if (wasInitialized && !dead && !IsDone())
        {
            internalUpdate();
            return true;
        }

        return false;
    }
}
