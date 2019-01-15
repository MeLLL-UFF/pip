using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public int scenarioId;
    public GameObject explosionPrefab;
    public GameObject dangerPrefab;
    public LayerMask levelMask;
    public bool exploded = false;
    public ulong bombId;

    //public float timer = 0;
    public int discrete_timer = 0;
    //int lastIterationCount = 0;

    public Player bomberman;
    public Grid grid;

    private StateType stateType;


    // Awake is called after Instantiate Prefab, Start no.
    private void Awake()
    {
        stateType = StateType.ST_Bomb;
        discrete_timer = 0;
    }

    public bool iterationUpdate()
    {
        if (!exploded)
        {
            discrete_timer += 1;
            if (discrete_timer >= Config.BOMB_TIMER_DISCRETE)
            {
                return Explode(false);
            }
        }

        return false;
    }

    public Vector2 GetGridPosition()
    {
        BinaryNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void autoDestroy()
    {
        grid.disableObjectOnGrid(stateType, GetGridPosition());
        Destroy(gameObject);
    }

    void createExplosionObject(bool forceTimeout, Vector3 pos, Quaternion rotation)
    {
        //Bomb code
        GameObject explosionObject = Instantiate(explosionPrefab, pos, rotation, transform.parent);
        explosionObject.GetComponent<DestroySelf>().bombermanOwner = bomberman;
        explosionObject.GetComponent<DestroySelf>().grid = grid;

        if (bomberman != null)
            explosionObject.GetComponent<DestroySelf>().bombermanOwnerNumber = bomberman.getPlayerNumber();

        if (forceTimeout)
        {
            explosionObject.GetComponent<DestroySelf>().discrete_timer = Config.EXPLOSION_TIMER_DISCRETE;
        }

        ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
        grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
        grid.disableObjectOnGrid(StateType.ST_Danger, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
    }

    bool Explode(bool forceTimeout)
    {
        //Bomb code
        createExplosionObject(forceTimeout, transform.position, Quaternion.identity);

        /*StartCoroutine(*/
        CreateExplosions(Vector3.forward, forceTimeout);
        CreateExplosions(Vector3.right, forceTimeout);
        CreateExplosions(Vector3.back, forceTimeout);
        CreateExplosions(Vector3.left, forceTimeout);

        GetComponent<MeshRenderer>().enabled = false;
        exploded = true;
        transform.Find("Collider").gameObject.SetActive(false);

        if (bomberman != null)
        {
            bomberman.canDropBombs = true;
        }

        grid.disableObjectOnGrid(stateType, GetGridPosition());
        //grid.disableObjectOnGrid(StateType.ST_Danger, GetGridPosition());

        Destroy(gameObject);

        return true;
    }

    private void CreateExplosions(Vector3 direction, bool forceTimeout)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position /*+ new Vector3(0, .5f, 0)*/, direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                createExplosionObject(forceTimeout, transform.position + (i * direction), explosionPrefab.transform.rotation);
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))  
                {
                    createExplosionObject(forceTimeout, transform.position + (i * direction), explosionPrefab.transform.rotation);
                }
                else if (hit.collider.CompareTag("Bomb") )
                {
                    Bomb otherBomb = hit.collider.GetComponentInParent<Bomb>();
                    if (otherBomb.bombId != bombId)
                    {
                        createExplosionObject(forceTimeout, transform.position + (i * direction), explosionPrefab.transform.rotation);
                    }
                    /*else
                    {
                        Debug.Log("Mesma bomba");
                    }*/
                }

                break;
            }
        }

        //yield return new WaitForSeconds(.05f);
        //yield return null;
    }

    private GameObject CreateDangerObject(bool forceTimeout, Vector3 pos, Quaternion rotation)
    {
        GameObject dangerObject = Instantiate(dangerPrefab, pos, rotation, transform.parent);
        dangerObject.GetComponent<Danger>().bombermanOwner = bomberman;

        if (bomberman != null)
            dangerObject.GetComponent<Danger>().bomberOwnerNumber = bomberman.getPlayerNumber();

        dangerObject.GetComponent<Danger>().grid = grid;

        if (forceTimeout)
        {
            dangerObject.GetComponent<Danger>().discrete_timer = Config.BOMB_TIMER_DISCRETE;
        }

        ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());

        return dangerObject;
    }

    private void createDangerObjectAndEnableOnGrid(bool forceTimeout, Vector3 pos)
    {
        GameObject dangerObject = CreateDangerObject(forceTimeout, pos, dangerPrefab.transform.rotation);
        grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
    }

    public void CreateDangerZone(bool forceTimeout)
    {
        // in current position
        CreateDangerObject(forceTimeout, transform.position, Quaternion.identity);

        // in around positions
        CreateDangers(Vector3.forward, forceTimeout);
        CreateDangers(Vector3.right, forceTimeout);
        CreateDangers(Vector3.back, forceTimeout);
        CreateDangers(Vector3.left, forceTimeout);
    }

    private void CreateDangers(Vector3 direction, bool forceTimeout)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position /*+ new Vector3(0, .5f, 0)*/, direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                createDangerObjectAndEnableOnGrid(forceTimeout, transform.position + (i * direction));
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))
                {
                    createDangerObjectAndEnableOnGrid(forceTimeout, transform.position + (i * direction));
                }
                else if (hit.collider.CompareTag("Bomb"))
                {
                    Bomb otherBomb = hit.collider.GetComponentInParent<Bomb>();
                    if (otherBomb.bombId != bombId)
                    {
                        createDangerObjectAndEnableOnGrid(forceTimeout, transform.position + (i * direction));
                    }
                    /*else
                    {
                        Debug.Log("Mesma bomba");
                    }*/
                }

                break;
            }
        }

        //yield return null;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!exploded && other.CompareTag("Explosion"))
        {
            //CancelInvoke("Explode");
            // se explodiu ativada por outra bomba, precisamos informar que o tempo para explodir já passou
            if (Explode(true))
                ServiceLocator.getManager(scenarioId).GetBombManager().removeBomb(bombId);
        }
    }
}
