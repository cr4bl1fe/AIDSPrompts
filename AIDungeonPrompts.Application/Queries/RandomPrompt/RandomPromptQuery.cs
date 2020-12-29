using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIDungeonPrompts.Application.Queries.RandomPrompt
{
	public class RandomPromptQuery : IRequest<RandomPromptViewModel>
	{
	}

	public class RandomPromptQueryHandler : IRequestHandler<RandomPromptQuery, RandomPromptViewModel>
	{
		private readonly IAIDungeonPromptsDbContext _dbContext;

		public RandomPromptQueryHandler(IAIDungeonPromptsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<RandomPromptViewModel> Handle(RandomPromptQuery request, CancellationToken cancellationToken)
		{
			var count = await _dbContext.Prompts.CountAsync();
			var value = new Random().Next(count);
			var id = await _dbContext.Prompts.OrderBy(e => e.Id).Skip(value).Take(1).Select(e => e.Id).FirstOrDefaultAsync();
			return new RandomPromptViewModel
			{
				Id = id
			};
		}
	}

	public class RandomPromptViewModel
	{
		public int Id { get; internal set; }
	}
}