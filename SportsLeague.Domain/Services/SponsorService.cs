using Microsoft.Extensions.Logging;

using SportsLeague.Domain.Entities;

using SportsLeague.Domain.Interfaces.Repositories;

using SportsLeague.Domain.Interfaces.Services;

using System.Net.Mail;


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

        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);

        var sponsor = await _sponsorRepository.GetByIdAsync(id);

        if (sponsor == null)

            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

        return sponsor;

    }


    public async Task<Sponsor> CreateAsync(Sponsor sponsor)

    {

        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))

        {

            throw new InvalidOperationException(

            $"Ya existe un sponsor con el nombre '{sponsor.Name}'");

        }


        if (!IsValidEmail(sponsor.ContactEmail))

        {

            throw new InvalidOperationException("El correo de contacto no tiene un formato válido");

        }


        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);

        return await _sponsorRepository.CreateAsync(sponsor);

    }


    public async Task UpdateAsync(int id, Sponsor sponsor)

    {

        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

        if (existingSponsor == null)

        {

            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        }


        if (existingSponsor.Name != sponsor.Name &&

            await _sponsorRepository.ExistsByNameAsync(sponsor.Name))

        {

            throw new InvalidOperationException(

            $"Ya existe un sponsor con el nombre '{sponsor.Name}'");

        }


        if (!IsValidEmail(sponsor.ContactEmail))

        {

            throw new InvalidOperationException("El correo de contacto no tiene un formato válido");

        }


        existingSponsor.Name = sponsor.Name;

        existingSponsor.ContactEmail = sponsor.ContactEmail;

        existingSponsor.Phone = sponsor.Phone;

        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;

        existingSponsor.Category = sponsor.Category;


        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);

        await _sponsorRepository.UpdateAsync(existingSponsor);

    }


    public async Task DeleteAsync(int id)

    {

        var exists = await _sponsorRepository.ExistsAsync(id);

        if (!exists)

        {

            throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

        }


        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);

        await _sponsorRepository.DeleteAsync(id);

    }


    public async Task LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)

    {

        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);

        if (!sponsorExists)

        {

            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        }


        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);

        if (!tournamentExists)

        {

            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

        }


        if (contractAmount <= 0)

        {

            throw new InvalidOperationException("El monto del contrato debe ser mayor a cero");

        }


        var existingRelation = await _tournamentSponsorRepository

        .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (existingRelation != null)

        {

            throw new InvalidOperationException("Este sponsor ya está vinculado al torneo");

        }


        var tournamentSponsor = new TournamentSponsor

        {

            TournamentId = tournamentId,

            SponsorId = sponsorId,

            ContractAmount = contractAmount,

            JoinedAt = DateTime.UtcNow

        };


        _logger.LogInformation(

        "Linking sponsor {SponsorId} to tournament {TournamentId}",

        sponsorId, tournamentId);

        await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);

    }


    public async Task<IEnumerable<Tournament>> GetTournamentsBySponsorAsync(int sponsorId)

    {

        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);

        if (!sponsorExists)

        {

            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        }


        var relations = await _tournamentSponsorRepository.GetBySponsorAsync(sponsorId);

        return relations.Select(ts => ts.Tournament);

    }


    public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)

    {

        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);

        if (!sponsorExists)

        {

            throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

        }


        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);

        if (!tournamentExists)

        {

            throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

        }


        var relation = await _tournamentSponsorRepository

        .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);

        if (relation == null)

        {

            throw new KeyNotFoundException("No existe vínculo entre el sponsor y el torneo");

        }


        _logger.LogInformation(

        "Unlinking sponsor {SponsorId} from tournament {TournamentId}",

        sponsorId, tournamentId);

        await _tournamentSponsorRepository.DeleteAsync(relation.Id);

    }


    private static bool IsValidEmail(string email)

    {

        if (string.IsNullOrWhiteSpace(email))

            return false;


        try

        {

            var mailAddress = new MailAddress(email);

            return mailAddress.Address == email;

        }

        catch

        {

            return false;

        }

    }

}
