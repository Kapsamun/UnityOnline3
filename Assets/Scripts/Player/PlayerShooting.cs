using FishNet.Object;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 0.4f;
    [SerializeField] private int _maxAmmo = 30;

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _playerNetwork = GetComponent<PlayerNetwork>();
        if (IsServer)
            _playerNetwork.Ammo.Value = _maxAmmo;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!_playerNetwork.IsAlive.Value) return;

        if (Input.GetKeyDown(KeyCode.Space))
            ShootServerRpc(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 pos, Vector3 dir)
    {
        // 1. Жив ли игрок?
        if (_playerNetwork.HP.Value <= 0) return;

        // 2. Есть ли патроны?
        if (_playerNetwork.Ammo.Value <= 0) return;

        // 3. Прошёл ли кулдаун?
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        _playerNetwork.Ammo.Value--;

        // Создаем снаряд
        GameObject go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        NetworkObject nob = go.GetComponent<NetworkObject>();
        // Спавним снаряд на сервере и передаем ему владельца (Owner)
        base.Spawn(nob, base.Owner);
    }

    public int GetCurrentAmmo() => _playerNetwork != null ? _playerNetwork.Ammo.Value : 0;
}