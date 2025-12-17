using UnityEngine;

public class VFXAutoDestroy : MonoBehaviour
{
    [SerializeField] private float extraLife = 0.2f; // 마지막 파티클 다 끝난 후 여유 시간
    private ParticleSystem[] _ps;

    private void Awake()
    {
        _ps = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void OnEnable()
    {
        PlayAll();
        Invoke(nameof(DestroySelf), GetMaxDuration() + extraLife);
    }

    public void PlayAll()
    {
        if (_ps == null || _ps.Length == 0)
            _ps = GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < _ps.Length; i++)
        {
            var p = _ps[i];
            if (!p) continue;
            p.Clear(true);
            p.Play(true);
        }
    }

    float GetMaxDuration()
    {
        float maxT = 0f;
        for (int i = 0; i < _ps.Length; i++)
        {
            var p = _ps[i];
            if (!p) continue;

            var main = p.main;
            // duration + startLifetime 최대치로 대충 상한 잡기
            float startLife = main.startLifetime.constantMax;
            float t = main.duration + startLife;
            if (t > maxT) maxT = t;
        }
        return Mathf.Max(0.1f, maxT);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }
}
