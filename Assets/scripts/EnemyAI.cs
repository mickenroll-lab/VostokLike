using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 20f;
    public float alertDetectionRange = 60f;
    public float fieldOfView = 90f;
    public float attackRange = 8f;
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    private float lastAttackTime;
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private enum State { Patrol, Alert, Search, Chase, Attack }
    private State currentState = State.Patrol;

    // 状態タイマー
    private float searchTimer = 0f;
    private float alertTimer = 0f;
    private const float searchDuration = 20f;
    private const float alertDuration = 20f;

    // ロストポイント
    private Vector3 lostPoint;

    // DetectionRange の補間用
    private float currentDetectionRange;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        currentDetectionRange = detectionRange;

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
        float dist = Vector3.Distance(transform.position, player.position);
        if (angle < fieldOfView / 2f && dist < currentDetectionRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, dir, out hit, currentDetectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                    return true;
            }
        }
        return false;
    }

    void Update()
    {
        PlayerState playerState = player.GetComponent<PlayerState>();
        if (playerState != null && playerState.deathPanel.activeSelf) return;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                currentDetectionRange = Mathf.MoveTowards(currentDetectionRange, detectionRange, Time.deltaTime * 5f);
                if (CanSeePlayer())
                    TransitionTo(State.Chase);
                else
                    Patrol();
                break;

            case State.Alert:
                currentDetectionRange = Mathf.MoveTowards(currentDetectionRange, alertDetectionRange, Time.deltaTime * 10f);
                alertTimer -= Time.deltaTime;
                if (CanSeePlayer())
                    TransitionTo(State.Chase);
                else if (alertTimer <= 0f)
                    TransitionTo(State.Patrol);
                else
                    Patrol(); // Alert中も巡回継続
                break;

            case State.Search:
                currentDetectionRange = alertDetectionRange;
                searchTimer -= Time.deltaTime;
                if (CanSeePlayer())
                    TransitionTo(State.Chase);
                else if (searchTimer <= 0f)
                    TransitionTo(State.Alert);
                else
                    Search();
                break;

            case State.Chase:
                currentDetectionRange = alertDetectionRange;
                if (dist < attackRange)
                    TransitionTo(State.Attack);
                else if (!CanSeePlayer())
                {
                    lostPoint = player.position;
                    TransitionTo(State.Search);
                }
                else
                    Chase();
                break;

            case State.Attack:
                if (!CanSeePlayer())
                {
                    lostPoint = player.position;
                    TransitionTo(State.Search);
                }
                else if (dist > attackRange)
                    TransitionTo(State.Chase);
                else
                    Attack();
                break;
        }
    }

    void TransitionTo(State newState)
    {
        Debug.Log($"[EnemyAI] {currentState} → {newState}");
        currentState = newState;
        switch (newState)
        {
            case State.Search:
                searchTimer = searchDuration;
                agent.SetDestination(lostPoint);
                break;
            case State.Alert:
                alertTimer = alertDuration;
                break;
            case State.Patrol:
                if (patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                break;
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

    void Search()
    {
        // ロストポイント付近をランダムに索敵
        if (agent.remainingDistance < 0.5f && !agent.pathPending)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-10f, 10f), 0,
                Random.Range(-10f, 10f));
            agent.SetDestination(lostPoint + randomOffset);
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
            if (Physics.Raycast(ray, out hit, 50f))
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

    public void OnDamaged()
    {
        lostPoint = player.position;
        TransitionTo(State.Alert);
        alertTimer = alertDuration;
        currentDetectionRange = alertDetectionRange;
    }
}