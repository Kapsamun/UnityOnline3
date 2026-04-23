using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [AllowMutableSyncType]
    public SyncVar<string> Nickname = new SyncVar<string>("Player");

    [AllowMutableSyncType]
    public SyncVar<int> HP = new SyncVar<int>(100);

    [AllowMutableSyncType]
    public SyncVar<bool> IsAlive = new SyncVar<bool>(true);

    [AllowMutableSyncType]
    public SyncVar<int> Ammo = new SyncVar<int>(30);

    private List<Transform> spawnPoints = new List<Transform>();

    private void Awake()
    {
        HP.OnChange += OnHpChanged;
        IsAlive.OnChange += OnIsAliveChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
            foreach (GameObject point in respawnObjects)
                spawnPoints.Add(point.transform);
        }

        // Устанавливаем позицию на сервере
        if (IsServer)
        {
            Ammo.Value = 30;
            if (spawnPoints.Count > 0)
            {
                int index = Random.Range(0, spawnPoints.Count);
                transform.position = spawnPoints[index].position;
            }
            else
            {
                transform.position = Vector3.zero;
            }
        }

        if (OwnerId != 0)
            transform.Rotate(Vector3.up, 180);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Только владелец отправляет на сервер свой локально введенный ник.
        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    // Хуки для SyncVar. Они вызываются на всех клиентах, где есть этот объект.
    private void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        // Только сервер запускает цикл смерти
        if (!IsServer) return;

        if (newValue <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        SetActivePlayer(newValue);
    }

    private void SetActivePlayer(bool isActive)
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = isActive;
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        // Выбрать случайную точку респавна
        int idx = Random.Range(0, spawnPoints.Count);
        Vector3 newPosition = spawnPoints[idx].transform.position;

        // 1. Сначала обновляем позицию на сервере (важно для хоста)
        transform.position = newPosition;

        // 2. Затем оповещаем всех клиентов (включая владельца) через RPC
        TPPlayerObserversRpc(newPosition);
        
        HP.Value = 100;
        Ammo.Value = 30;
        IsAlive.Value = true;
    }

    // В FishNet вместо [ClientRpc] используется [ObserversRpc].
    [ObserversRpc]
    private void TPPlayerObserversRpc(Vector3 spawnPosition)
    {
        if (IsOwner)
            transform.position = spawnPosition;
    }

    // [ServerRpc] в FishNet работает аналогично, но из другого пространства имен.
    // RequireOwnership = false позволяет не-владельцам вызывать этот RPC (например, для UI).
    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает итоговое значение в SyncVar.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerId}" : nickname.Trim();
        if (safeValue.Length > 31)
            safeValue = safeValue.Substring(0, 31);

        Nickname.Value = safeValue;
    }
}