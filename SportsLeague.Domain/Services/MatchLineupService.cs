using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Application.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchLineupRepository _lineupRepository;

    public MatchLineupService(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IMatchLineupRepository lineupRepository)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _lineupRepository = lineupRepository;
    }
    public async Task<MatchLineup> AddPlayerAsync(int matchId, MatchLineup lineup)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        var player = await _playerRepository.GetByIdAsync(lineup.PlayerId);

        if (player == null)
            throw new KeyNotFoundException($"No se encontró el jugador con ID {lineup.PlayerId}");

        if (player.TeamId != match.HomeTeamId &&
            player.TeamId != match.AwayTeamId)
        {
            throw new InvalidOperationException(
                "El jugador no pertenece a ninguno de los equipos del partido");
        }
        var exists = await _lineupRepository
            .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);

        if (exists)
        {
            throw new InvalidOperationException(
                "El jugador ya está registrado en la alineación de este partido");
        }

        if (lineup.IsStarter)
        {
            var starters = await _lineupRepository
                .CountStartersAsync(matchId, player.TeamId);

            if (starters >= 11)
            {
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");
            }
        }
        if (match.Status != MatchStatus.Scheduled)
        {
            throw new InvalidOperationException(
                "Solo se pueden registrar alineaciones en partidos Scheduled");
        }

        lineup.MatchId = matchId;

        return await _lineupRepository.AddAsync(lineup);
    }
    public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
    {
        return await _lineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
    {
        return await _lineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task DeleteAsync(int matchId, int id)
    {
        var lineup = await _lineupRepository.GetByIdAsync(id);

        if (lineup == null || lineup.MatchId != matchId)
            throw new KeyNotFoundException("Registro de alineación no encontrado");

        await _lineupRepository.DeleteAsync(lineup);
    }
}