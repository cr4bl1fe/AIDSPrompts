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
		public SimilarTagQuery(string tag, bool publicPromptsOnly = true)
		{
			Tag = tag;
			PublicPromptsOnly = publicPromptsOnly;
		}

		public string Tag { get; set; }
		public bool PublicPromptsOnly { get; set; }
	}

	public class SimilarTagQueryHandler : IRequestHandler<SimilarTagQuery, SimilarTagQueryViewModel>
	{
		private const int MaxResults = 5;
		private const double TagSimilarityBias = 10.0;
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
			var sanitizedTrimmedTag = SanitizeQueryString(request.Tag).Trim();
			if (sanitizedTrimmedTag.Length == 0)
			{
				return new SimilarTagQueryViewModel { Matched = false };
			}

			var searchQueryText = string.Join(" & ", sanitizedTrimmedTag
				.Split(' ')
				.Where(t => !string.IsNullOrEmpty(t))
				.Select(t => t + ":*"));

			if (searchQueryText.Length == 0)
			{
				return new SimilarTagQueryViewModel { Matched = false };
			}

			List<SimilarTagQueryViewModelTag>? similarTags = await _dbContext
				.Tags
				.AsNoTracking()
				.Where(e => e.SearchVector.Matches(EF.Functions.ToTsQuery(searchQueryText)) || e.Name.StartsWith(sanitizedTrimmedTag))
				.Select(e => new SimilarTagQueryViewModelTag
				{
					Tag = e.Name,
					NumPrompts = _dbContext.Prompts.Where(p => p.PublishDate != null && p.PromptTags.Any(pt => pt.TagId == e.Id)).Count(),
					Score = (float)((e.SearchVector.RankCoverDensity(EF.Functions.ToTsQuery(searchQueryText))
						+ (e.Name == sanitizedTrimmedTag ? TagSimilarityBias : 0.0)) * 10.0)
				})
				.Where(e => e.NumPrompts > 0 || !request.PublicPromptsOnly)
				.OrderByDescending(e => e.NumPrompts + e.Score)
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
