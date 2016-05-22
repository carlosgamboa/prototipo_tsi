using UnityEngine;
using System.Collections;

public class CameraZoomControl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	

        if (Input.GetKeyDown(KeyCode.X))
        {
            transform.Translate(0, 0, -10.0f);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            transform.Translate(0, 0, 10.0f);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.Translate(0, 5.0f, 0);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.Translate(0, -5.0f, 0);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.Translate(-5.0f, 0, 0);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.Translate(5.0f, 0, 0);
        }

    }
}
