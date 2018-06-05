using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public GameObject explosionPrefab;
    public GameObject dangerPrefab;
    public LayerMask levelMask;
    public bool exploded = false;
    public int bombId;
    public float timer = 0;

    public Player bomberman;
    public Grid grid;

    private StateType stateType;

    private void Awake()
    {
        grid = GameObject.Find("GridSystem").GetComponent<Grid>();
        stateType = StateType.ST_Bomb;
    }

    // Use this for initialization
    void Start () {
        timer = 0;
        Invoke("Explode", 3f);
	}

    private void FixedUpdate()
    {
        if (!exploded)
            timer = timer + Time.deltaTime;
    }

    public Vector2 GetGridPosition()
    {
        BaseNode n = grid.NodeFromWorldPoint(transform.localPosition);
        return new Vector2(n.gridX, n.gridY);
    }

    public void CreateDangerZone()
    {
        GameObject dangerObject = Instantiate(dangerPrefab, transform.position, Quaternion.identity, transform.parent);
        dangerObject.GetComponent<Danger>().myBomb = this;
        dangerObject.GetComponent<Danger>().grid = grid;

        ServiceLocator.GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
        //comentado porque senão vai sobrescrever a bomba no mapa
        //grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<DestroySelf>().GetGridPosition());

        StartCoroutine(CreateDangers(Vector3.forward));
        StartCoroutine(CreateDangers(Vector3.right));
        StartCoroutine(CreateDangers(Vector3.back));
        StartCoroutine(CreateDangers(Vector3.left));
    }

    void Explode()
    {
        GameObject explosionObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity, transform.parent);
        explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
        explosionObject.GetComponent<DestroySelf>().grid = grid;

        ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
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
        ServiceLocator.GetBombManager().removeBomb(bombId);

        Destroy(gameObject, .3f);
    }

    public void autoDestroy()
    {
        Destroy(gameObject);
    }

    private IEnumerator CreateExplosions(Vector3 direction)
    {
        for(int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                explosionObject.GetComponent<DestroySelf>().grid = grid;

                ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))
                {
                    GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation, transform.parent);
                    explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                    explosionObject.GetComponent<DestroySelf>().grid = grid;

                    ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                    grid.enableObjectOnGrid(StateType.ST_Fire, explosionObject.GetComponent<DestroySelf>().GetGridPosition());
                }

                break;
            }
        }

        yield return new WaitForSeconds(.05f);
    }

    private IEnumerator CreateDangers(Vector3 direction)
    {
        for (int i = 1; i < 3; i++)
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, .5f, 0), direction, out hit, i, levelMask);

            if (!hit.collider)
            {
                GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                dangerObject.GetComponent<Danger>().grid = grid;

                ServiceLocator.GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
                grid.enableObjectOnGrid(StateType.ST_Danger, dangerObject.GetComponent<Danger>().GetGridPosition());
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))
                {
                    GameObject dangerObject = Instantiate(dangerPrefab, transform.position + (i * direction), dangerPrefab.transform.rotation, transform.parent);
                    dangerObject.GetComponent<Danger>().myBomb = gameObject.GetComponent<Bomb>();
                    dangerObject.GetComponent<Danger>().grid = grid;

                    ServiceLocator.GetBombManager().addDanger(dangerObject.GetComponent<Danger>());
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
            CancelInvoke("Explode");
            Explode();
        }
    }
}
