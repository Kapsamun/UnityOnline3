using FishNet.Object;
using UnityEngine;

public class PlayerDeathVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject alivePanel;
    private PlayerNetwork playerNetwork;

    private void Awake()
    {
        playerNetwork = GetComponent<PlayerNetwork>();
        model = gameObject;
        alivePanel.SetActive(false);

        playerNetwork.IsAlive.OnChange += OnIsAliveChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // ’ук OnIsAliveChanged в PlayerNetwork вызовет нужный метод.
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
    }

    // Ётот метод будет вызыватьс€ хуком из PlayerNetwork
    public void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (model != null)
        {
            model.GetComponent<Renderer>().enabled = newValue;
            ui.SetActive(newValue);
            alivePanel.SetActive(!newValue);
        }
    }
}