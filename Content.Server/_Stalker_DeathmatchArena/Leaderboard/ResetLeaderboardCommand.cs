using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Stalker_DeathmatchArena.Leaderboard;

[AdminCommand(AdminFlags.Host)]
public sealed partial class ResetLeaderboardCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public string Command => "stda-reset_leaderboard";
    public string Description => "Resets the score leaderboard.";
    public string Help => "stda-reset_leaderboard";

    public void Execute(IConsoleShell shell, string argstr, string[] args)
    {
        if (!_entityManager.TrySystem<LeaderboardSystem>(out var leaderboardSystem))
        {
            shell.WriteLine("Failed; the system can't be found yet!");
            return;
        }

        shell.WriteLine(leaderboardSystem.TryResetLeaderboard() ?
            "Successfully resetted leaderboard!" :
            "Couldn't reset leaderboard. Maybe a DB operation was in progress that was blocking it? Try again in a few moments.");
    }
}
