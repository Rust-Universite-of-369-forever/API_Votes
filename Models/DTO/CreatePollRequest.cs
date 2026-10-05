using System.ComponentModel.DataAnnotations;

namespace VotingApi.Models.Dto;

public class CreatePollRequest
{
    [Required(ErrorMessage = "Вопрос обязателен")]
    [StringLength(300, MinimumLength = 3)]
    public string Question { get; set; } = string.Empty;

    // Если не задано — автоматически создаются варианты "За" и "Против"
    public List<string>? Options { get; set; }
}