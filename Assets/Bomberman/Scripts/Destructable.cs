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

    private Grid grid;
    private StateType stateType;

    public Player bombermanVilain;

    private int discreteTimerAfterExplosion;

    private void Awake()
    {
        discreteTimerAfterExplosion = 0;
        bombermanVilain = null;
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

        grid = transform.parent.parent.Find("GridSystem").GetComponent<Grid>();
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
        BinaryNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void reset()
    {
        discreteTimerAfterExplosion = 0;
        bombermanVilain = null;
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

    /*public void attackByHammer(Player hammerman)
    {
        if (!wasDestroy)
        {
            wasDestroy = true;
            
            if (hammerman != null)
            {
                Player.AddRewardToAgent(hammerman, Config.REWARD_BLOCK_DESTROY, "Agente" + hammerman.getPlayerNumber() + " destruiu um bloco");
            }
            else
            {
                ServiceLocator.getManager(scenarioId).GetLogManager().print("hammerman nulo");
            }

            gameObject.SetActive(false);
            grid.disableObjectOnGrid(stateType, GetGridPosition());
            //Destroy(gameObject, 0.1f);
        }
    }*/

    //Bomb code
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Explosion"))
        {
            if (!wasDestroy)
            {
                wasDestroy = true;
                bombermanVilain = other.gameObject.GetComponent<DestroySelf>().bombermanOwner;

                ServiceLocator.getManager(scenarioId).GetBlocksManager().addBlockToDestroy(this);
            }
        }
    }

    public bool destroyMethod()
    {
        ++discreteTimerAfterExplosion;

        if (discreteTimerAfterExplosion >= Config.EXPLOSION_TIMER_DISCRETE)
        {
            if (bombermanVilain != null)
            {
                Player.AddRewardToAgent(bombermanVilain, Config.REWARD_BLOCK_DESTROY, "Agente" + bombermanVilain.getPlayerNumber() + " destruiu um bloco");
            }
            else
            {
                ServiceLocator.getManager(scenarioId).GetLogManager().print("Bomberman nulo");
            }

            gameObject.SetActive(false);
            grid.disableObjectOnGrid(stateType, GetGridPosition());

            return true;
        }

        return false;
    }
}
