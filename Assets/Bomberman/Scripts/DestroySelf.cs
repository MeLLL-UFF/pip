
using UnityEngine;
using System.Collections;

/// <summary>
/// Small script for easily destroying an object after a while
/// </summary>
public class DestroySelf : MonoBehaviour
{
    //public int scenarioId;
    public Player bombermanOwner = null;
    public int bombermanOwnerNumber = -1;
    public ulong id;

    public Grid grid;

    private StateType stateType;

    public int discrete_timer = 0;

    private bool wasDestroyed;

    private void Awake()
    {
        stateType = StateType.ST_Fire;
        discrete_timer = 0;
        wasDestroyed = false;
    }

    public Vector2 GetGridPosition()
    {
        BinaryNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }


    public void forceDestroy()
    {
        //desativamos o colisor
        gameObject.GetComponent<BoxCollider>().enabled = false;

        Vector2 p = GetGridPosition();

        //testamos se há outro colisor de explosão no mesmo lugar
        //se não há, então desativamos o estado de fogo no grid
        if (!grid.hasAnotherFireInThisPosition(p))
        {
            grid.disableObjectOnGrid(stateType, p);
        }
        /*else
        {
            Debug.Log("há outro fogo nessa posição");
        }*/
  
        gameObject.SetActive(false);
        Destroy(gameObject);
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
