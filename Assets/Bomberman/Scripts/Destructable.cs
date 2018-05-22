﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    bool wasDestroy = false;
    Vector3 initPos;

    private Grid grid;
    private StateType stateType;

	// Use this for initialization
	void Start () {
        stateType = StateType.ST_Block;
        grid = GameObject.Find("GridSystem").GetComponent<Grid>();
        wasDestroy = false;
        initPos = transform.position;
        ServiceLocator.GetBlocksManager().addBlock(this);
        grid.enableObjectOnGrid(stateType, GetGridPosition());
    }

    public Vector2 GetGridPosition()
    {
        Node n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void reset()
    {
        gameObject.SetActive(true);
        wasDestroy = false;
        transform.position = initPos;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            if (!wasDestroy)
            {
                wasDestroy = true;
                Bomb bomb = other.gameObject.GetComponent<DestroySelf>().myBomb;
                if (bomb != null)
                {
                    bomb.bomberman.GetComponent<Player>().AddReward(0.1f);
                    ServiceLocator.GetLogManager().rewardPrint("Agente" + bomb.bomberman.GetComponent<Player>().playerNumber + " destruiu um bloco", 0.1f);
                }
                else
                {
                    ServiceLocator.GetLogManager().print("Bomba nula");
                }

                gameObject.SetActive(false);
                grid.disableObjectOnGrid(GetGridPosition());
                //Destroy(gameObject, 0.1f);
            }

        }
    }
}
