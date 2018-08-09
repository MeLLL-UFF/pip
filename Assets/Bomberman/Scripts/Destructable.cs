using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    public int scenarioId;
    bool wasDestroy = false;
    Vector3 initPos;

    private Grid grid;
    private StateType stateType;

	// Use this for initialization
	void Start () {
        stateType = StateType.ST_Block;
        if (scenarioId == 1)
            grid = GameObject.Find("GridSystem1").GetComponent<Grid>();
        else if (scenarioId == 2)
            grid = GameObject.Find("GridSystem2").GetComponent<Grid>();

        wasDestroy = false;
        initPos = transform.position;
        ServiceLocator.getManager(scenarioId).GetBlocksManager().addBlock(this);
        grid.enableObjectOnGrid(stateType, GetGridPosition());
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
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
                Player bomberman = other.gameObject.GetComponent<DestroySelf>().bomberman;
                if (bomberman != null)
                {
                    bomberman.AddReward(Config.REWARD_BLOCK_DESTROY);
                    ServiceLocator.getManager(scenarioId).GetLogManager().rewardPrint("Agente" + bomberman.playerNumber + " destruiu um bloco", Config.REWARD_BLOCK_DESTROY);
                }
                else
                {
                    ServiceLocator.getManager(scenarioId).GetLogManager().print("Bomba nula");
                }

                gameObject.SetActive(false);
                grid.disableObjectOnGrid(stateType, GetGridPosition());
                //Destroy(gameObject, 0.1f);
            }

        }
    }
}
