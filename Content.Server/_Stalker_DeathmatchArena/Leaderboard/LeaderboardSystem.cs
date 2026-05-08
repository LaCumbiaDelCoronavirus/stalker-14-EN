using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Content.Server.Administration;
using Content.Server.Database;
using Content.Server.MapText;
using Content.Shared._Stalker_DeathmatchArena.MapText;
using Content.Shared._Stalker_DeathmatchArena.Scorestreak;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Stalker_DeathmatchArena.Leaderboard;

// no locale legends

/// <summary>
///     Leaderboard for highest scorestreak.
/// </summary>
public sealed class LeaderboardSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IServerDbManager _serverDbManager = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;

    private const int TopDisplayed = 3;
    private string? _nextText = null;

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(3d);
    private TimeSpan _nextUpdate = TimeSpan.MinValue;

    private volatile bool _dataLoaded = false; // cross-thread
    private volatile bool _busy = false; // cross-thread, this is whether something is busy using a cross-thread dict on another thread
    private readonly ConcurrentDictionary<Guid, string> _cachedUsernames = []; // cross-thread, more easy to use
    private readonly Dictionary<Guid, int> _currentHighestScores = []; // cross-thread and needs some work too; scores for a player only get saved when theyre in here
    private readonly Stack<(Guid, int)> _queuedWrites = []; // main thread only // for putting off writes to highestscores on main thread until when nothing is busy

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ActorComponent, ScorestreakChangedEvent>(OnScore);

        _ = DoLoadAsync();
    }

    public bool TryResetLeaderboard()
    {
        if (_busy)
            return false;

        foreach (var key in _currentHighestScores.Keys)
            _currentHighestScores[key] = 0;
        _queuedWrites.Clear();
        _nextText = BuildScore();
        _ = DoSaveAsync();

        return true;
    }

    public override void Shutdown()
    {
        DoSaveAsync().Wait(); // we wait
        base.Shutdown();
    }

    private void OnScore(Entity<ActorComponent> entity, ref ScorestreakChangedEvent args)
    {
        TrySetHighestScore(entity.Comp.PlayerSession.UserId.UserId, args.NewScore);
    }

    public void TrySetHighestScore(Guid userId, int newScore)
    {
        ref var scoreRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_currentHighestScores, userId, out var exists);

        if (!_cachedUsernames.ContainsKey(userId))
        {
            _cachedUsernames[userId] = "Loading…"; // lazy hack to make sure nothing else tries to load this username while this is loading it
            _ = LoadUsernameAsync(userId);
        }

        // Do nothing if theres an existing score, thats equal to or bigger than new score
        if (exists &&
            scoreRef >= newScore)
        {
            return;
        }

        // If busy then put the write off to later, otherwise directly set the value
        if (_busy)
            _queuedWrites.Push((userId, newScore));
        else
            scoreRef = newScore;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextText is { } text)
        {
            _nextText = null;
            var eqe = EntityQueryEnumerator<LeaderboardComponent, MapTextComponent>();
            while (eqe.MoveNext(out var uid, out _, out _))
            {
                var overrideComponent = EnsureComp<MapTextOverrideComponent>(uid);
                overrideComponent.Text = text;
                Dirty(uid, overrideComponent);
            }
        }

        if (!_dataLoaded ||
            _busy)
            return;

        // Assumed that rn, nothing is busy and data is loaded
        while (_queuedWrites.TryPop(out var write))
        {
            var (writtenUserId, writtenScore) = write;
            _currentHighestScores[writtenUserId] = writtenScore;
        }

        if (_gameTiming.CurTime < _nextUpdate)
            return;

        _nextUpdate = _gameTiming.CurTime + _updateInterval;
        _ = DoSaveAsync();

        _nextText = BuildScore();
    }

    /// <summary>
    ///     This assumes that nothing is busy!
    /// </summary>
    private string BuildScore()
    {
        // linq gods
        var highestScores = _currentHighestScores.ToList().OrderBy(kv => kv.Value).ToList();

        var text = "Killstreak Leaderboard:\n";
        var count = Math.Min(highestScores.Count, TopDisplayed);
        for (var i = 1; i <= count; i++)
        {
            var profile = highestScores[^i];
            text += $"{i}.  {_cachedUsernames.GetValueOrDefault(profile.Key) ?? "Unknown BEAST"}: {profile.Value}\n";
        }

        if (count == 0)
            text += "Nobody!";

        return text;
    }

    private async Task DoLoadAsync()
    {
        _busy = true;

        var profileList = await _serverDbManager.GetFullStdaLeaderboard();
        foreach (var profile in profileList)
        {
            _currentHighestScores.Add(profile.UserId, profile.Score);
            await LoadUsernameAsync(profile.UserId);
        }

        _busy = false;
        _dataLoaded = true;
    }

    private async Task DoSaveAsync()
    {
        _busy = true;

        foreach (var (savedUserId, savedScore) in _currentHighestScores)
            await _serverDbManager.SetStdaLeaderboard(savedUserId, savedScore);

        _busy = false;
    }

    private async Task LoadUsernameAsync(Guid userId)
    {
        var data = await _playerLocator.LookupIdAsync(new NetUserId(userId));
        var username = data?.Username ?? ("USER-" + userId.ToString());
        _cachedUsernames.AddOrUpdate(userId, username, (oldKey, oldVal) => username);
    }
}
