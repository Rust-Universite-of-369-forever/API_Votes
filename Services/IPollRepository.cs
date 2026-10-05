using VotingApi.Models;

namespace VotingApi.Services;

public interface IPollRepository
{
    Task<IEnumerable<Poll>> GetAllAsync();
    Task<Poll?> GetByIdAsync(int id);
    Task<Poll> AddAsync(Poll poll);
    Task<Poll?> VoteAsync(int pollId, int optionIndex);
    Task<bool> DeleteAsync(int id);
}