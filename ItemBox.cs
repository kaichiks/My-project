using UnityEngine;

public class ItemBox : MonoBehaviour
{
    public float respawnTime = 3.0f;

    private Collider itemCollider;
    private Renderer[] renderers;
    private bool active = true;

    private void Awake()
    {
        itemCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;

        RunnerNPC npc = other.GetComponent<RunnerNPC>();

        if (npc == null)
            return;

        npc.GiveRandomItem();

        StartCoroutine(Respawn());
    }

    private System.Collections.IEnumerator Respawn()
    {
        active = false;

        if (itemCollider != null)
            itemCollider.enabled = false;

        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        yield return new WaitForSeconds(respawnTime);

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        if (itemCollider != null)
            itemCollider.enabled = true;

        active = true;
    }
}