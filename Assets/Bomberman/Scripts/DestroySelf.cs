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

/// <summary>
/// Small script for easily destroying an object after a while
/// </summary>
public class DestroySelf : MonoBehaviour
{
    public int scenarioId;
    public Bomb myBomb = null;
    public int id;

    public Player bomberman;
    public Grid grid;

    private StateType stateType;

    public int discrete_timer = 0;

    private bool wasDestroyed;

    private void Awake()
    {
        stateType = StateType.ST_Fire;
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void forceDestroy()
    {
        grid.disableObjectOnGrid(stateType, GetGridPosition());
        gameObject.SetActive(false);
        GameObject.Destroy(gameObject);
    }

    void Start ()
    {
        //Debug.Log(Delay);
        //Invoke("myDestroy", Config.EXPLOSION_TIMER);

        discrete_timer = 0;
        wasDestroyed = false;
    }

    public bool iterationUpdate()
    {
        if (!wasDestroyed)
        {
            discrete_timer += 1;
            if (discrete_timer >= Config.EXPLOSION_TIMER_DISCRETE)
            {
                return myDestroy();
            }
        }

        return false;
    }

    bool myDestroy()
    {
        forceDestroy();
        wasDestroyed = true;
        return true;
    }
}
