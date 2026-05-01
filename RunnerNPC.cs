
//using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
//public class RunnerNPC : MonoBehaviour
//{
//    [Header("References")]
//    public MarathonPath path;

//    [Header("Path Follow")]
//    public int currentPoint = 0;
//    public float pointReachDistance = 0.5f;

//    [Header("Movement")]
//    public float speed = 4.0f;
//    public float rotationSpeed = 8.0f;

//    [Header("Lane Movement")]
//    public int targetLane = 0;          // -1 = left, 0 = center, 1 = right
//    public float laneWidth = 2.0f;
//    public float laneChangeSpeed = 8.0f;

//    private float currentLaneOffset = 0.0f;

//    [Header("Lane Scan")]
//    public int laneScanSteps = 3;
//    public float laneScanSpacing = 1.5f;

//    [Header("Obstacle Detection")]
//    public LayerMask obstacleLayer;
//    public float obstacleCheckDistance = 3.0f;
//    public float obstacleCheckRadius = 1.5f;
//    public float dodgeCooldown = 0.7f;

//    private float dodgeCooldownTimer = 0.0f;

//    private CharacterController controller;

//    void Start()
//    {
//        controller = GetComponent<CharacterController>();

//        if (currentPoint < 0)
//            currentPoint = 0;
//    }

//    void Update()
//    {
//        if (path == null)
//        {
//            Debug.LogError("Path is not assigned.");
//            return;
//        }

//        if (path.PointCount() < 2)
//        {
//            Debug.LogError("Path needs at least 2 points.");
//            return;
//        }

//        if (currentPoint >= path.PointCount() - 1)
//        {
//            // Reached end, stop.
//            return;
//        }

//        if (dodgeCooldownTimer > 0.0f)
//            dodgeCooldownTimer -= Time.deltaTime;

//        CheckObstacleAndDodge();

//        MoveAlongPath();
//    }

//    void CheckObstacleAndDodge()
//    {
//        if (dodgeCooldownTimer > 0.0f)
//            return;

//        Vector3 currentPathPoint = path.GetPoint(currentPoint);
//        Vector3 nextPathPoint = path.GetPoint(currentPoint + 1);

//        Vector3 forwardDir = nextPathPoint - currentPathPoint;
//        forwardDir.y = 0.0f;

//        if (forwardDir.sqrMagnitude < 0.001f)
//            return;

//        forwardDir.Normalize();

//        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

//        int mask = obstacleLayer.value;

//        if (mask == 0)
//        {
//            Debug.LogWarning("Obstacle Layer is empty. Using Everything.");
//            mask = ~0;
//        }

//        // Use the lane scan directly.
//        bool frontBlocked = !IsLaneFree(targetLane, forwardDir, rightDir, mask);

//        if (!frontBlocked)
//        {
//            Debug.Log("Current lane is free");
//            return;
//        }

//        Debug.Log("Obstacle detected in current lane");

//        int newLane = FindBestFreeLane(forwardDir, rightDir, mask);

//        if (newLane == targetLane)
//        {
//            Debug.LogWarning("Obstacle detected, but no free lane.");
//            return;
//        }

//        Debug.Log("Dodge from lane " + targetLane + " to lane " + newLane);

//        targetLane = newLane;
//        dodgeCooldownTimer = dodgeCooldown;
//    }

//    bool IsLaneFree(int lane, Vector3 forwardDir, Vector3 rightDir, int mask)
//    {
//        if (lane < -1 || lane > 1)
//            return false;

//        for (int i = 1; i <= laneScanSteps; i++)
//        {
//            Vector3 checkCenter =
//                transform.position +
//                rightDir * (lane * laneWidth) +
//                forwardDir * (i * laneScanSpacing) +
//                Vector3.up * 0.7f;

//            Collider[] hits = Physics.OverlapSphere(
//                checkCenter,
//                obstacleCheckRadius,
//                mask,
//                QueryTriggerInteraction.Collide
//            );

//            if (hits.Length > 0)
//            {
//                Debug.Log("Lane " + lane + " blocked by " + hits[0].name);
//                return false;
//            }
//        }

//        Debug.Log("Lane " + lane + " is free");
//        return true;
//    }

