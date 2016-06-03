using UnityEngine;
using System.Collections;


public enum ExitPointType
{
    Room = 0,
    Building = 1
}

public enum ExitPointPriority
{
    Low = 0,
    High = 1
}

public class ExitPoint : MonoBehaviour {

    public ExitPointType exitPointType = 0; //0 room 1 bulding
    public ExitPointPriority exitPointPriority = 0; // 0 low 1 high

    public GameObject roomAsset;
    public GameObject buildingAsset;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetType(int pointId)
    {
        exitPointType = (pointId == 4 || pointId == 5) ? ExitPointType.Room : ExitPointType.Building;
        exitPointPriority = (pointId == 4 || pointId == 6) ? ExitPointPriority.High : ExitPointPriority.Low;

        //Active gameobjects
        roomAsset.SetActive((exitPointType == ExitPointType.Room) ? true: false);
        buildingAsset.SetActive((exitPointType == ExitPointType.Building) ? true : false);

    }
}
