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

public class Player : Agent
{
    public GlobalStateManager globalManager;

    [Header("Specific to Player")]
    private BombermanAcademy academy;

    //Player parameters
    [Range (1, 2)] //Enables a nifty slider in the editor. Indicates what player this is: P1 or P2
    public int playerNumber = 1;

    public float moveSpeed = 5f;
    public bool canDropBombs;
    public bool dead = false;
    public bool canMove = true;
    public GameObject bombPrefab;
    public Transform Target;
    public Grid grid;

    private Rigidbody rigidBody;
    private Transform myTransform;
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
    
    public float timeBetweenDecisionsAtInference;
    private float timeSinceDecision;

    private GameObject playerModel;

    public Vector2 GetGridPosition()
    {
        Node n = grid.NodeFromWorldPoint(myTransform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public Vector2 GetOldGridPosition()
    {
        Node n = grid.NodeFromWorldPoint(oldLocalPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        isInDanger = false;
        canDropBombs = true;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;

        academy = FindObjectOfType(typeof(BombermanAcademy)) as BombermanAcademy;
        playerModel = transform.Find("PlayerModel").gameObject;

        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();

        initialPosition = myTransform.position;
        oldLocalPosition = myTransform.localPosition;

        Vector3 gridTarget3d = Target.transform.localPosition - Vector3.one;
        targetGridPosition = new Vector2(Mathf.RoundToInt(gridTarget3d.x), Mathf.RoundToInt(gridTarget3d.z));

        //grid.updateAgentOnGrid(this); // dando erro de referencia pois o grid não foi iniciado ainda. Porém o grid recupera essa informação quando inicia.
    }

    public override void AgentReset()
    {
        this.transform.position = initialPosition;
        this.rigidBody.angularVelocity = Vector3.zero;
        this.rigidBody.velocity = Vector3.zero;
        
        canMove = true;
        hasPlacedBomb = false;
        canDropBombs = true;
        dead = false;
        isInDanger = false;
        closestDistance = float.MaxValue;
        previousDistance = float.MaxValue;

        ServiceLocator.GetBombManager().clearBombs();
        ServiceLocator.GetLogManager().localEpisodePrint(localEpisode++);
        ServiceLocator.GetBlocksManager().resetBlocks();
        playerModel.SetActive(true);
        grid.refreshNodesInGrid();
    }

    public override void CollectObservations()
    {
        myGridPosition = GetGridPosition();
        AddVectorObs(myGridPosition); //add +2

        //adicionando posição do objetivo
        AddVectorObs(targetGridPosition); //add +2

        //velocidade do agente
        /*float velX = rigidBody.velocity.x / moveSpeed;
        float velZ = rigidBody.velocity.z / moveSpeed;
        AddVectorObs(velX);
        AddVectorObs(velZ);*/

        AddVectorObs(canDropBombs ? 1 : 0);
        AddVectorObs(isInDanger ? 1 : 0);
        AddVectorObs(ServiceLocator.GetBombManager().existsBombOrDanger() ? 1 : 0);

        //adicionando grid de observação da posição dos agentes
        for (int y = grid.GetGridSizeY() - 1; y >= 0; --y)
        {
            for (int x = 0; x < grid.GetGridSizeX(); ++x)
            {
                AddVectorObs((int)grid.NodeFromPos(x, y).stateType);
            }
        }

        ServiceLocator.GetLogManager().statePrint("Agent" + playerNumber,
                                                    myGridPosition,
                                                    targetGridPosition,
                                                    //new Vector2(velX, velZ),
                                                    grid.gridToString(),
                                                    canDropBombs,
                                                    isInDanger,
                                                    ServiceLocator.GetBombManager().existsBombOrDanger());
        
    }

    private void penalizeInvalidMovement()
    {
        AddReward(-0.006f);
        ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " tentou andar sem poder", -0.006f);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Monitor.Log("CumulativeReward", GetCumulativeReward(), MonitorType.text, transform);
        if (Input.GetKeyUp(KeyCode.J))
        {
            Debug.Log("GridPos" + GetGridPosition());
        }

        if (!dead)
        {
            ServiceLocator.GetLogManager().localStepPrint(this);

            ActionType action = ActionTypeExtension.convert((int)vectorAction[0]);

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
                        myTransform.position = myTransform.position + new Vector3(0, 0, 1);
                    }
                    else
                        penalizeInvalidMovement();

                    myTransform.rotation = Quaternion.Euler(0, 0, 0);
                    animator.SetBool("Walking", true);
                    break;
                //baixo
                case ActionType.AT_Down:
                    //rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
                    newPos = myGridPosition + new Vector2(0, -1);
                    if (grid.checkFreePosition(newPos))
                    {
                        myTransform.position = myTransform.position + new Vector3(0, 0, -1);
                    }
                    else
                        penalizeInvalidMovement();

                    myTransform.rotation = Quaternion.Euler(0, 180, 0);
                    animator.SetBool("Walking", true);
                    break;
                //direita
                case ActionType.AT_Right:
                    //rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                    newPos = myGridPosition + new Vector2(1, 0);
                    if (grid.checkFreePosition(newPos))
                    {
                        myTransform.position = myTransform.position + new Vector3(1, 0, 0);
                    }
                    else
                        penalizeInvalidMovement();

                    myTransform.rotation = Quaternion.Euler(0, 90, 0);
                    animator.SetBool("Walking", true);
                    break;
                //esquerda
                case ActionType.AT_Left:
                    //rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                    newPos = myGridPosition + new Vector2(-1, 0);
                    if (grid.checkFreePosition(newPos))
                    {
                        myTransform.position = myTransform.position + new Vector3(-1, 0, 0);
                    }
                    else
                        penalizeInvalidMovement();

                    myTransform.rotation = Quaternion.Euler(0, 270, 0);
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
                        AddReward(-0.001f);
                        ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " tentou colocar bomba sem poder", -0.006f);
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
                AddReward(0.05f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " melhor aproximacao do objetivo", 0.05f);
            }

            if (distanceToTarget < previousDistance)
            {
                AddReward(0.001f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " se aproximou", 0.001f);
            }
            previousDistance = distanceToTarget;

            //penalidade de tempo
            AddReward(-0.003f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " sofreu penalidade de tempo", -0.003f);

            if (ServiceLocator.GetBombManager().existsBombOrDanger())
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
            }

