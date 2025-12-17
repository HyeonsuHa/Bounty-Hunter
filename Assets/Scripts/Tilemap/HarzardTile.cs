using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HazardTile : MonoBehaviour
{
    private void Reset()
    {
        // 자동으로 트리거 켜주기
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 루트에서 RespawnController 찾기
        var respawn = other.GetComponentInParent<PlayerRespawnController>();
        if (respawn != null)
        {
            respawn.KillAndRespawn();
        }
    }
}
