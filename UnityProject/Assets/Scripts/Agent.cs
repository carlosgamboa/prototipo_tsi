using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Agent : MonoBehaviour {

    public GameObject caster;
    public GameObject agentCollider;

    public float agentSpeed = 1f;
    public float castDelay = 0.1f;
    public bool renderCollision = false;

    private Transform _targetPosition;
    private ExitPoint _targetExitPoint;
    private List<ExitPoint> _exitPoints = new List<ExitPoint>();
    private DynamicLight _dynamicCaster;
    private bool _moving = false;

    // Use this for initialization
    void Start() {

        _dynamicCaster = caster.GetComponent<DynamicLight>();
        caster.SetActive(false);
        agentSpeed = Random.Range(agentSpeed - 1.5f, agentSpeed + 0.5f);
    }

    // Update is called once per frame
    void Update() {

        _dynamicCaster.RenderLights = renderCollision;
        if (_moving)
        {
            Vector3 movement = _targetPosition.position - transform.position;

            if (movement.sqrMagnitude < 0.1f)
            {
                _exitPoints.Add(_targetExitPoint);
                _moving = false;
                
                if (_targetExitPoint.exitPointType == ExitPointType.Save)
                {
                    Destroy(this.gameObject);
                }
                else
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
            _dynamicCaster.exitPoints.Clear();

            //if (_targetExitPoint)
           //     _targetExitPoint.DisableCollision();

            agentCollider.SetActive(false);
            caster.SetActive(true);
            StartCoroutine(CastRoutine());
        }
    }

    IEnumerator CastRoutine()
    {
        yield return new WaitForSeconds(castDelay);
        caster.SetActive(false);
        //agentCollider.SetActive(true);
        //if (_targetExitPoint)
        //    _targetExitPoint.EnableCollision();

        SetTargetPosition();
    }

    private void SetTargetPosition()
    {
        ExitPoint choosenPoint = null;
        float distanceToChoosen = 99999f;

        foreach (ExitPoint temp in _exitPoints)
        {
            _dynamicCaster.exitPoints.Remove(temp.gameObject);
        }

        foreach (GameObject exitPoint in _dynamicCaster.exitPoints)
        {
            ExitPoint tempComponent = exitPoint.GetComponent<ExitPoint>();
            float distanceToTemp = (transform.position - tempComponent.transform.position).sqrMagnitude;

            if (choosenPoint != null)
                distanceToChoosen = (transform.position - choosenPoint.transform.position).sqrMagnitude;

            if (tempComponent.exitPointType == ExitPointType.Save)
            {
                if (distanceToChoosen > distanceToTemp)
                {
                    choosenPoint = tempComponent;
                    break;
                }
                
            }
            else if (tempComponent.exitPointType == ExitPointType.Building)
            {
                if (choosenPoint == null)
                {
                    choosenPoint = tempComponent;
                }
                if (choosenPoint.exitPointType == ExitPointType.Room)
                {
                    choosenPoint = tempComponent;
                }
                else if ((int)tempComponent.exitPointPriority > (int)choosenPoint.exitPointPriority)
                {
                    //TODO: CHECK DISTANCE 
                    choosenPoint = tempComponent;
                }
            }
            else
            {
                if (choosenPoint != null && choosenPoint.exitPointType == ExitPointType.Building)
                    continue;

                if (choosenPoint == null)
                {
                    choosenPoint = tempComponent;
                }
                else if ((int)tempComponent.exitPointPriority > (int)choosenPoint.exitPointPriority)
                {
                    //TODO: CHECK DISTANCE 
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

