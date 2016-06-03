using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {


    public MapReader mapReader;
    [Tooltip("value in seconds to update the main loop and call all agents to cast")]
    public float updateTick;


    private bool running = false;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Init()
    {
        mapReader.CreateMap();
    }

    public void StartSimulation()
    {
        Debug.Log("starting simulation");
        running = true;
        StartCoroutine(UpdateCasting());        
    }

    //Main loop routine
    IEnumerator UpdateCasting()
    {
        while(running)
        {
            for (int i = 0; i < mapReader.Agents.Count; i++)
            {
                mapReader.Agents[i].Cast();
            }
            yield return new WaitForSeconds(updateTick);
        }       
    }
}   
    