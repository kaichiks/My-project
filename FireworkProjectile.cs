using UnityEngine;

public class FireworkProjectile : MonoBehaviour
{
    public float flySpeed = 14.0f;
    public float arcHeight = 3.0f;

    public float explosionRadius = 2.5f;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 1.5f;

    public LayerMask racerLayer;
    public GameObject explosionEffectPrefab;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float timer;
    private float duration;
    private bool flying;

    private RunnerNPC owner;

    public void SetOwner(RunnerNPC ownerNpc)
    {
        owner = ownerNpc;
    }

    public void LaunchTo(Vector3 target)
    {
        startPosition = transform.position;
        targetPosition = target;

        float distance = Vector3.Distance(startPosition, targetPosition);
        duration = distance / flySpeed;

        if (duration < 0.2f)
            duration = 0.2f;

        timer = 0.0f;
        flying = true;
    }

    private void Update()
    {
        if (!flying)
            return;

        timer += Time.deltaTime;

        float t = timer / duration;
        t = Mathf.Clamp01(t);

        Vector3 position = Vector3.Lerp(startPosition, targetPosition, t);
        position.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = position;

        if (t >= 1.0f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        flying = false;

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, targetPosition, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(
            targetPosition,
            explosionRadius,
            racerLayer
        );

        foreach (Collider hit in hits)
        {
            RunnerNPC runner = hit.GetComponentInParent<RunnerNPC>();

            if (runner == null)
                continue;

            if (runner == owner)
                continue;

            //if (runner.isPlayer)
            //    continue;

            runner.HitByItem(transform.position);
        }

        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}