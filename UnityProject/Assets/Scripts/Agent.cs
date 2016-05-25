using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

    public GameObject caster;
    public GameObject agentCollider;

    private List<Transform> _exitPoints;
    private DynamicLight _dynamicCaster;

    // Use this for initialization
    void Start() {

        _dynamicCaster = caster.GetComponent<DynamicLight>();
        caster.SetActive(false);
    }

    // Update is called once per frame
    void Update() {

    }

    public void Cast()
    {
        agentCollider.SetActive(false);
        caster.SetActive(true);
        StartCoroutine(CastRoutine());
    }

    IEnumerator CastRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        caster.SetActive(false);
        agentCollider.SetActive(true);

        foreach(GameObject exitPoint in _dynamicCaster.exitPoints)
        {
            Debug.Log("Agent: "+gameObject.name+" ExitTo: " + exitPoint.name + " in " + exitPoint.transform.position.ToString());
        }
      
    }
}

