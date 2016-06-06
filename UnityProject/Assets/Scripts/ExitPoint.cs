using UnityEngine;
using System.Collections;


public enum ExitPointType
{
    Room = 0,
    Building = 1,
    Save = 2
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
    public GameObject savePointAsset;
    public GameObject objectCollider;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetType(int pointId)
    {
        //Debug.Log("exit point id" + pointId);
        if (pointId == 12)
        {
            exitPointType = ExitPointType.Save;
            exitPointPriority = ExitPointPriority.High;

            savePointAsset.SetActive(true);
            roomAsset.SetActive(false);
            buildingAsset.SetActive(false);
        }
        else
        {
            exitPointType = (pointId == 5 || pointId == 6) ? ExitPointType.Room : ExitPointType.Building;
            exitPointPriority = (pointId == 5 || pointId == 7) ? ExitPointPriority.High : ExitPointPriority.Low;

            //Active gameobjects
            savePointAsset.SetActive(false);
            roomAsset.SetActive((exitPointType == ExitPointType.Room) ? true : false);
            buildingAsset.SetActive((exitPointType == ExitPointType.Building) ? true : false);
        }
    }

    public void DisableCollision()
    {
        objectCollider.layer = LayerMask.NameToLayer("Default");
    }

    public void EnableCollision()
    {
        objectCollider.layer = LayerMask.NameToLayer("Wall");
    }
}
