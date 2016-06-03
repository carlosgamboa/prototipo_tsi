using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

    public GameObject caster;
    public GameObject agentCollider;

    public float agentSpeed = 1f;
    public float castDelay = 0.1f;


    private Transform _targetPosition;
    private ExitPoint _targetExitPoint;
    private List<ExitPoint> _exitPoints = new List<ExitPoint>();
    private DynamicLight _dynamicCaster;
    private bool _moving = false;

    // Use this for initialization
    void Start() {

        _dynamicCaster = caster.GetComponent<DynamicLight>();
        caster.SetActive(false);
    }

    // Update is called once per frame
    void Update() {

        if (_moving)
        {
            Vector3 movement = _targetPosition.position - transform.position;

            if (movement.sqrMagnitude < 0.1f)
            {
                _exitPoints.Add(_targetExitPoint);
                _moving = false;
                Cast();
            }
            else
            {
                float step = agentSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition.position, step);
            }           
        }
    }

    public void Cast()
    {
        if (!_moving)
        {
            agentCollider.SetActive(false);
            caster.SetActive(true);
            StartCoroutine(CastRoutine());
        }
    }

    IEnumerator CastRoutine()
    {
        yield return new WaitForSeconds(castDelay);
        caster.SetActive(false);
        agentCollider.SetActive(true);

        SetTargetPosition();
    }

    private void SetTargetPosition()
    {
        ExitPoint choosenPoint = null;

        foreach (ExitPoint temp in _exitPoints)
        {
            _dynamicCaster.exitPoints.Remove(temp.gameObject);
        }

        foreach (GameObject exitPoint in _dynamicCaster.exitPoints)
        {
            ExitPoint tempComponent = exitPoint.GetComponent<ExitPoint>();
            if (tempComponent.exitPointType == ExitPointType.Building)
            {
                if (choosenPoint == null)
                {
                    choosenPoint = tempComponent;
                }
                else if ((int)tempComponent.exitPointPriority > (int)choosenPoint.exitPointPriority)
                {
                    choosenPoint = tempComponent;
                }
            }
            else
            {
                if (tempComponent.exitPointType == ExitPointType.Building)
                    continue;

                if (choosenPoint == null)
                {
                    choosenPoint = tempComponent;
                }
                else if ((int)tempComponent.exitPointPriority > (int)choosenPoint.exitPointPriority)
                {
                    choosenPoint = tempComponent;
                }
            }
            //Debug.Log("Agent: " + gameObject.name + " ExitTo: " + exitPoint.name + " in " + exitPoint.transform.position.ToString());
        }

        if (choosenPoint != null)
        {
            _targetExitPoint = choosenPoint;
            _targetPosition = choosenPoint.transform;
            _moving = true;
        }
    }
}

