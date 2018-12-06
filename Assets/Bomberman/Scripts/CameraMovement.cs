using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    float stepX;
    float stepZ;

	// Use this for initialization
	void Start () {
        stepX = 20;
        stepZ = 20;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            if(transform.position.z <= 55)
                transform.position = transform.position + new Vector3(0, 0, stepZ);
        }
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            if (transform.position.z >= 15)
                transform.position = transform.position - new Vector3(0, 0, stepZ);
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            if (transform.position.x >= 20)
                transform.position = transform.position - new Vector3(stepX, 0, 0);
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (transform.position.x <= 166)
                transform.position = transform.position + new Vector3(stepX, 0, 0);
        }
    }
}
