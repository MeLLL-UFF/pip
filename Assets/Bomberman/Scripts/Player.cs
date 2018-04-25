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
    private float previousDistance = float.MaxValue;
    private bool hasPlacedBomb = false;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        rigidBody = GetComponent<Rigidbody>();
        myTransform = transform;
        animator = myTransform.Find("PlayerModel").GetComponent<Animator>();

        initialPosition = myTransform.position;
        oldPosition = myTransform.position;
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
    }

    public override void CollectObservations()
    {
        Vector3 relativePosition = Target.position - this.transform.position;

        //posição relativa
        AddVectorObs(relativePosition.x / globalManager.xMax);
        AddVectorObs(relativePosition.z / globalManager.zMax);

        //velocidade do agente
        AddVectorObs(rigidBody.velocity.x / moveSpeed);
        AddVectorObs(rigidBody.velocity.z / moveSpeed);

        AddVectorObs(System.Convert.ToInt32(hasPlacedBomb));

        //colocar por enquanto apenas as 3 bombas mais próximas
        Vector3 agentPosition = myTransform.position;
        List<Bomb> bombsList = ServiceLocator.GetBombManager().getBombs(2);

        Debug.Log("Bombas: " + bombsList.Count);

        for (int i = 0; i < bombsList.Count; i++)
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
        }
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
            AddReward(-0.01f * (4.0f/distance));
        }
        else
        {
            AddReward(0.01f);
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

            //recompensas
            float distanceToTarget = Vector3.Distance(this.transform.position, Target.position);

            //alcançou o objetivo
            if (distanceToTarget < 1.12f)
            {
                AddReward(1.0f);
                Done();
            }

            //se aproximando
            if (distanceToTarget < previousDistance)
            {
                AddReward(0.02f);
            }

            //Adicionar penalidade por estar próximo demais de uma bomba
            penaltyNearbyBombs();

            //penalidade de tempo
            AddReward(-0.05f);

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

            oldPosition = this.transform.position;
        }
    }

    /// <summary>
    /// Updates Player 1's movement and facing rotation using the WASD keys and drops bombs using Space
    /// </summary>
    /*private void UpdatePlayer1Movement ()
    {
        if (Input.GetKey (KeyCode.W))
        { //Up movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
            myTransform.rotation = Quaternion.Euler (0, 0, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.A))
        { //Left movement
            rigidBody.velocity = new Vector3 (-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 270, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.S))
        { //Down movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
            myTransform.rotation = Quaternion.Euler (0, 180, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.D))
        { //Right movement
            rigidBody.velocity = new Vector3 (moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 90, 0);
            animator.SetBool ("Walking", true);
        }

        if (canDropBombs && Input.GetKeyDown (KeyCode.Space))
        { //Drop bomb
            DropBomb ();
        }
    }*/

    /// <summary>
    /// Updates Player 2's movement and facing rotation using the arrow keys and drops bombs using Enter or Return
    /// </summary>
    /*private void UpdatePlayer2Movement ()
    {
        if (Input.GetKey (KeyCode.UpArrow))
        { //Up movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, moveSpeed);
            myTransform.rotation = Quaternion.Euler (0, 0, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.LeftArrow))
        { //Left movement
            rigidBody.velocity = new Vector3 (-moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 270, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.DownArrow))
        { //Down movement
            rigidBody.velocity = new Vector3 (rigidBody.velocity.x, rigidBody.velocity.y, -moveSpeed);
            myTransform.rotation = Quaternion.Euler (0, 180, 0);
            animator.SetBool ("Walking", true);
        }

        if (Input.GetKey (KeyCode.RightArrow))
        { //Right movement
            rigidBody.velocity = new Vector3 (moveSpeed, rigidBody.velocity.y, rigidBody.velocity.z);
            myTransform.rotation = Quaternion.Euler (0, 90, 0);
            animator.SetBool ("Walking", true);
        }

        if (canDropBombs && (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return)))
        { //Drop Bomb. For Player 2's bombs, allow both the numeric enter as the return key or players 
            //without a numpad will be unable to drop bombs
            DropBomb ();
        }
    }*/

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
                                          bombPrefab.transform.rotation);
            bomb.GetComponent<Bomb>().bomberman = gameObject;

            ServiceLocator.GetBombManager().addBomb(bomb);

            canDropBombs = false;
        }
    }

    public void OnTriggerEnter (Collider other)
    {
        if (other.CompareTag ("Explosion"))
        {
            if (!dead)
            {
                Debug.Log("P" + playerNumber + " hit by explosion!");
                dead = true;
                AddReward(-1.0f);
                globalManager.PlayerDied(playerNumber);
                //Destroy(gameObject);
                myTransform.position = new Vector3(99999, 1, 99999);

                Invoke("DoneWithDelay", 3.0f);
            }
           
        }
    }

    private void DoneWithDelay()
    {
        Done();
        AddReward(-1.0f);
    }
}
