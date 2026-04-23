using FishNet;
using System.Collections;
using UnityEngine;

public class PickUpManager : MonoBehaviour
{
    [SerializeField] private GameObject healthPickUpPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDelay = 10f;
    private bool spawned = false;

    private void Update()
    {
        if ((!spawned) && InstanceFinder.NetworkManager.IsServer)
        {
            spawned = true;
            SpawnAll();
        }
    }

    private void SpawnAll()
    {
        foreach (var point in spawnPoints)
            SpawnPickUp(point.position);
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnPickUp(position);
    }

    private void SpawnPickUp(Vector3 position)
    {
        GameObject go = Instantiate(healthPickUpPrefab, position, Quaternion.identity);
        HealthPickUp pickup = go.GetComponent<HealthPickUp>();
        pickup.SetUp(this);
        // Спавним объект в сети
        InstanceFinder.NetworkManager.ServerManager.Spawn(go);
    }
}