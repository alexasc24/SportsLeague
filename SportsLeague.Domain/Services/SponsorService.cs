using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;
using System.Text.RegularExpressions;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentRepository tournamentRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentRepository = tournamentRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _logger = logger;
    }


    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(id);
        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {Id} not found", id);

        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {

        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
            throw new InvalidOperationException("Ya existe un sponsor con ese nombre");


        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El email no tiene un formato válido");

        _logger.LogInformation("Creating sponsor: {Name}", sponsor.Name);

        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existing = await _sponsorRepository.GetByIdAsync(id);
        if (existing == null)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");
       
        if (existing.Name != sponsor.Name &&
            await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
        {
            throw new InvalidOperationException("Ya existe un sponsor con ese nombre");
        }

        if (!IsValidEmail(sponsor.ContactEmail))
            throw new InvalidOperationException("El email no tiene un formato válido");

        existing.Name = sponsor.Name;
        existing.ContactEmail = sponsor.ContactEmail;
        existing.Phone = sponsor.Phone;
        existing.WebsiteUrl = sponsor.WebsiteUrl;
        existing.Category = sponsor.Category;

        _logger.LogInformation("Updating sponsor with ID: {Id}", id);

        await _sponsorRepository.UpdateAsync(existing);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);
        if (!exists)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        _logger.LogInformation("Deleting sponsor with ID: {Id}", id);

        await _sponsorRepository.DeleteAsync(id);
    }


    public async Task AddSponsorToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {

        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");


        var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
        if (tournament == null)
            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");


        var existing = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (existing != null)
            throw new InvalidOperationException("Este sponsor ya está vinculado al torneo");


        if (contractAmount <= 0)
            throw new InvalidOperationException("El monto del contrato debe ser mayor a 0");

        var entity = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Linking sponsor {SponsorId} to tournament {TournamentId}",
            sponsorId, tournamentId);

        await _tournamentSponsorRepository.CreateAsync(entity);
    }

    public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var sponsor = await _sponsorRepository.GetByIdAsync(sponsorId);
        if (sponsor == null)
            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        var relations = await _tournamentSponsorRepository
            .GetBySponsorAsync(sponsorId);

        return relations.Select(ts => ts.Tournament);
    }

    public async Task RemoveSponsorFromTournamentAsync(int sponsorId, int tournamentId)
    {
        var existing = await _tournamentSponsorRepository
            .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (existing == null)
            throw new KeyNotFoundException("La relación no existe");

        await _tournamentSponsorRepository.DeleteAsync(existing.Id);
    }


    private bool IsValidEmail(string email)
    {
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return regex.IsMatch(email);
    }
}
