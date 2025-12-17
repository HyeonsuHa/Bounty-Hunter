using System.Collections;
using UnityEngine;

public class PlayerRespawnController : MonoBehaviour
{
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 3f;

    [SerializeField] private Transform spawnPoint;

    [Header("Disable On Death")]
    [SerializeField] private Behaviour[] disableScripts;   
    [SerializeField] private Rigidbody rb;                 
    [SerializeField] private Collider[] collidersToDisable;
    [SerializeField] private Renderer[] renderersToHide;

    private Vector3 _spawnPos;
    private Quaternion _spawnRot;
    private bool _respawning;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        CacheSpawnPose();
    }

    public void CacheSpawnPose()
    {
        var t = spawnPoint ? spawnPoint : transform;
        _spawnPos = t.position;
        _spawnRot = t.rotation;
    }

    public void KillAndRespawn()
    {
        if (_respawning) return;
        StartCoroutine(CoRespawn());
    }

    private IEnumerator CoRespawn()
    {
        _respawning = true;

        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }

        foreach (var b in disableScripts)
            if (b) b.enabled = false;

        foreach (var c in collidersToDisable)
            if (c) c.enabled = false;

        foreach (var r in renderersToHide)
            if (r) r.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.SetPositionAndRotation(_spawnPos, _spawnRot);

        foreach (var r in renderersToHide)
            if (r) r.enabled = true;

        foreach (var c in collidersToDisable)
            if (c) c.enabled = true;

        foreach (var b in disableScripts)
            if (b) b.enabled = true;

        if (rb)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _respawning = false;
    }
}
