
using UnityEngine;
using UnityEngine.AI;

public class RunnerNPC : MonoBehaviour
{

    [Header("Race Path")]
    public Transform[] checkpoints;
    public bool loopRace = true;

    [Header("Movement")]
    public float baseSpeed = 7.0f;
    public float maxSpeed = 10.0f;
    public float acceleration = 18.0f;
    public float turnSlowDownDistance = 6.0f;
    public float checkpointReachDistance = 2.0f;

    [Header("Obstacle Dodge")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 2.0f;
    public float obstacleCheckRadius = 0.35f;
    public float sideDodgeDistance = 0.9f;
    public float forwardDodgeDistance = 2.0f;
    public float dodgeFinishDistance = 0.5f;
    public float maxDodgeTime = 0.6f;
    public float obstacleIgnoreAfterDodge = 0.25f;

    private float obstacleIgnoreTimer = 0.0f;

    [Header("Lane Offset")]
    public float laneOffset = 0.6f;
    public bool randomLaneOnStart = true;

    [Header("Avoid Other Racers")]
    public LayerMask racerLayer;
    public float avoidCheckDistance = 1.8f;
    public float avoidRadius = 0.7f;
    public float avoidStrength = 0.8f;

    [Header("Follow Racer After Obstacle")]
    public float freshlyAvoidedObstacleTime = 1.0f;
    public float followRacerTime = 0.8f;
    public float followBehindDistance = 1.4f;
    public float followSideOffset = 0.25f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";

    private NavMeshAgent agent;
    private int currentCheckpoint;
    private float currentSpeed;
    private float myLaneOffset;

    private enum RaceState
    {
        FollowPath,
        DodgeObstacle,
        FollowRacer
    }

    private RaceState state = RaceState.FollowPath;

    private Vector3 dodgeTarget;
    private float dodgeTimer;

    private Transform targetRacer;
    private float followRacerTimer;

    private float freshlyAvoidedObstacleTimer;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        agent.updateRotation = true;
        agent.autoBraking = false;
        agent.acceleration = acceleration;
        agent.speed = baseSpeed;
    }

    private void Start()
    {
        currentSpeed = baseSpeed;

        if (randomLaneOnStart)
        {
            myLaneOffset = Random.Range(-laneOffset, laneOffset);
        }
        else
        {
            myLaneOffset = laneOffset;
        }

        MoveToCurrentCheckpoint();
    }

    private void Update()
    {
        if (!agent.isOnNavMesh)
            return;

        if (checkpoints == null || checkpoints.Length == 0)
            return;

        if (obstacleIgnoreTimer > 0.0f)
            obstacleIgnoreTimer -= Time.deltaTime;

        switch (state)
        {
            case RaceState.FollowPath:
                CheckCheckpointReached();

                if (obstacleIgnoreTimer <= 0.0f)
                {
                    CheckObstacleAndStartDodge();
                    if (state == RaceState.FollowPath && freshlyAvoidedObstacleTimer > 0.0f)
                    {
                        CheckRacerAndStartFollow();
                    }
                    if (state == RaceState.FollowPath)
                    {
                        AvoidOtherRacers();
                    }
                }
                break;

            case RaceState.DodgeObstacle:
                UpdateDodge();
                break;

            case RaceState.FollowRacer:
                UpdateFollowRacer();
                break;
        }

        UpdateSpeed();
        UpdateAnimation();
    }

    private void CheckCheckpointReached()
    {
        if (agent.pathPending)
            return;

        if (!agent.hasPath)
            return;

        if (agent.remainingDistance > checkpointReachDistance)
            return;

        currentCheckpoint++;

        if (currentCheckpoint >= checkpoints.Length)
        {
            agent.isStopped = true;
            agent.ResetPath();
            currentSpeed = 0.0f;
            agent.speed = 0.0f;
            return;
        }

        MoveToCurrentCheckpoint();
    }

