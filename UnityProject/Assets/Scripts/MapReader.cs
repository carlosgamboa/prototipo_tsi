using UnityEngine;
using System.Collections;
using System.IO;

public class MapReader : MonoBehaviour {

    private int[,] walls;
    private int[,] points;
    private int[,] agents;

	// Use this for initialization
	void Start () {

        ReadFile();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void ReadFile()
    {
        TextAsset fileText = Resources.Load("first_floor") as TextAsset;
        JSONObject jsonObject = new JSONObject(fileText.text);

        int height = 0;
        int width = 0;
        jsonObject.GetField(out height, "height", 0);
        jsonObject.GetField(out width, "width", 0);

        walls = new int[height, width];
        points = new int[height, width];
        agents = new int[height, width];

       
    }

    private void FillWalls(JSONObject wallsData)
    {
        //wallsData.
    }

}
