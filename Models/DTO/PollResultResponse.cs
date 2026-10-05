namespace VotingApi.Models.Dto;

public class PollOptionResult
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Votes { get; set; }
    public double Percent { get; set; }   
}

public class PollResultResponse
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public List<PollOptionResult> Options { get; set; } = new();
}