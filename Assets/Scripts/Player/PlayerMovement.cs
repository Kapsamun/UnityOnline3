using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

public struct MoveData : IReplicateData
{
    public float Horizontal;
    public float Vertical;
    public Vector3 CamForward;
    public Vector3 CamRight;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

public struct ReconcileData : IReconcileData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public float VerticalVelocity;

    private uint _tick;
    public void Dispose() { }
    public uint GetTick() => _tick;
    public void SetTick(uint value) => _tick = value;
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork playerNetwork;
    private CharacterController _cc;

    public float MoveSpeed = 5f;
    public float Gravity = -9.81f;

    private float _verticalVelocity;
    private Camera _mainCamera;

    private void Awake() => _cc = GetComponent<CharacterController>();

    public override void OnStartNetwork()
    {
        if (base.Owner.IsLocalClient)
        {
            _mainCamera = Camera.main;
        }
        else if (!base.IsServerInitialized)
        {
            // Отключаем физику для чужих игроков на клиенте, 
            // чтобы NetworkTransform мог плавно их двигать без сопротивления CharacterController
            _cc.enabled = false;
        }

        base.TimeManager.OnTick += OnTick;
        base.TimeManager.OnPostTick += OnPostTick;
    }

    public override void OnStopNetwork()
    {
        if (base.TimeManager != null)
        {
            base.TimeManager.OnTick -= OnTick;
            base.TimeManager.OnPostTick -= OnPostTick;
        }
    }

    private void OnTick()
    {
        // Только владелец отправляет ввод
        // Обозреватели не должны вызывать Replicate(default), иначе они будут стоять на месте
        if (base.IsOwner)
        {
            Replicate(BuildMoveData());
        }
    }

    private void OnPostTick()
    {
        // Только сервер должен создавать данные для сверки
        if (base.IsServerInitialized)
        {
            CreateReconcile();
        }
    }

    public override void CreateReconcile()
    {
        ReconcileData rd = new ReconcileData
        {
            Position = transform.position,
            Rotation = transform.rotation,
            VerticalVelocity = _verticalVelocity,
        };
        Reconcile(rd);
    }

    private MoveData BuildMoveData()
    {
        if (!playerNetwork.IsAlive.Value)
            return default;

        MoveData md = new MoveData
        {
            Horizontal = Input.GetAxisRaw("Horizontal"),
            Vertical = Input.GetAxisRaw("Vertical"),
        };

        if (_mainCamera != null)
        {
            md.CamForward = _mainCamera.transform.forward;
            md.CamRight = _mainCamera.transform.right;
            md.CamForward.y = 0; md.CamRight.y = 0;
            md.CamForward.Normalize(); md.CamRight.Normalize();
        }

        return md;
    }

    /// <summary>
    /// Метод для перемещения
    /// </summary>
    [Replicate]
    private void Replicate(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        float delta = (float)base.TimeManager.TickDelta;
        Vector3 finalMovement = Vector3.zero;

        if (playerNetwork.IsAlive.Value)
        {
            Vector3 moveDir = md.CamForward * md.Vertical + md.CamRight * md.Horizontal;
            float currentSpeed = MoveSpeed;

            finalMovement = moveDir * currentSpeed;
        }

        _verticalVelocity += Gravity * delta;
        finalMovement.y = _verticalVelocity;

        // Двигаем только если CharacterController включен (он выключен у обозревателей)
        if (_cc.enabled)
        {
            _cc.Move(finalMovement * delta);
            if (_cc.isGrounded) _verticalVelocity = -2f;
        }
    }

    /// <summary>
    /// Метод для коррекции позиции
    /// </summary>
    [Reconcile]
    private void Reconcile(ReconcileData rd, Channel channel = Channel.Unreliable)
    {
        bool wasEnabled = _cc.enabled;
        _cc.enabled = false;
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        _verticalVelocity = rd.VerticalVelocity;
        _cc.enabled = wasEnabled;
    }
}