//    void MoveAlongPath()
//    {
//        Vector3 currentPathPoint = path.GetPoint(currentPoint);
//        Vector3 nextPathPoint = path.GetPoint(currentPoint + 1);

//        Vector3 forwardDir = nextPathPoint - currentPathPoint;
//        forwardDir.y = 0.0f;

//        if (forwardDir.sqrMagnitude < 0.001f)
//            return;

//        forwardDir.Normalize();

//        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

//        float targetLaneOffset = targetLane * laneWidth;

//        float oldLaneOffset = currentLaneOffset;

//        currentLaneOffset = Mathf.MoveTowards(
//            currentLaneOffset,
//            targetLaneOffset,
//            laneChangeSpeed * Time.deltaTime
//        );

//        float laneDelta = currentLaneOffset - oldLaneOffset;

//        Vector3 move =
//            forwardDir * speed * Time.deltaTime +
//            rightDir * laneDelta;

//        controller.Move(move);

//        Quaternion targetRot = Quaternion.LookRotation(forwardDir);
//        transform.rotation = Quaternion.Slerp(
//            transform.rotation,
//            targetRot,
//            rotationSpeed * Time.deltaTime
//        );

//        Vector3 toNext = nextPathPoint - transform.position;
//        toNext.y = 0.0f;

//        if (Vector3.Dot(toNext, forwardDir) <= pointReachDistance)
//        {
//            currentPoint++;

//            if (currentPoint >= path.PointCount() - 1)
//                currentPoint = path.PointCount() - 1;
//        }
//    }
//    int FindBestFreeLane(Vector3 forwardDir, Vector3 rightDir, int mask)
//    {
//        bool leftFree = IsLaneFree(-1, forwardDir, rightDir, mask);
//        bool centerFree = IsLaneFree(0, forwardDir, rightDir, mask);
//        bool rightFree = IsLaneFree(1, forwardDir, rightDir, mask);

//        Debug.Log(
//            "Free lanes: Left=" + leftFree +
//            " Center=" + centerFree +
//            " Right=" + rightFree
//        );

//        if (targetLane == -1)
//        {
//            if (centerFree) return 0;
//            if (rightFree) return 1;
//        }
//        else if (targetLane == 0)
//        {
//            if (leftFree && rightFree)
//                return Random.value < 0.5f ? -1 : 1;

//            if (leftFree) return -1;
//            if (rightFree) return 1;
//        }
//        else if (targetLane == 1)
//        {
//            if (centerFree) return 0;
//            if (leftFree) return -1;
//        }

//        return targetLane;
//    }

//    void OnDrawGizmos()
//    {
//        if (path == null || path.PointCount() < 2)
//            return;

//        int safePoint = Mathf.Clamp(currentPoint, 0, path.PointCount() - 2);

//        Vector3 currentPathPoint = path.GetPoint(safePoint);
//        Vector3 nextPathPoint = path.GetPoint(safePoint + 1);

//        Vector3 forwardDir = nextPathPoint - currentPathPoint;
//        forwardDir.y = 0.0f;

//        if (forwardDir.sqrMagnitude < 0.001f)
//            return;

//        forwardDir.Normalize();

//        Vector3 rightDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

//        DrawLaneScan(-1, rightDir, forwardDir, Color.green);
//        DrawLaneScan(0, rightDir, forwardDir, Color.yellow);
//        DrawLaneScan(1, rightDir, forwardDir, Color.green);
//    }

//    void DrawLaneScan(int lane, Vector3 rightDir, Vector3 forwardDir, Color color)
//    {
//        Gizmos.color = color;

//        for (int i = 1; i <= laneScanSteps; i++)
//        {
//            Vector3 center =
//                transform.position +
//                rightDir * (lane * laneWidth) +
//                forwardDir * (i * laneScanSpacing) +
//                Vector3.up * 0.7f;

//            Gizmos.DrawWireSphere(center, obstacleCheckRadius);
//        }
//    }
//    void DrawLaneSphere(
//        int lane,
//        Vector3 rightDir,
//        Vector3 forwardDir,
//        Color color
//    )
//    {
//        Gizmos.color = color;

//        Vector3 center =
//            transform.position +
//            rightDir * (lane * laneWidth) +
//            forwardDir * obstacleCheckDistance +
//            Vector3.up * 0.7f;

