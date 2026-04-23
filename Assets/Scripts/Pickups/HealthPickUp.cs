using FishNet.Object;
using UnityEngine;

public class HealthPickUp : NetworkBehaviour
{
    [SerializeField] private int healAmount = 50;
    private PickUpManager pickUpManager;
    private Vector3 spawnPosition;

    public void SetUp(PickUpManager manager)
    {
        pickUpManager = manager;
        spawnPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerNetwork player = other.GetComponent<PlayerNetwork>();
        if (player == null) return;
        if (!player.IsAlive.Value) return;
        if (player.HP.Value >= 100) return;

        player.HP.Value = Mathf.Min(100, player.HP.Value + healAmount);
        pickUpManager.OnPickedUp(spawnPosition);
        base.Despawn(gameObject);
    }
}