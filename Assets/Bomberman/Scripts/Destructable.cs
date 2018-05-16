using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour {

    bool wasDestroy = false;
    Vector3 initPos;

	// Use this for initialization
	void Start () {
        wasDestroy = false;
        initPos = transform.position;
        ServiceLocator.GetBlocksManager().addBlock(this);
    }

    public Vector2 GetGridPosition()
    {
        Vector2 myPos = new Vector2(transform.localPosition.x, transform.localPosition.z) - Vector2.one;
        return myPos;
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
                ServiceLocator.GetBlocksManager().disableBlockOnGrid(this);
                //Destroy(gameObject, 0.1f);
            }

        }
    }
}