//        Gizmos.DrawWireSphere(center, obstacleCheckRadius);
//    }

//    //void DrawLaneBox(
//    //    int lane,
//    //    Vector3 rightDir,
//    //    Vector3 forwardDir,
//    //    Quaternion rotation,
//    //    Vector3 halfExtents,
//    //    Color color
//    //)
//    //{
//    //    Gizmos.color = color;

//    //    Vector3 center =
//    //        transform.position +
//    //        rightDir * (lane * laneWidth) +
//    //        forwardDir * (obstacleCheckDistance * 0.5f) +
//    //        Vector3.up * 0.7f;

//    //    Matrix4x4 oldMatrix = Gizmos.matrix;
//    //    Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
//    //    Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2.0f);
//    //    Gizmos.matrix = oldMatrix;
//    //}
//}

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
    public float obstacleCheckDistance = 3.0f;
    public float obstacleCheckRadius = 0.45f;
    public float sideDodgeDistance = 1.8f;
    public float forwardDodgeDistance = 4.0f;
    public float dodgeFinishDistance = 0.8f;
    private float obstacleIgnoreTimer = 0.0f;
    public float obstacleIgnoreAfterDodge = 1.0f;

    private bool isDodging;
    private Vector3 dodgeTarget;

    [Header("Lane Offset")]
    public float laneOffset = 1.0f;
    public bool randomLaneOnStart = true;

    [Header("Avoid Other Racers")]
    public LayerMask racerLayer;
    public float avoidCheckDistance = 3.0f;
    public float avoidRadius = 0.7f;
    public float avoidStrength = 1.5f;

    [Header("Animation")]
    public Animator animator;
    public string speedParameter = "Speed";

    private NavMeshAgent agent;
    private int currentCheckpoint;
    private float currentSpeed;
    private float myLaneOffset;

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

        if (isDodging)
        {
            UpdateDodge();
        }
        else
        {
            CheckCheckpointReached();

            if (obstacleIgnoreTimer <= 0.0f)
            {
                CheckObstacleAndStartDodge();
            }

            AvoidOtherRacers();
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
            if (loopRace)
            {
                currentCheckpoint = 0;
            }
            else
            {
                agent.ResetPath();
                return;
            }
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

    private Vector3 GetOffsetCheckpointPosition()
    {
        Transform checkpoint = checkpoints[currentCheckpoint];

        Vector3 targetPosition = checkpoint.position;

        // use checkpoint's right direction for lane offset
        Vector3 right = checkpoint.right;
        targetPosition += right * myLaneOffset;

        // make sure offset position is on NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return checkpoint.position;
    }

    private void UpdateSpeed()
    {
        float distanceToCheckpoint = Vector3.Distance(
            transform.position,
            checkpoints[currentCheckpoint].position
        );

        float targetSpeed = maxSpeed;

        // Slow down when near checkpoint/corner
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

        Vector3 avoidDirection = Vector3.zero;

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject)
                continue;

            Vector3 away = transform.position - hit.transform.position;
            away.y = 0f;

            if (away.sqrMagnitude > 0.001f)
            {
                avoidDirection += away.normalized;
            }
        }

        if (avoidDirection == Vector3.zero)
            return;

        Vector3 newTarget = agent.destination + avoidDirection.normalized * avoidStrength;

        if (NavMesh.SamplePosition(newTarget, out NavMeshHit navHit, 2.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(navHit.position);
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
            1.5f,
            NavMesh.AllAreas
        );

        bool rightOk = NavMesh.SamplePosition(
            rightTarget,
            out NavMeshHit rightHit,
            1.5f,
            NavMesh.AllAreas
        );

        if (!leftOk && !rightOk)
        {
            return;
        }

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

        isDodging = true;
        agent.SetDestination(dodgeTarget);
    }

    private void UpdateDodge()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= dodgeFinishDistance)
        {
            isDodging = false;

            obstacleIgnoreTimer = obstacleIgnoreAfterDodge;

            agent.isStopped = false;

            MoveToCurrentCheckpoint();
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
        Gizmos.DrawWireSphere(origin + transform.forward * avoidCheckDistance, avoidRadius);

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