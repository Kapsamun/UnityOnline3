using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerUIHandler : NetworkBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text respawnTimerText;
    private PlayerShooting playerShooting;
    private PlayerNetwork playerNetwork;
    private float deathTime;

    private void Awake()
    {
        playerShooting = GetComponent<PlayerShooting>();
        playerNetwork = GetComponent<PlayerNetwork>();

        playerNetwork.IsAlive.OnChange += OnIsAliveChanged;
        playerNetwork.Ammo.OnChange += OnAmmoChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (!base.Owner.IsLocalClient)
        {
            enabled = false;
            ammoText.gameObject.SetActive(false);
            return;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        // ќтписка от хуков не требуетс€, так как они управл€ютс€ самим FishNet.
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (playerShooting != null && ammoText != null)
            ammoText.text = $"{playerShooting.GetCurrentAmmo()}";

        if ((playerNetwork != null) && (!playerNetwork.IsAlive.Value) && (respawnTimerText != null))
        {
            float elapsed = Time.time - deathTime;
            float remaining = Mathf.Max(0, 3f - elapsed);
            respawnTimerText.text = $"Respawn after {remaining:F1} sec.";
            if (remaining <= 0)
                respawnTimerText.text = "";
        }
        else if (respawnTimerText != null)
        {
            respawnTimerText.text = "";
        }
    }

    // Ётот метод будет вызыватьс€ хуком из PlayerNetwork
    public void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (!newValue)
            deathTime = Time.time;
    }

    // Ётот метод будет вызыватьс€ хуком из PlayerNetwork
    public void OnAmmoChanged(int oldValue, int newValue, bool asServer)
    {
        if (ammoText != null)
            ammoText.text = $"{newValue}";
    }
}