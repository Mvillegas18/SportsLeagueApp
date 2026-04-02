namespace SportsLeague.Domain.Entities;


public class TournamentSponsor : AuditBase

{

    public int TournamentId { get; set; }

    public int SponsorId { get; set; }


    // Navigation Properties

    public Tournament Tournament { get; set; } = null!;

    public Sponsor Sponsor { get; set; } = null!;

}
