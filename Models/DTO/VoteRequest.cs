using System.ComponentModel.DataAnnotations;

namespace VotingApi.Models.Dto;

public class VoteRequest
{
    [Required]
    public int PollId { get; set; }

    [Required]
    public int OptionIndex { get; set; }
}