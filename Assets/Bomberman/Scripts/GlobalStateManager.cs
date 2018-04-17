﻿/*
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
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalStateManager : MonoBehaviour
{
    private int deadPlayers = 0;
    private int deadPlayerNumber = -1;

    //default is false
    private bool[] deadPlayersControl = new bool[2];

    public Text txtRef;
    public Button btnRef;
    public float xMax = 9.0f/2.0f;
    public float zMax = 8.0f/2.0f;

    private void Start()
    {
        btnRef.gameObject.SetActive(false);
    }

    public void PlayerDied (int playerNumber)
    {
        if (!deadPlayersControl[playerNumber-1])
        {
            deadPlayers++;
            deadPlayersControl[playerNumber - 1] = true;

            if (deadPlayers == 1)
            {
                deadPlayerNumber = playerNumber;
                Invoke("CheckPlayerDeath", .3f);
            }
        }
    }

    void CheckPlayerDeath()
    {
        string temp = "";
        if (deadPlayers == 1)
        {
            if (deadPlayerNumber == 1)
            {
                temp = "Player 2 is the winner!";
            }
            else
            {
                temp = "Player 1 is the winner!";
            }
        }
        else
        {
            temp = "The game ended in a draw!";
        }

        Debug.Log(temp);
        txtRef.text = temp;
        btnRef.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