    private void MoveToCurrentCheckpoint()
    {
        if (checkpoints == null || checkpoints.Length == 0)
            return;

        if (currentCheckpoint < 0 || currentCheckpoint >= checkpoints.Length)
            return;

        if (checkpoints[currentCheckpoint] == null)
        {
            Debug.LogError("Checkpoint " + currentCheckpoint + " is missing.");
            return;
        }

        Vector3 targetPosition = GetOffsetCheckpointPosition();

        agent.isStopped = false;
        agent.ResetPath();

        bool success = agent.SetDestination(targetPosition);

        if (!success)
        {
            Debug.LogWarning("Failed to set destination to checkpoint " + currentCheckpoint);
        }
    }

    private bool IsTargetInside90Degrees(Vector3 target)
    {
        Vector3 toTarget = target - transform.position;
        toTarget.y = 0.0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return true;

        toTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, toTarget);

        // dot >= 0 ‚¾‚Á‚½‚çtarget‚Í‚P‚W‚O“xˆÈ“à
        // npc‚ª¶‰E90“xÅ‘å‚µ‚©‚µ‹È‚ª‚ê‚È‚¢
        return dot >= 0.0f;
    }

    private Vector3 GetOffsetCheckpointPosition()
    {
        Transform checkpoint = checkpoints[currentCheckpoint];

        Vector3 targetPosition = checkpoint.position;

        Vector3 right = checkpoint.right;
        targetPosition += right * myLaneOffset;

        // make sure offset position is on NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return checkpoint.position;
    }

    private void CheckRacerAndStartFollow()
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            avoidRadius * 1.5f,
            racerLayer
        );

        if (hits.Length == 0)
            return;

        Transform bestRacer = null;
        float bestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 toOther = hit.transform.position - transform.position;
            toOther.y = 0.0f;

            if (toOther.sqrMagnitude < 0.001f)
                continue;

            float frontDot = Vector3.Dot(transform.forward, toOther.normalized);

            // allow racer in front or beside
            if (frontDot < -0.2f)
                continue;

            float distance = toOther.magnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestRacer = hit.transform;
            }
        }

        if (bestRacer == null)
            return;

        targetRacer = bestRacer;
        followRacerTimer = followRacerTime;
        state = RaceState.FollowRacer;
    }

    private void UpdateFollowRacer()
    {
        followRacerTimer -= Time.deltaTime;

        if (targetRacer == null)
        {
            ReturnToPath();
            return;
        }

        Vector3 followTarget =
            targetRacer.position
            - targetRacer.forward * followBehindDistance
            + targetRacer.right * followSideOffset;

        if (NavMesh.SamplePosition(followTarget, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            if (IsTargetInside90Degrees(hit.position))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                ReturnToPath();
            }
        }

        if (followRacerTimer <= 0.0f)
        {
            ReturnToPath();
        }
    }

    private void CheckObstacleAndStartDodge()
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f;
        Vector3 forward = transform.forward;

        bool blocked = Physics.SphereCast(
            origin,
            obstacleCheckRadius,
            forward,
            out RaycastHit hit,
            obstacleCheckDistance,
            obstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        if (!blocked)
            return;

        Vector3 leftTarget =
            transform.position
            - transform.right * sideDodgeDistance
            + forward * forwardDodgeDistance;

        Vector3 rightTarget =
            transform.position
            + transform.right * sideDodgeDistance
            + forward * forwardDodgeDistance;

        bool leftOk = NavMesh.SamplePosition(
            leftTarget,
            out NavMeshHit leftHit,
            1.0f,
            NavMesh.AllAreas
        );

        bool rightOk = NavMesh.SamplePosition(
            rightTarget,
            out NavMeshHit rightHit,
            1.0f,
            NavMesh.AllAreas
        );

        if (!leftOk && !rightOk)
            return;

        if (leftOk && rightOk)
        {
            float leftDistance = Vector3.Distance(leftHit.position, checkpoints[currentCheckpoint].position);
            float rightDistance = Vector3.Distance(rightHit.position, checkpoints[currentCheckpoint].position);

            dodgeTarget = leftDistance < rightDistance ? leftHit.position : rightHit.position;
        }
        else
        {
            dodgeTarget = leftOk ? leftHit.position : rightHit.position;
        }

        StartDodge(dodgeTarget);
    }

    private void ReturnToPath()
    {
        targetRacer = null;

        state = RaceState.FollowPath;

        obstacleIgnoreTimer = obstacleIgnoreAfterDodge;

        agent.isStopped = false;

        MoveToCurrentCheckpoint();
    }

    private void UpdateSpeed()
    {
        float distanceToCheckpoint = Vector3.Distance(
            transform.position,
            checkpoints[currentCheckpoint].position
        );

        float targetSpeed = maxSpeed;

        if (distanceToCheckpoint < turnSlowDownDistance)
        {
            targetSpeed = baseSpeed;
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            acceleration * Time.deltaTime
        );

        agent.speed = currentSpeed;
    }

    private void AvoidOtherRacers()
    {
        Vector3 origin = transform.position + Vector3.up * 0.7f;
        Vector3 forward = transform.forward;

        Collider[] hits = Physics.OverlapSphere(
            origin + forward * avoidCheckDistance,
            avoidRadius,
            racerLayer
        );

        if (hits.Length == 0)
            return;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 toOther = hit.transform.position - transform.position;
            toOther.y = 0.0f;

            if (toOther.sqrMagnitude < 0.001f)
                continue;

            // only avoid racers in front
            if (Vector3.Dot(transform.forward, toOther.normalized) < 0.3f)
                continue;

            Vector3 leftTarget =
                transform.position
                - transform.right * sideDodgeDistance
                + transform.forward * forwardDodgeDistance;

            Vector3 rightTarget =
                transform.position
                + transform.right * sideDodgeDistance
                + transform.forward * forwardDodgeDistance;

            bool leftOk = NavMesh.SamplePosition(
                leftTarget,
                out NavMeshHit leftHit,
                1.0f,
                NavMesh.AllAreas
            );

            bool rightOk = NavMesh.SamplePosition(
                rightTarget,
                out NavMeshHit rightHit,
                1.0f,
                NavMesh.AllAreas
            );

            if (!leftOk && !rightOk)
                return;

            if (leftOk && rightOk)
            {
                float leftDistance = Vector3.Distance(leftHit.position, checkpoints[currentCheckpoint].position);
                float rightDistance = Vector3.Distance(rightHit.position, checkpoints[currentCheckpoint].position);

                dodgeTarget = leftDistance < rightDistance ? leftHit.position : rightHit.position;
            }
            else
            {
                dodgeTarget = leftOk ? leftHit.position : rightHit.position;
            }

            StartDodge(dodgeTarget);
            return;
        }
    }

    private void StartDodge(Vector3 target)
    {
        if (!IsTargetInside90Degrees(target))
        {
            ReturnToPath();
            return;
        }

        dodgeTarget = target;
        dodgeTimer = maxDodgeTime;

        state = RaceState.DodgeObstacle;

        agent.isStopped = false;
        agent.SetDestination(dodgeTarget);
    }


    private void UpdateDodge()
    {
        if (agent.pathPending)
            return;

        dodgeTimer -= Time.deltaTime;

        if (agent.remainingDistance <= dodgeFinishDistance)
        {
            //isDodging = false;

            //obstacleIgnoreTimer = obstacleIgnoreAfterDodge;

            //agent.isStopped = false;

            //MoveToCurrentCheckpoint();

            freshlyAvoidedObstacleTimer = freshlyAvoidedObstacleTime;

            ReturnToPath();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        animator.SetFloat(speedParameter, agent.velocity.magnitude);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 0.7f;

        Gizmos.DrawWireSphere(
            origin + transform.forward * avoidCheckDistance,
            avoidRadius
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            origin + transform.forward * obstacleCheckDistance,
            obstacleCheckRadius
        );

        if (checkpoints != null)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i] != null)
                {
                    Gizmos.DrawSphere(checkpoints[i].position, 0.3f);
                }
            }
        }
    }
}