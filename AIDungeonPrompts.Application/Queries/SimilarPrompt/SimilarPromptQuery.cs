using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Application.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIDungeonPrompts.Application.Queries.SimilarPrompt
{
	public class SimilarPromptQuery : IRequest<SimilarPromptViewModel>
	{
		public SimilarPromptQuery(string title, int? currentId = null)
		{
			Title = title;
			CurrentId = currentId;
		}

		public int? CurrentId { get; set; }
		public string Title { get; set; }
	}

	public class SimilarPromptQueryHandler : IRequestHandler<SimilarPromptQuery, SimilarPromptViewModel>
	{
		private readonly IAIDungeonPromptsDbContext _dbContext;

		public SimilarPromptQueryHandler(IAIDungeonPromptsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<SimilarPromptViewModel> Handle(SimilarPromptQuery request,
			CancellationToken cancellationToken = default)
		{
			var query = _dbContext
				.Prompts
				.Where(prompt => !prompt.IsDraft)
				.Where(prompt =>
					EF.Functions.ILike(prompt.Title, NpgsqlHelper.SafeIlike(request.Title), NpgsqlHelper.EscapeChar));

			if (request.CurrentId.HasValue)
			{
				query = query.Where(e => e.Id != request.CurrentId.Value);
			}

			List<SimilarPromptDetailsViewModel>? similarPrompts = await query
				.AsNoTracking()
				.Select(prompt => new SimilarPromptDetailsViewModel { Id = prompt.Id, Title = prompt.Title })
				.ToListAsync(cancellationToken);

			if (similarPrompts.Count < 1)
			{
				return new SimilarPromptViewModel { Matched = false };
			}

			return new SimilarPromptViewModel { Matched = true, SimilarPrompts = similarPrompts };
		}
	}
}
