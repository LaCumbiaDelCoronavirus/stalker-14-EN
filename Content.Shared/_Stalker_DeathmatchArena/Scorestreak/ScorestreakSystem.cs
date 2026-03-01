namespace Content.Shared._Stalker_DeathmatchArena.Scorestreak;

public sealed class ScorestreakSystem : EntitySystem
{
    private EntityQuery<ScorestreakComponent> _scorestreakQuery;

    public override void Initialize()
    {
        base.Initialize();
        _scorestreakQuery = GetEntityQuery<ScorestreakComponent>();
    }

    private void OnScorestreakUpdated(Entity<ScorestreakComponent> entity, int oldScore)
    {
        Dirty(entity);

        var ev = new ScorestreakChangedEvent(oldScore, entity.Comp.Score);
        RaiseLocalEvent(entity, ref ev);
    }

    public void SetScore(Entity<ScorestreakComponent?> entity, int value)
    {
        if (!_scorestreakQuery.HasComponent(entity.Owner))
            entity.Comp = AddComp<ScorestreakComponent>(entity);

        var oldScore = entity.Comp!.Score;
        if (oldScore == value)
            return;

        entity.Comp!.Score = value;

        OnScorestreakUpdated(entity!, oldScore);
    }

    /// <returns>New score.</returns>
    public int AddScore(Entity<ScorestreakComponent?> entity, int added)
    {
        if (!_scorestreakQuery.HasComponent(entity.Owner))
            entity.Comp = AddComp<ScorestreakComponent>(entity);

        var oldScore = entity.Comp!.Score;
        entity.Comp!.Score += added;

        OnScorestreakUpdated(entity!, oldScore);
        return entity.Comp!.Score;
    }

    public int GetScorestreak(Entity<ScorestreakComponent?> entity)
    {
        if (!_scorestreakQuery.Resolve(entity.Owner, ref entity.Comp, logMissing: false))
            return 0;

        return entity.Comp.Score;
    }
}
