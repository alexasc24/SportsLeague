namespace SportsLeague.Domain.Interfaces.Repositories;

public interface IMatchLineupRepository
{
    Task<MatchLineup> AddAsync(MatchLineup lineup);

    Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId);

    Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId);

    Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId);

    Task<int> CountStartersAsync(int matchId, int teamId);

    Task<MatchLineup?> GetByIdAsync(int id);

    Task DeleteAsync(MatchLineup lineup);
}