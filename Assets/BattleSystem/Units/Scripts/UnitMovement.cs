using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private float baseSpeed = 3.5f;
    private float targetSpeed = 3.5f;
    private float snapDistance = 0.35f;
    private bool hasTarget = false;

    public float MoveSpeed => baseSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        var stats = GetComponent<UnitStats>();
        if (stats != null) baseSpeed = stats.moveSpeed;

        agent.speed = baseSpeed;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.autoBraking = true;
    }

    void Update()
    {
        if (!hasTarget || agent == null || !agent.isActiveAndEnabled) return;

        agent.speed = Mathf.Lerp(agent.speed, targetSpeed, Time.deltaTime * 6f);

        if (!agent.pathPending && agent.remainingDistance <= snapDistance)
        {
            agent.isStopped = true;
            hasTarget = false;
        }
        else
        {
            agent.isStopped = false;
        }
    }

    public void SetMovementTarget(Vector3 destination, float speed)
    {
        if (agent == null || !agent.isActiveAndEnabled) return;

        targetSpeed = Mathf.Max(0.1f, speed);
        agent.speed = targetSpeed;
        agent.isStopped = false;
        agent.SetDestination(destination);
        hasTarget = true;
    }

    public void SetDestination(Vector3 destination)
    {
        SetMovementTarget(destination, baseSpeed);
    }

    public void StopImmediate()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            hasTarget = false;
        }
    }
}