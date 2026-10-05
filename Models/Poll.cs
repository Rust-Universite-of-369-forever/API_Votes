namespace VotingApi.Models;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<PollOption> Options { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TotalVotes => Options.Sum(o => o.Votes);
}