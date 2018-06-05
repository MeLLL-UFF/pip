
using UnityEngine;
using System.Collections;

/// <summary>
/// Small script for easily destroying an object after a while
/// </summary>
public class Danger : MonoBehaviour
{
    public Bomb myBomb = null;
    public int id;

    public float Delay = 3.0f;
    //Delay in seconds before destroying the gameobject

    public Grid grid;
    private StateType stateType;

    private float dangerLevelOfPosition;
    private float timePassed;

    private void Awake()
    {
        stateType = StateType.ST_Danger;
    }

    void Start()
    {
        timePassed = 0;
        dangerLevelOfPosition = timePassed / Delay;
        Invoke("myDestroy", Delay);
    }

    private void FixedUpdate()
    {
        timePassed += Time.fixedDeltaTime;

        if (timePassed > Delay)
        {
            timePassed = Delay;
        }

        dangerLevelOfPosition = timePassed / Delay;
    }

    public float GetDangerLevelOfPosition(Player player)
    {
        //penalty The danger value is negative if the bomb has been placed by the player and positive if it has been placed by an opponent(or environment)
        float penalty = myBomb.bomberman.playerNumber == player.playerNumber ? -1.0f : 1.0f;
        return dangerLevelOfPosition * penalty;
    }

    public string GetDangerLevelOfPositionToPrint()
    {
        return (dangerLevelOfPosition * -1.0f).ToString("0.000");
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void forceDestroy()
    {
        grid.disableObjectOnGrid(stateType, GetGridPosition());
        Destroy(gameObject);
    }

    void myDestroy()
    {
        ServiceLocator.GetBombManager().removeDanger(this.id);
        forceDestroy();
    }
}
