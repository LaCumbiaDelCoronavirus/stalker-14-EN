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
        if (entity.Comp is not { } scorestreakComponent &&
            !_scorestreakQuery.TryGetComponent(entity.Owner, out scorestreakComponent))
            scorestreakComponent = AddComp<ScorestreakComponent>(entity);
        entity.Comp = scorestreakComponent;

        var oldScore = scorestreakComponent.Score;
        if (oldScore == value)
            return;

        scorestreakComponent.Score = value;

        OnScorestreakUpdated(entity!, oldScore);
    }

    /// <returns>New score.</returns>
    public int AddScore(Entity<ScorestreakComponent?> entity, int added)
    {
        if (entity.Comp is not { } scorestreakComponent &&
            !_scorestreakQuery.TryGetComponent(entity.Owner, out scorestreakComponent))
            scorestreakComponent = AddComp<ScorestreakComponent>(entity);
        entity.Comp = scorestreakComponent;

        var oldScore = scorestreakComponent.Score;
        scorestreakComponent.Score += added;

        OnScorestreakUpdated(entity!, oldScore);
        return scorestreakComponent.Score;
    }

    public int GetScorestreak(Entity<ScorestreakComponent?> entity)
    {
        if (!_scorestreakQuery.Resolve(entity.Owner, ref entity.Comp, logMissing: false))
            return 0;

        return entity.Comp.Score;
    }
}
