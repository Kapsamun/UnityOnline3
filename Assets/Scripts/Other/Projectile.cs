using FishNet.Object;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float _speed = 25f;
    [SerializeField] private int _damage = 20;

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerNetwork target = other.GetComponent<PlayerNetwork>();
        if (target == null) return;

        // Не наносим урон самому себе
        if (target.OwnerId == OwnerId) return;

        //Debug.Log($"shoot: previous HP = {target.HP.Value}");
        int newHp = Mathf.Max(0, target.HP.Value - _damage);
        target.HP.Value = newHp;
        //Debug.Log($"shoot: new HP = {target.HP.Value}");

        base.Despawn(gameObject);
    }
}