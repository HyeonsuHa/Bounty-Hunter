using System.Collections.Generic;
using UnityEngine;

public class GhostOverlayController : MonoBehaviour
{
    [Header("Overlay Material")]
    [SerializeField] private Material overlayMaterial;
    [SerializeField] private string resourcesMaterialPath = "M_GhostOverlay";

    [Header("Behavior")]
    [Tooltip("고스트일 때 원본 Renderer들을 꺼버리고(원본색 제거) 오버레이만 보여줌")]
    [SerializeField] private bool disableSourceRenderers = false;

    [Tooltip("SkinnedMeshRenderer를 매 프레임 Bake 업데이트할지(고스트가 애니메이션/포즈 변화가 있으면 켜기)")]
    [SerializeField] private bool updateSkinnedEachFrame = false;

    [Header("Tints")]
    [SerializeField] private Color canPlaceTint = new Color(0.2f, 1.0f, 0.2f, 0.85f);
    [SerializeField] private Color cannotPlaceTint = new Color(1.0f, 0.15f, 0.15f, 0.85f);

    private static readonly int TintId = Shader.PropertyToID("_Tint");

    private MaterialPropertyBlock _mpb;

    private readonly List<Renderer> _overlayRenderers = new();
    private readonly List<Renderer> _sourceRenderers = new();

    private readonly List<(SkinnedMeshRenderer src, Mesh bakedMesh)> _skinnedBakes = new();

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();

        if (!overlayMaterial)
            overlayMaterial = Resources.Load<Material>(resourcesMaterialPath);

        if (!overlayMaterial)
        {
            Debug.LogError($"[GhostOverlayController] overlayMaterial missing. Put at Assets/Resources/{resourcesMaterialPath}.mat");
            return;
        }

        BuildOverlays();
        SetCanPlace(true);
    }

    private void LateUpdate()
    {
        if (!updateSkinnedEachFrame) return;

        // 스킨드가 움직이면 Bake 갱신
        for (int i = 0; i < _skinnedBakes.Count; i++)
        {
            var (src, mesh) = _skinnedBakes[i];
            if (!src || !mesh) continue;
            src.BakeMesh(mesh);
        }
    }

    private void BuildOverlays()
    {
        _overlayRenderers.Clear();
        _sourceRenderers.Clear();
        _skinnedBakes.Clear();

        // 1) MeshRenderer 처리 (서브메쉬/머티리얼 다중 대응)
        var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var srcMR in meshRenderers)
        {
            var mf = srcMR.GetComponent<MeshFilter>();
            if (!mf || !mf.sharedMesh) continue;

            _sourceRenderers.Add(srcMR);

            var overlayGO = new GameObject(srcMR.gameObject.name + "_GhostOverlay");
            overlayGO.transform.SetParent(srcMR.transform, false);

            var overlayMF = overlayGO.AddComponent<MeshFilter>();
            overlayMF.sharedMesh = mf.sharedMesh;

            var overlayMR = overlayGO.AddComponent<MeshRenderer>();

            int matCount = Mathf.Max(1, srcMR.sharedMaterials != null ? srcMR.sharedMaterials.Length : 1);
            var mats = new Material[matCount];
            for (int i = 0; i < matCount; i++) mats[i] = overlayMaterial;
            overlayMR.sharedMaterials = mats;

            SetupOverlayRenderer(overlayMR);
        }

        // 2) SkinnedMeshRenderer 처리 (플레이어 등)
        var skinned = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var srcSMR in skinned)
        {
            if (!srcSMR || !srcSMR.sharedMesh) continue;

            _sourceRenderers.Add(srcSMR);

            var overlayGO = new GameObject(srcSMR.gameObject.name + "_GhostOverlay");
            overlayGO.transform.SetParent(srcSMR.transform, false);

            var baked = new Mesh();
            baked.name = srcSMR.gameObject.name + "_BakedGhostMesh";
            srcSMR.BakeMesh(baked);

            var overlayMF = overlayGO.AddComponent<MeshFilter>();
            overlayMF.sharedMesh = baked;

            var overlayMR = overlayGO.AddComponent<MeshRenderer>();

            int matCount = Mathf.Max(1, srcSMR.sharedMaterials != null ? srcSMR.sharedMaterials.Length : 1);
            var mats = new Material[matCount];
            for (int i = 0; i < matCount; i++) mats[i] = overlayMaterial;
            overlayMR.sharedMaterials = mats;

            SetupOverlayRenderer(overlayMR);

            _skinnedBakes.Add((srcSMR, baked));
        }

        // 3) 원본 렌더러 끄기(원본색 제거)
        if (disableSourceRenderers)
        {
            foreach (var r in _sourceRenderers)
                if (r) r.enabled = false;
        }
    }

    private void SetupOverlayRenderer(Renderer overlayRenderer)
    {
        overlayRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        overlayRenderer.receiveShadows = false;
        overlayRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        overlayRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        _overlayRenderers.Add(overlayRenderer);
    }

    public void SetCanPlace(bool canPlace)
    {
        if (_overlayRenderers.Count == 0) return;

        var tint = canPlace ? canPlaceTint : cannotPlaceTint;

        _mpb.SetColor(TintId, tint);
        foreach (var r in _overlayRenderers)
            if (r) r.SetPropertyBlock(_mpb);
    }
}
