namespace FluentFilters
{
    public class TeamSeason
    {
        public int Year { get; set; }
        public string League { get; set; }
        public string TeamIdAlt { get; set; }
        public string FranchiseId { get; set; }
        public string Division { get; set; }
        public int Rank { get; set; }
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public int? Runs { get; set; }
        public int? HomeRuns { get; set; }
        public decimal Era { get; set; }
        public string Name { get; set; }
        public string Park { get; set; }
        public string TeamId { get; set; }
    }
}