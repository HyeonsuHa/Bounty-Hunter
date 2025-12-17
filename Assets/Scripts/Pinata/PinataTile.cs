using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PinataTile : MonoBehaviour
{
    [Header("Collect")]
    [SerializeField] private float vanishDelay = 0f;   // 닿고 바로 사라지면 0
    [SerializeField] private float destroyDelay = 0f;  // 필요하면 약간 뒤에 Destroy
    [SerializeField] private bool useTriggerOnly = true;

    [Header("Visuals to hide (optional)")]
    [SerializeField] private Renderer[] renderersToHide;
    [SerializeField] private Collider[] collidersToDisable;

    private bool _collected;

    private void Reset()
    {
        // 안전하게 Trigger로 쓰기 권장
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        if (PinataGoalManager.Instance != null)
            PinataGoalManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        // 비활성/파괴 시점에 매니저 정리
        if (PinataGoalManager.Instance != null)
            PinataGoalManager.Instance.Unregister(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!useTriggerOnly) return;
        TryCollect(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (useTriggerOnly) return;
        TryCollect(collision.collider);
    }

    private void TryCollect(Collider other)
    {
        if (_collected) return;

        // 플레이어 판정: Tag로 하거나, Player 컴포넌트로 하거나 택1
        if (!other.CompareTag("Player"))
            return;

        _collected = true;
        StartCoroutine(CoCollect());
    }

    private IEnumerator CoCollect()
    {
        if (vanishDelay > 0f)
            yield return new WaitForSeconds(vanishDelay);

        // 1) 즉시 안 보이게 / 충돌 끄기
        if (renderersToHide != null && renderersToHide.Length > 0)
        {
            foreach (var r in renderersToHide) if (r) r.enabled = false;
        }
        else
        {
            // 지정 안 했으면 자기 하위 렌더러 전부 끄기(편의)
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }

        if (collidersToDisable != null && collidersToDisable.Length > 0)
        {
            foreach (var c in collidersToDisable) if (c) c.enabled = false;
        }
        else
        {
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        // 2) 필요하면 조금 뒤에 Destroy (타일맵/그리드에서 직접 제거한다면 destroyDelay=0 추천)
        if (destroyDelay > 0f)
            yield return new WaitForSeconds(destroyDelay);

        // 실제 오브젝트를 Destroy하면 OnDisable → Unregister가 호출되면서 카운트 줄어듦
        Destroy(gameObject);
    }
}
