using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataImpactVFXOnHit : MonoBehaviour
{
    [Header("VFX")]
    [SerializeField] private List<GameObject> immediateVfxPrefabs = new();
    [SerializeField] private List<GameObject> delayedVfxPrefabs = new();
    [SerializeField] private float delayedVfxDelay = 1f;
    [SerializeField] private Vector3 vfxOffset = new Vector3(0, 0.2f, 0);

    [Header("Hit Filter")]
    [SerializeField] private string playerTag = "Player";

    private bool _played;

    private void SpawnVFX(Vector3 pos)
    {
        if (_played) return;
        _played = true;

        // VFX 전용 루트(피냐타가 죽어도 살아있음)
        var root = new GameObject("ImpactVFXRoot");
        root.transform.position = pos + vfxOffset;

        // 즉시 VFX
        for (int i = 0; i < immediateVfxPrefabs.Count; i++)
        {
            var prefab = immediateVfxPrefabs[i];
            if (!prefab) continue;
            Instantiate(prefab, root.transform.position, Quaternion.identity, root.transform);
        }

        // 지연 VFX는 루트에 Runner 붙여서 실행
        var runner = root.AddComponent<VFXDelayRunner>();
        runner.PlayDelayed(delayedVfxPrefabs, delayedVfxDelay, root.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        Vector3 p = other.ClosestPoint(transform.position);
        SpawnVFX(p);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;
        Vector3 p = collision.contactCount > 0
            ? collision.GetContact(0).point
            : collision.collider.ClosestPoint(transform.position);

        SpawnVFX(p);
    }
}

public class VFXDelayRunner : MonoBehaviour
{
    public void PlayDelayed(List<GameObject> prefabs, float delay, Vector3 pos)
    {
        StartCoroutine(Co(prefabs, delay, pos));
    }

    private IEnumerator Co(List<GameObject> prefabs, float delay, Vector3 pos)
    {
        Debug.Log("[VFX] Delayed runner START");
        yield return new WaitForSeconds(delay);
        Debug.Log("[VFX] Delayed runner AFTER WAIT");

        for (int i = 0; i < prefabs.Count; i++)
        {
            var prefab = prefabs[i];
            if (!prefab) continue;
            Instantiate(prefab, pos, Quaternion.identity, transform);
        }
    }
}
