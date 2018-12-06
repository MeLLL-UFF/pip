using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public int scenarioId;
    public GameObject explosionPrefab;
    public GameObject dangerPrefab;
    public LayerMask levelMask;
    public bool exploded = false;
    public int bombId;

    //public float timer = 0;
    public int discrete_timer = 0;
    int lastIterationCount = 0;

    public Player bomberman;
    public Grid grid;

    private StateType stateType;

    private void Awake()
    {
        stateType = StateType.ST_Bomb;
    }

    // Use this for initialization
    void Start () {
        //timer = 0;
        discrete_timer = 0;
        //Invoke("Explode", Config.BOMB_TIMER);
	}

    /*private void FixedUpdate()
    {
        if (!exploded)
        {
            timer = timer + Time.deltaTime;
        }
            
    }*/

    public bool iterationUpdate()
    {
        if (!exploded)
        {
            discrete_timer += 1;
            if (discrete_timer >= Config.BOMB_TIMER_DISCRETE)
            {
                return Explode();
            }
        }

        return false;
    }

    public Vector2 GetGridPosition()
    {
        BinaryNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void CreateDangerZone()
    {
        GameObject dangerObject = Instantiate(dangerPrefab, transform.position, Quaternion.identity, transform.parent);
        dangerObject.GetComponent<Danger>().myBomb = this;
        dangerObject.GetComponent<Danger>().grid = grid;
        //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

        ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
        //comentado porque senão vai sobrescrever a bomba no mapa. Código chamado junto com a bomba
        //grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<DestroySelf>().GetGridPosition());

        StartCoroutine(CreateDangers(Vector3.forward));
        StartCoroutine(CreateDangers(Vector3.right));
        StartCoroutine(CreateDangers(Vector3.back));
        StartCoroutine(CreateDangers(Vector3.left));
    }

    bool Explode()
    {
        //Bomb code
        GameObject explosionObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity, transform.parent);
        explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
        explosionObject.GetComponent<DestroySelf>().grid = grid;
        //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
        explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

        ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
        grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());

        StartCoroutine(CreateExplosions(Vector3.forward));
        StartCoroutine(CreateExplosions(Vector3.right));
        StartCoroutine(CreateExplosions(Vector3.back));
        StartCoroutine(CreateExplosions(Vector3.left));

        GetComponent<MeshRenderer>().enabled = false;
        exploded = true;
        transform.Find("Collider").gameObject.SetActive(false);

        if (bomberman != null)
        {
            bomberman.canDropBombs = true;
        }

        grid.disableObjectOnGrid(stateType, GetGridPosition());

        //comentado porque dá erro ao tentar remover bomba dentro de uma iteração de bombas com foreach
        //ServiceLocator.getManager(scenarioId).GetBombManager().removeBomb(bombId);

        Destroy(gameObject, Config.BOMB_TIMER_AFTER_DESTROY);

        return true;
    }

    public void autoDestroy()
    {
        grid.disableObjectOnGrid(stateType, GetGridPosition());
        Destroy(gameObject);
    }

    private IEnumerator CreateExplosions(Vector3 direction)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                explosionObject.GetComponent<DestroySelf>().grid = grid;
                //explosionObject.GetComponent<DestroySelf>().scenarioId = scenarioId;
                explosionObject.GetComponent<DestroySelf>().bomberman = bomberman;

                ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
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

                    ServiceLocator.getManager(scenarioId).GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                    grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                }

                break;
            }
        }

        //yield return new WaitForSeconds(.05f);
        yield return null;
    }

    private IEnumerator CreateDangers(Vector3 direction)
    {
        //Bomb code
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                dangerObject.GetComponent<Danger>().grid = grid;
                //dangerObject.GetComponent<Danger>().scenarioId = scenarioId;

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

                    ServiceLocator.getManager(scenarioId).GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
                    grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
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
            if (Explode())
                ServiceLocator.getManager(scenarioId).GetBombManager().removeBomb(bombId);
        }
    }
}