            ServiceLocator.GetLogManager().rewardResumePrint(GetReward(), GetCumulativeReward());
            ServiceLocator.GetLogManager().actionPrint("Agent" + playerNumber, action);

            grid.updateAgentOnGrid(this);
            oldLocalPosition = myTransform.localPosition;
        }
    }

    /// <summary>
    /// Drops a bomb beneath the player
    /// </summary>
    private void DropBomb ()
    {
        if (bombPrefab)
        { 
            float temp = Mathf.RoundToInt(myTransform.position.x) - myTransform.position.x >= 0.0f ? -0.5f : 0.5f;

            GameObject bomb = Instantiate(bombPrefab, 
                                        new Vector3(Mathf.RoundToInt(myTransform.position.x) + temp,
                                                    Mathf.RoundToInt(myTransform.position.y),
                                                    Mathf.RoundToInt(myTransform.position.z)),
                                          bombPrefab.transform.rotation,
                                          myTransform.parent);
            bomb.GetComponent<Bomb>().bomberman = this;
            bomb.GetComponent<Bomb>().grid = grid;

            ServiceLocator.GetBombManager().addBomb(bomb);
            grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());
            bomb.GetComponent<Bomb>().CreateDangerZone();

            AddReward(0.006f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " colocou uma bomba", 0.006f);

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
                dead = true;
                AddReward(-1f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " atingido por explosao", -1f);
                globalManager.PlayerDied(playerNumber);

                grid.clearAgentOnGrid(this);

                //Destroy(gameObject);
                playerModel.SetActive(false);
                myTransform.position = initialPosition;

                Invoke("DoneWithDelay", 3.0f);
                //Done();
            }
        }
        else if (other.CompareTag("Target"))
        {
            AddReward(1.0f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou o objetivo", 1.0f);
            Done();
        }
        /* Agente toma muita recompensa negativa e desiste de colocar bomba muito rápido*/
        else if (other.CompareTag("Danger"))
        {
            isInDanger = true;
            float dangerLevel = Mathf.Abs(other.gameObject.GetComponent<Danger>().GetDangerLevelOfPosition(this));
            dangerLevel *= -0.1f;
            AddReward(dangerLevel);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " em area de perigo", dangerLevel);
        }
    }

    private void DoneWithDelay()
    {
        Done();
    }

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    private void WaitTimeInference()
    {
        if (!dead)
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
