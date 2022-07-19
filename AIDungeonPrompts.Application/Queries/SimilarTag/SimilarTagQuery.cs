using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Application.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

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
		private static readonly Regex SanitizationPattern = new Regex("[<&|!\t\r\n()]");

		private readonly IAIDungeonPromptsDbContext _dbContext;

		public SimilarTagQueryHandler(IAIDungeonPromptsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		private static string SanitizeQueryString(string input)
		{
			return SanitizationPattern.Replace(input, "");
		}

		public async Task<SimilarTagQueryViewModel> Handle(SimilarTagQuery request,
			CancellationToken cancellationToken = default)
		{
			var searchQueryText = string.Join(" & ", SanitizeQueryString(request.Tag).Trim().Split(' ').Where(t => !string.IsNullOrEmpty(t)).Select(t => t + ":*"));
			if (searchQueryText.Length == 0)
			{
				return new SimilarTagQueryViewModel { Matched = false };
			}

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
