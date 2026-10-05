using System.Text.Json;
using VotingApi.Models;

namespace VotingApi.Services;

public class JsonPollRepository : IPollRepository
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public JsonPollRepository(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "polls.json");

        if (!File.Exists(_filePath))
            File.WriteAllText(_filePath, "[]");
    }

    private async Task<List<Poll>> ReadAsync()
    {
        var json = await File.ReadAllTextAsync(_filePath);
        return JsonSerializer.Deserialize<List<Poll>>(json, _json) ?? new();
    }

    private async Task WriteAsync(List<Poll> polls)
        => await File.WriteAllTextAsync(_filePath,
            JsonSerializer.Serialize(polls, _json));

    public async Task<IEnumerable<Poll>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return await ReadAsync(); }
        finally { _lock.Release(); }
    }

    public async Task<Poll?> GetByIdAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var polls = await ReadAsync();
            return polls.FirstOrDefault(p => p.Id == id);
        }
        finally { _lock.Release(); }
    }

    public async Task<Poll> AddAsync(Poll poll)
    {
        await _lock.WaitAsync();
        try
        {
            var polls = await ReadAsync();
            poll.Id = polls.Count == 0 ? 1 : polls.Max(p => p.Id) + 1;
            polls.Add(poll);
            await WriteAsync(polls);
            return poll;
        }
        finally { _lock.Release(); }
    }

    public async Task<Poll?> VoteAsync(int pollId, int optionIndex)
    {
        await _lock.WaitAsync();
        try
        {
            var polls = await ReadAsync();
            var poll = polls.FirstOrDefault(p => p.Id == pollId);
            if (poll is null) return null;

            var option = poll.Options.FirstOrDefault(o => o.Index == optionIndex);
            if (option is null) return null;

            option.Votes++;
            await WriteAsync(polls);
            return poll;
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _lock.WaitAsync();
        try
        {
            var polls = await ReadAsync();
            var poll = polls.FirstOrDefault(p => p.Id == id);
            if (poll is null) return false;

            polls.Remove(poll);
            await WriteAsync(polls);
            return true;
        }
        finally { _lock.Release(); }
    }
}