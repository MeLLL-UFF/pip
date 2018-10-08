using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    float step;

	// Use this for initialization
	void Start () {
        step = 15;

    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            transform.position = transform.position + new Vector3(0, 0, step);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            transform.position = transform.position - new Vector3(0, 0, step);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            transform.position = transform.position - new Vector3(step, 0, 0);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            transform.position = transform.position + new Vector3(step, 0, 0);
        }
    }
}
