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

    public void CreateDangerZone(bool forceTimeout)
    {
        GameObject dangerObject = Instantiate(dangerPrefab, transform.position, Quaternion.identity, transform.parent);
        dangerObject.GetComponent<Danger>().myBomb = this;
        dangerObject.GetComponent<Danger>().grid = grid;
        //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

        if (forceTimeout)
        {
            dangerObject.GetComponent<Danger>().discrete_timer = Config.BOMB_TIMER_DISCRETE;
        }

        ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
        //comentado porque senão vai sobrescrever a bomba no mapa. Código chamado junto com a bomba
        //grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<DestroySelf>().GetGridPosition());

        StartCoroutine(CreateDangers(Vector3.forward, forceTimeout));
        StartCoroutine(CreateDangers(Vector3.right, forceTimeout));
        StartCoroutine(CreateDangers(Vector3.back, forceTimeout));
        StartCoroutine(CreateDangers(Vector3.left, forceTimeout));
    }

    bool Explode(bool forceTimeout)
    {
        //Bomb code
        GameObject explosionObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity, transform.parent);
        explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
        explosionObject.GetComponent<DestroySelf>().grid = grid;
        //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
        explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

        if (forceTimeout)
        {
            explosionObject.GetComponent<DestroySelf>().discrete_timer = Config.EXPLOSION_TIMER_DISCRETE;
        }

        ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
        grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());

        StartCoroutine(CreateExplosions(Vector3.forward, forceTimeout));
        StartCoroutine(CreateExplosions(Vector3.right, forceTimeout));
        StartCoroutine(CreateExplosions(Vector3.back, forceTimeout));
        StartCoroutine(CreateExplosions(Vector3.left, forceTimeout));

        GetComponent<MeshRenderer>().enabled = false;
        exploded = true;
        transform.Find("Collider").gameObject.SetActive(false);

        if (bomberman != null)
        {
            bomberman.canDropBombs = true;
        }

        grid.disableObjectOnGrid(stateType, GetGridPosition());
        grid.disableObjectOnGrid(StateType.ST_Danger, GetGridPosition());

        //comentado porque dá erro ao tentar remover bomba dentro de uma iteração de bombas com foreach
        //ServiceLocator.getManager(scenarioId).GetBombManager().removeBomb(bombId);

        Destroy(gameObject);//, Config.BOMB_TIMER_AFTER_DESTROY);

        return true;
    }

    public void autoDestroy()
    {
        grid.disableObjectOnGrid(stateType, GetGridPosition());
        Destroy(gameObject);
    }

    private IEnumerator CreateExplosions(Vector3 direction, bool forceTimeout)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position /*+ new Vector3(0, .5f, 0)*/, direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                explosionObject.GetComponent<DestroySelf>().grid = grid;
                //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
                explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

                if (forceTimeout)
                    explosionObject.GetComponent<DestroySelf>().discrete_timer = Config.EXPLOSION_TIMER_DISCRETE;

                ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                grid.disableObjectOnGrid(StateType.ST_Danger, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))  
                {
                    GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                    explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                    explosionObject.GetComponent<DestroySelf>().grid = grid;
                    //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
                    explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

                    if (forceTimeout)
                        explosionObject.GetComponent<DestroySelf>().discrete_timer = Config.EXPLOSION_TIMER_DISCRETE;

                    ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                    grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                    grid.disableObjectOnGrid(StateType.ST_Danger, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                }
                else if (hit.collider.CompareTag("Bomb") )
                {
                    Bomb otherBomb = hit.collider.GetComponentInParent<Bomb>();
                    if (otherBomb.bombId != bombId)
                    {
                        GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                        explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                        explosionObject.GetComponent<DestroySelf>().grid = grid;
                        //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
                        explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

                        if (forceTimeout)
                            explosionObject.GetComponent<DestroySelf>().discrete_timer = Config.EXPLOSION_TIMER_DISCRETE;

                        ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                        grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                        grid.disableObjectOnGrid(StateType.ST_Danger, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                    }
                    else
                    {
                        Debug.Log("Mesma bomba");
                    }
                }

                break;
            }
        }

        //yield return new WaitForSeconds(.05f);
        yield return null;
    }

    private IEnumerator CreateDangers(Vector3 direction, bool forceTimeout)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position /*+ new Vector3(0, .5f, 0)*/, direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                dangerObject.GetComponent<Danger>().grid = grid;
                //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

                if (forceTimeout)
                    dangerObject.GetComponent<Danger>().discrete_timer = Config.BOMB_TIMER_DISCRETE;

                ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
                grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))
                {
                    GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                    dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                    dangerObject.GetComponent<Danger>().grid = grid;
                    //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

                    if (forceTimeout)
                        dangerObject.GetComponent<Danger>().discrete_timer = Config.BOMB_TIMER_DISCRETE;

                    ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
                    grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
                }
                else if (hit.collider.CompareTag("Bomb"))
                {
                    Bomb otherBomb = hit.collider.GetComponentInParent<Bomb>();
                    if (otherBomb.bombId != bombId)
                    {
                        GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                        dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                        dangerObject.GetComponent<Danger>().grid = grid;
                        //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

                        if (forceTimeout)
                            dangerObject.GetComponent<Danger>().discrete_timer = Config.BOMB_TIMER_DISCRETE;

                        ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
                        grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
                    }
                    else
                    {
                        Debug.Log("Mesma bomba");
                    }
                }

                break;
            }
        }

        yield return null;
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
