﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

    public GameObject explosionPrefab;
    public GameObject explosionPrefabSecondary;
    public LayerMask levelMask;
    public bool exploded = false;
    public int bombId;
    public float timer = 0;

    public GameObject bomberman;

	// Use this for initialization
	void Start () {
        timer = 0;
        Invoke("Explode", 3f);
	}

    private void Update()
    {
        if (!exploded)
            timer = timer + Time.deltaTime;
    }

    public Vector2 GetGridPosition()
    {
        Vector2 myPos = new Vector2(transform.localPosition.x, transform.localPosition.z) - Vector2.one;
        return myPos;
    }

    void Explode()
    {
        GameObject explosionObject = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
        ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());

        StartCoroutine(CreateExplosions(Vector3.forward));
        StartCoroutine(CreateExplosions(Vector3.right));
        StartCoroutine(CreateExplosions(Vector3.back));
        StartCoroutine(CreateExplosions(Vector3.left));

        GetComponent<MeshRenderer>().enabled = false;
        exploded = true;
        transform.Find("Collider").gameObject.SetActive(false);

        if (bomberman != null)
        {
            bomberman.GetComponent<Player>().canDropBombs = true;
        }

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
                GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation);
                explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
            }
            else
            {
                if (hit.collider.CompareTag("Destructable"))
                {
                    GameObject explosionObject = Instantiate(explosionPrefab, transform.position + (i * direction), explosionPrefab.transform.rotation);
                    explosionObject.GetComponent<DestroySelf>().myBomb = gameObject.GetComponent<Bomb>();
                    ServiceLocator.GetBombManager().addExplosion(explosionObject.GetComponent<DestroySelf>());
                }

                break;
            }
        }

        yield return new WaitForSeconds(.05f);
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