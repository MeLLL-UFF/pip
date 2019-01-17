using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    private int scenarioId;
    bool wasDestroy = false;
    public bool randomStart = false;
    public bool randomReset = false;
    private bool assortedActivation;
    private bool initialActivation;
    Vector3 initPos;

    private MyGrid grid;
    private StateType stateType;

    private void Awake()
    {
        initialActivation = gameObject.activeSelf;

        if (randomStart)
        {
            int randomNumber = Random.Range(0, 2);
            if (randomNumber == 0)
            {
                assortedActivation = false;
            }
            else
            {
                assortedActivation = true;
            }

        }
    }

    // Use this for initialization
    void Start () {
        stateType = StateType.ST_Block;

        grid = transform.parent.parent.Find("GridSystem").GetComponent<MyGrid>();
        scenarioId = grid.scenarioId;

        wasDestroy = false;
        initPos = transform.position;
        ServiceLocator.getManager(scenarioId).GetBlocksManager().addBlock(this);
        

        if (randomStart)
        {
            gameObject.SetActive(assortedActivation);
            if (assortedActivation)
                grid.enableObjectOnGrid(stateType, GetGridPosition());
            else
                grid.disableObjectOnGrid(stateType, GetGridPosition());
        }
        else
        {
            gameObject.SetActive(true);
            grid.enableObjectOnGrid(stateType, GetGridPosition());
        }
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void reset()
    {
        wasDestroy = false;
        transform.position = initPos;

        if (randomReset)
        {
            int randomNumber = Random.Range(0, 2);
            if (randomNumber == 0)
            {
                gameObject.SetActive(false);
                grid.disableObjectOnGrid(stateType, GetGridPosition());
            }
            else
            {
                gameObject.SetActive(true);
                grid.enableObjectOnGrid(stateType, GetGridPosition());
            }
        }
        else
        {
            if (randomStart)
            {
                gameObject.SetActive(assortedActivation);
                if (assortedActivation)
                    grid.enableObjectOnGrid(stateType, GetGridPosition());
                else
                    grid.disableObjectOnGrid(stateType, GetGridPosition());
            }
            else
            {
                gameObject.SetActive(initialActivation);
                if (initialActivation)
                    grid.enableObjectOnGrid(stateType, GetGridPosition());
                else
                    grid.disableObjectOnGrid(stateType, GetGridPosition());
            }
        }
    }

    public void attackByHammer(Player hammerman)
    {
        if (!wasDestroy)
        {
            wasDestroy = true;
            
            if (hammerman != null)
            {
                Player.AddRewardToAgent(hammerman, Config.REWARD_BLOCK_DESTROY, "Agente" + 1 + " destruiu um bloco");
            }
            else
            {
                ServiceLocator.getManager(scenarioId).GetLogManager().print("hammerman nulo");
            }

            gameObject.SetActive(false);
            grid.disableObjectOnGrid(stateType, GetGridPosition());
            //Destroy(gameObject, 0.1f);
        }
    }
}
