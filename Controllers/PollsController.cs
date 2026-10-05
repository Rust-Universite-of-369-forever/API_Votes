using Microsoft.AspNetCore.Mvc;
using VotingApi.Models;
using VotingApi.Models.Dto;
using VotingApi.Services;
using web_api.Models;

namespace VotingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PollsController : ControllerBase
    {
        private readonly IPollRepository _repository;

        public PollsController(IPollRepository repository)
        {
            _repository = repository;
        }

        // GET: api/polls
        // Список голосований БЕЗ результатов — только ID и вопрос
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var polls = await _repository.GetAllAsync();
            var list = polls.Select(p => new
            {
                p.Id,
                p.Question,
                OptionsCount = p.Options.Count
            });
            return Ok(list);
        }

        // GET: api/polls/5
        // Детали с процентами
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PollResultResponse>> GetDetails(int id)
        {
            var poll = await _repository.GetByIdAsync(id);
            if (poll is null) return NotFound();

            return Ok(BuildResult(poll));
        }

        // POST: api/polls
        [HttpPost]
        public async Task<ActionResult<Poll>> Create([FromBody] CreatePollRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            List<string> optionTexts;

            if (request.Options is null || request.Options.Count == 0)
            {
                optionTexts = new List<string> { "За", "Против" };
            }
            else
            {
                if (request.Options.Count < 2 || request.Options.Count > 4)
                    return BadRequest("Количество вариантов должно быть от 2 до 4");

                optionTexts = request.Options
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .ToList();

                if (optionTexts.Count < 2)
                    return BadRequest("Нужно минимум 2 непустых варианта");
            }

            var poll = new Poll
            {
                Question = request.Question,
                Options = optionTexts
                    .Select((text, i) => new PollOption { Index = i, Text = text })
                    .ToList()
            };

            var created = await _repository.AddAsync(poll);
            return CreatedAtAction(nameof(GetDetails), new { id = created.Id }, created);
        }

        [HttpPost("vote")]
        public async Task<ActionResult<PollResultResponse>> Vote([FromBody] VoteRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var poll = await _repository.VoteAsync(request.PollId, request.OptionIndex);
            if (poll is null)
                return NotFound("Голосование или вариант не найдены");

            return Ok(BuildResult(poll));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }


        private static PollResultResponse BuildResult(Poll poll)
        {
            int total = poll.TotalVotes;

            return new PollResultResponse
            {
                Id = poll.Id,
                Question = poll.Question,
                TotalVotes = total,
                Options = poll.Options.Select(o => new PollOptionResult
                {
                    Index = o.Index,
                    Text = o.Text,
                    Votes = o.Votes,
                    Percent = total == 0
                        ? 0
                        : Math.Round(o.Votes * 100.0 / total, 2)
                }).ToList()
            };
        }
    }
}