using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 20f;
    public float fieldOfView = 90f;
    public float attackRange = 8f;
    public float attackDamage = 10f;
    public float attackCooldown = 3f;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    private float lastAttackTime;
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;

        GameObject[] points = GameObject.FindGameObjectsWithTag("PatrolPoint");
        patrolPoints = new Transform[points.Length];
        for (int i = 0; i < points.Length; i++)
            patrolPoints[i] = points[i].transform;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
    }

    

    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle < fieldOfView / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                    return true;
            }
        }
        return false;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < attackRange)
            currentState = State.Attack;
        else if (dist < detectionRange && CanSeePlayer())
            currentState = State.Chase;
        else
            currentState = State.Patrol;

        switch (currentState)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }

        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            isWaiting = true;
            waitTimer = patrolWaitTime;
        }
    }

    void Chase()
    {
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        agent.ResetPath();

        Vector3 dir = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dir);

        if (Time.time - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Time.time;

            Ray ray = new Ray(transform.position + Vector3.up * 1.5f, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    PlayerState playerState = player.GetComponent<PlayerState>();
                    if (playerState != null)
                        playerState.TakeDamage((int)attackDamage);
                }
            }
        }
    }
}