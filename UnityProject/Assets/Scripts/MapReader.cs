using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapReader : MonoBehaviour {


    public Transform agentParent; 
    public GameObject square;
    public GameObject agentPrefab;
    public GameObject exitPointPrefab;

    private int[,] walls;
    private int[,] points;
    private int[,] agents;

    private int height = 0;
    private int width = 0;

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

       
        jsonObject.GetField(out height, "height", 0);
        jsonObject.GetField(out width, "width", 0);

        walls = new int[height, width];
        points = new int[height, width];
        agents = new int[height, width];


        FillWalls(jsonObject.GetField("layers").list[0].GetField("data"));
        FillPoints(jsonObject.GetField("layers").list[1].GetField("data"));
        FillAgents(jsonObject.GetField("layers").list[2].GetField("data"));

        CreateEnvironment();
        PopulateAgents();
        CreateExitPoints();
    }

    private void FillWalls(JSONObject wallsData)
    {
        for (int m = 0; m < wallsData.list.Count; m++)
        {
            int i = (int)(m / width);
            int j = m - width * i;
            walls[i, j] = (int)wallsData.list[m].n;         
        }
    }

    private void FillPoints(JSONObject pointsData)
    {
        for (int m = 0; m < pointsData.list.Count; m++)
        {
            int i = (int)(m / width);
            int j = m - width * i;
            points[i, j] = (int)pointsData.list[m].n;       
        }
    }

    private void FillAgents(JSONObject agentsData)
    {
        for (int m = 0; m < agentsData.list.Count; m++)
        {
            int i = (int)(m / width);
            int j = m - width * i;
            agents[i, j] = (int)agentsData.list[m].n;

        }

        Debug.Log(agents.GetLength(0));
    }

    private void CreateEnvironment()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (walls[i, j] == 1)
                {
                    Object obj = GameObject.Instantiate(square, new Vector3(j, -i), Quaternion.identity);
                    ((GameObject)obj).transform.parent = this.transform;
                }
            }
        }
    }

    private void PopulateAgents()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (agents[i, j] == 9)
                {
                    Object obj = GameObject.Instantiate(agentPrefab, new Vector3(j, -i), Quaternion.identity);
                    ((GameObject)obj).transform.parent = agentParent;
                }
            }
        }
    }

    private void CreateExitPoints()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (points[i, j] != 0)
                {
                    Object obj = GameObject.Instantiate(exitPointPrefab, new Vector3(j, -i), Quaternion.identity);
                    ((GameObject)obj).transform.parent = this.transform;
                }
            }
        }
    }

}
