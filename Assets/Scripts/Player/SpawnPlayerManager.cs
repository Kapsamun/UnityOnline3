using FishNet.Object;
using UnityEngine;

public class SpawnPlayerManager : NetworkBehaviour
{
    [SerializeField] private Material hostMaterial;
    [SerializeField] private Material clientMaterial;
    private MeshRenderer meshRenderer;

    private void Awake() => meshRenderer = GetComponent<MeshRenderer>();

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (meshRenderer == null) return;
        // OwnerId == 0 соответствует Host (первый подключившийся)
        if (OwnerId == 0)
            meshRenderer.material = hostMaterial;
        else
            meshRenderer.material = clientMaterial;
    }
}