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
                Debug.Log("Bomba destruida");
                Bomb bomb = other.gameObject.GetComponent<DestroySelf>().myBomb;
                if (bomb != null)
                {
                    bomb.bomberman.GetComponent<Player>().AddReward(0.02f);
                    Debug.Log("Ganhou recompensa por ter destruído o bloco");
                }
                else
                {
                    Debug.Log("Bomba nula");
                }

                gameObject.SetActive(false);
                //Destroy(gameObject, 0.1f);
            }

        }
    }
}
