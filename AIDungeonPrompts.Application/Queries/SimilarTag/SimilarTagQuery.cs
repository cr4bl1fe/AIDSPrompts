using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Application.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIDungeonPrompts.Application.Queries.SimilarTag
{
	public class SimilarTagQuery : IRequest<SimilarTagQueryViewModel>
	{
		public SimilarTagQuery(string tag)
		{
			Tag = tag;
		}

		public string Tag { get; set; }
	}

	public class SimilarTagQueryHandler : IRequestHandler<SimilarTagQuery, SimilarTagQueryViewModel>
	{
		private const int MaxResults = 5;
		private readonly IAIDungeonPromptsDbContext _dbContext;

		public SimilarTagQueryHandler(IAIDungeonPromptsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<SimilarTagQueryViewModel> Handle(SimilarTagQuery request,
			CancellationToken cancellationToken = default)
		{
			var searchQueryText = string.Join(" & ", request.Tag.Trim().Split(' ').Select(t => t + ":*"));

			List<SimilarTagQueryViewModelTag>? similarTags = await _dbContext
				.Tags
				.AsNoTracking()
				.Where(e => e.SearchVector.Matches(EF.Functions.ToTsQuery(searchQueryText)))
				.Select(e => new SimilarTagQueryViewModelTag
				{
					Tag = e.Name,
					Score = e.SearchVector.RankCoverDensity(EF.Functions.ToTsQuery(searchQueryText))
				})
				.OrderByDescending(e => e.Score)
				.Take(MaxResults)
				.ToListAsync(cancellationToken: cancellationToken);

			if (similarTags.Count < 1)
			{
				return new SimilarTagQueryViewModel { Matched = false };
			}

			return new SimilarTagQueryViewModel { Matched = true, SimilarTags = similarTags };
		}
	}
}
