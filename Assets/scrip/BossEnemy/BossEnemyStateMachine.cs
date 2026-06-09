using System.Collections.Generic;

public class BossEnemyStateMachine
{
    private readonly Dictionary<BossEnemyState, IBossEnemyState> _states = new();
    private BossEnemyContext _context;

    public IBossEnemyState CurrentState { get; private set; }
    public BossEnemyState CurrentStateId => CurrentState?.StateId ?? BossEnemyState.Idle;

    public void Initialize(BossEnemyContext context, params IBossEnemyState[] states)
    {
        _context = context;
        _states.Clear();

        foreach (IBossEnemyState state in states)
            _states[state.StateId] = state;
    }

    public void ChangeState(BossEnemyState newState)
    {
        if (CurrentState != null && CurrentState.StateId == newState)
            return;

        if (!_states.TryGetValue(newState, out IBossEnemyState nextState))
            return;

        CurrentState?.Exit(_context);
        CurrentState = nextState;
        CurrentState.Enter(_context);
    }

    public void Update()
    {
        CurrentState?.Update(_context);
    }
}
