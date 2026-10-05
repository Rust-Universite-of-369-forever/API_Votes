namespace VotingApi.Models;

public class PollOption
{
    public int Index { get; set; }      // 0..3
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; } = 0;
}