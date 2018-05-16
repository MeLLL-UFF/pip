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

    public Vector2 GetGridPosition()
    {
        Vector2 myPos = new Vector2(transform.localPosition.x, transform.localPosition.z) - Vector2.one;
        return myPos;
    }

    public Vector2 GetOldGridPosition()
    {
        Vector2 myPos = new Vector2(oldLocalPosition.x, oldLocalPosition.z) - Vector2.one;
        return myPos;
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
        targetGridPosition = new Vector2(gridTarget3d.x, gridTarget3d.z);

        ServiceLocator.GetPlayersManager().updatePlayerOnGrid(this);
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

        ServiceLocator.GetBlocksManager().resetBlocks();
        ServiceLocator.GetPlayersManager().updatePlayerOnGrid(this);
        ServiceLocator.GetLogManager().print("Agente foi resetado");
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
        int[,] playerGrid = ServiceLocator.GetPlayersManager().getGrid();
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                AddVectorObs(playerGrid[x, y]);
            }
        }

        int[,] blocksGrid = ServiceLocator.GetBlocksManager().getGrid();
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                AddVectorObs(blocksGrid[x, y]);
            }
        }

        int[,] bombsGrid = ServiceLocator.GetBombManager().getGrid();
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 7; ++y)
            {
                AddVectorObs(bombsGrid[x, y]);
            }
        }

        ServiceLocator.GetLogManager().statePrint("Agent"+playerNumber,
                                                  gridPos,
                                                  targetGridPosition,
                                                  new Vector2(velX, velZ),
                                                  ServiceLocator.GetPlayersManager().gridToString(),
                                                  ServiceLocator.GetBlocksManager().gridToString(),
                                                  ServiceLocator.GetBombManager().gridToString());
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
        if (!dead)
        {
            hasPlacedBomb = false;
            bool putBomb = false;

            animator.SetBool("Walking", false);
            float vertical = Mathf.Clamp(vectorAction[0], -1, 1);
            float horizontal = Mathf.Clamp(vectorAction[1], -1, 1);
            float bombVal = Mathf.Clamp(vectorAction[2], 0, 1);
            putBomb = bombVal > 0.99f;
            bool stopped = (vertical > -0.5f && vertical < 0.5f) && (horizontal > -0.5f && horizontal < 0.5f);

            //variáveis de controle para o log
            bool key_W = vertical > 0.5f;
            bool key_S = vertical < -0.5f;
            bool key_D = horizontal > 0.5f;
            bool key_A = horizontal < -0.5f;
            //--------------------------------

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

            if (!stopped)
            {
                //cima
                if (vertical > 0.5f)
                {
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
                    animator.SetBool("Walking", true);
                }
                //baixo
                else if (vertical < -0.5f)
                {
                    rigidBody.velocity = new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
                    animator.SetBool("Walking", true);
                }

                //direita
                if (horizontal > 0.5f)
                {
                    rigidBody.velocity = new Vector3(moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                    animator.SetBool("Walking", true);
                }
                //esquerda
                else if (horizontal < -0.5f)
                {
                    rigidBody.velocity = new Vector3(-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
                    animator.SetBool("Walking", true);
                }

                float dirX = this.transform.position.x - oldPosition.x;
                float dirZ = this.transform.position.z - oldPosition.z;

                if (Mathf.Abs(dirX) > Mathf.Abs(dirZ))
                {
                    if (dirX >= 0)
                    {
                        myTransform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    else
                    {
                        myTransform.rotation = Quaternion.Euler(0, 270, 0);
                    }
                }
                else
                {
                    if (dirZ >= 0)
                    {
                        myTransform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    else
                    {
                        myTransform.rotation = Quaternion.Euler(0, 180, 0);
                    }
                }
            }

            if (canDropBombs && putBomb)
            { //Drop bomb
                hasPlacedBomb = true;
                DropBomb();
            }

            ServiceLocator.GetLogManager().actionPrint("Agent" + playerNumber, key_W, key_S, key_D, key_A, putBomb, canDropBombs, stopped);

            ServiceLocator.GetPlayersManager().updatePlayerOnGrid(this);

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

            ServiceLocator.GetBombManager().addBomb(bomb);

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

                ServiceLocator.GetPlayersManager().clearPlayerOnGrid(this);

                //Destroy(gameObject);
                myTransform.position = new Vector3(99999, 1, 99999);

                Invoke("DoneWithDelay", 3.0f);
            }
           
        }
    }

    private void DoneWithDelay()
    {
        Done();
    }
}
