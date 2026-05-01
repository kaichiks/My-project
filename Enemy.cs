using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 2f;

    private Transform player;
    private CharacterController characterController;
    private float nextFireTime;
    private float verticalVelocity;
    private bool hasFired = false;
    private const float GRAVITY = -20f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        HandleGravity();
        HandleMovement();

        if (player != null)
            HandleShooting();
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += GRAVITY * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector3 movement = transform.forward * moveSpeed * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(movement);
    }

    private void HandleShooting()
    {
        Vector3 toPlayer = player.position - transform.position;
        float dot = Vector3.Dot(transform.forward, toPlayer.normalized);

        bool playerIsBehind = dot < 0f;

        Debug.Log($"Dot: {dot:F2} | PlayerIsBehind: {playerIsBehind} | HasFired: {hasFired}");

        if (playerIsBehind && !hasFired && Time.time >= nextFireTime)
        {
            Shoot();
            hasFired = true;
            nextFireTime = Time.time + fireRate;
        }

        if (!playerIsBehind)
            hasFired = false;
    }

    private void Shoot()
    {
        Debug.Log("Enemy shooting!");
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Projectile>().Init(true);
    }
}
