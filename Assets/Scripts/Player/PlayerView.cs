using FishNet.Object;
using TMPro;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text hpText;

    private void Awake()
    {
        playerNetwork.HP.OnChange += OnHpChanged;
        playerNetwork.Nickname.OnChange += OnNicknameChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        nicknameText.text = playerNetwork.Nickname.Value;
        hpText.text = $"HP: {playerNetwork.HP.Value}";
    }

    // Эти методы будут вызываться хуками из PlayerNetwork
    public void OnNicknameChanged(string oldValue, string newValue, bool asServer)
    {
        nicknameText.text = newValue;
    }

    public void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        hpText.text = $"HP: {newValue}";
    }
}