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

    //Player parameters
    [Range (1, 2)] //Enables a nifty slider in the editor. Indicates what player this is: P1 or P2
    public int playerNumber = 1;

    public float moveSpeed = 5f;
    public bool canDropBombs = true;
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
    private Vector3 oldPosition;
    private Vector3 oldLocalPosition;
    private float previousDistance = float.MaxValue;
    private bool hasPlacedBomb = false;
    private Vector2 targetGridPosition = new Vector2(7, 0);

    private int localEpisode = 1;

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
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();

        initialPosition = myTransform.position;
        oldPosition = myTransform.position;
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
        //canDropBombs = true;
        dead = false;

        ServiceLocator.GetBombManager().clearBombs();
        grid.refreshNodesInGrid();
        //grid.updateAgentOnGrid(this);
        ServiceLocator.GetLogManager().localEpisodePrint(localEpisode++);

        Invoke("delayReset", 0.0f);
        //grid.printGrid();
    }

    void delayReset()
    {
        ServiceLocator.GetBlocksManager().resetBlocks();
    }

    public override void CollectObservations()
    {
        //Vector3 relativePosition = Target.position - this.transform.position;

        //posição relativa
        //AddVectorObs(relativePosition.x / globalManager.xMax);
        //AddVectorObs(relativePosition.z / globalManager.zMax);
        Vector2 gridPos = GetGridPosition();
        AddVectorObs(gridPos); //add +2

        //adicionando posição do objetivo
        AddVectorObs(targetGridPosition); //add +2

        //velocidade do agente
        float velX = rigidBody.velocity.x / moveSpeed;
        float velZ = rigidBody.velocity.z / moveSpeed;
        AddVectorObs(velX);
        AddVectorObs(velZ);

        //AddVectorObs(System.Convert.ToInt32(hasPlacedBomb));


        //colocar por enquanto apenas as 3 bombas mais próximas
        //Vector3 agentPosition = myTransform.position;
        // List<Bomb> bombsList = ServiceLocator.GetBombManager().getBombs(2);

        //Debug.Log("Bombas: " + bombsList.Count);

        /*for (int i = 0; i < bombsList.Count; i++)
        {
            if (bombsList[i] != null)
            {
                //Debug.Log("Bombas: " + i + "  timer: " + bombsList[i].timer);
                Vector3 relativePositionToBomb = bombsList[i].transform.position - agentPosition;
                AddVectorObs(relativePositionToBomb.x / globalManager.xMax);
                AddVectorObs(relativePositionToBomb.z / globalManager.zMax);
                AddVectorObs(bombsList[i].timer / 3.0f);
            }
            else
            {
                AddVectorObs(1.0f);
                AddVectorObs(1.0f);
                AddVectorObs(0.0f);
            }
        }*/

        //adicionando grid de observação da posição dos agentes
        for (int x = 0; x < grid.GetGridSizeX(); ++x)
        {
            for (int y = 0; y < grid.GetGridSizeY(); ++y)
            {
                AddVectorObs((int)grid.NodeFromPos(x, y).stateType);
            }
        }

        ServiceLocator.GetLogManager().statePrint("Agent"+playerNumber,
                                                  gridPos,
                                                  targetGridPosition,
                                                  new Vector2(velX, velZ),
                                                  grid.gridToString());
    }

    private void penaltyNearbyBombs()
    {
        List<Bomb> bombsList = ServiceLocator.GetBombManager().getBombs(2);
        bool penalize = false;
        float distance = 1;

        for (int i = 0; i < bombsList.Count; i++)
        {
            if (bombsList[i] != null)
            {
                distance = Vector3.Distance(myTransform.position, bombsList[i].transform.position);

                if (distance <= 4)
                {
                    penalize = true;
                    break;
                }
            }
        }

        if (penalize)
        {
            distance = distance + 1;
            float result = -0.1f * (4.0f / distance);
            AddReward(result);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " proximo da bomba", result);
        }

        if (bombsList.Count > 0 && !penalize)
        { 
            AddReward(0.05f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " longe da bomba", 0.05f);
        }
        
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (Input.GetKeyUp(KeyCode.J))
        {
            Debug.Log("GridPos" + GetGridPosition());
        }

        ServiceLocator.GetLogManager().localStepPrint(this);

        ActionType action = ActionTypeExtension.convert((int)vectorAction[0]);

        //if (!dead)
        {
            hasPlacedBomb = false;
            animator.SetBool("Walking", false);
            
            //-----------------------------------------------------------------------------------------------------


            //recompensas
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

            //alcançou o objetivo
            if (distanceToTarget < 1.12f)
            {
                AddReward(1.0f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " alcancou o objetivo", 1.0f);
                Done();
            }

            //se aproximando
            if (distanceToTarget < previousDistance + 1)
            {
                AddReward(0.02f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " se aproximou do objetivo", 0.02f);
            }

            //Adicionar penalidade por estar próximo demais de uma bomba
            //penaltyNearbyBombs();

            //penalidade de tempo
            AddReward(-0.05f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " sofreu penalidade de tempo", -0.05f);

            previousDistance = distanceToTarget;

            switch (action)
            {
                //cima
                case ActionType.AT_Up:
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
                    myTransform.rotation = Quaternion.Euler(0, 0, 0);
                    animator.SetBool("Walking", true);
                    break;
                //baixo
                case ActionType.AT_Down:
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
                    myTransform.rotation = Quaternion.Euler(0, 180, 0);
                    animator.SetBool("Walking", true);
                    break;
                //direita
                case ActionType.AT_Right:
                    rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                    myTransform.rotation = Quaternion.Euler(0, 90, 0);
                    animator.SetBool("Walking", true);
                    break;
                //esquerda
                case ActionType.AT_Left:
                    rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
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
                    break;
                //Wait
                case ActionType.AT_Wait:
                default:
                    break;
            }

            ServiceLocator.GetLogManager().rewardResumePrint(GetReward(), GetCumulativeReward());
            ServiceLocator.GetLogManager().actionPrint("Agent" + playerNumber, action, canDropBombs);

            grid.updateAgentOnGrid(this);

            oldPosition = this.transform.position;
            oldLocalPosition = myTransform.localPosition;
        }
    }

    /// <summary>
    /// Drops a bomb beneath the player
    /// </summary>
    private void DropBomb ()
    {
        if (bombPrefab)
        { //Check if bomb prefab is assigned first

            float temp = Mathf.RoundToInt(myTransform.position.x) - myTransform.position.x >= 0.0f ? -0.5f : 0.5f;

            GameObject bomb = Instantiate(bombPrefab, 
                                        new Vector3(Mathf.RoundToInt(myTransform.position.x) + temp,
                                                    Mathf.RoundToInt(myTransform.position.y),
                                                    Mathf.RoundToInt(myTransform.position.z)),
                                          bombPrefab.transform.rotation,
                                          myTransform.parent);
            bomb.GetComponent<Bomb>().bomberman = gameObject;
            bomb.GetComponent<Bomb>().grid = grid;

            ServiceLocator.GetBombManager().addBomb(bomb);
            grid.enableObjectOnGrid(StateType.ST_Bomb, bomb.GetComponent<Bomb>().GetGridPosition());

            AddReward(0.05f);
            ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " colocou uma bomba", 0.05f);

            canDropBombs = false;
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                dead = true;
                AddReward(-1.0f);
                ServiceLocator.GetLogManager().rewardPrint("Agente" + playerNumber + " atingido por explosao", -1.0f);
                globalManager.PlayerDied(playerNumber);

                grid.clearAgentOnGrid(this);

                //Destroy(gameObject);
                myTransform.position = new Vector3(99999, 1, 99999);

                //Invoke("DoneWithDelay", 3.0f);
                Done();
            }
           
        }
    }

    private void DoneWithDelay()
    {
        Done();
    }
}
