public interface IBossEnemyState
{
    BossEnemyState StateId { get; }
    void Enter(BossEnemyContext context);
    void Update(BossEnemyContext context);
    void Exit(BossEnemyContext context);
}
