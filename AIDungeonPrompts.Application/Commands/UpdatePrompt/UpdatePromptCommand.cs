using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Application.Abstractions.Identity;
using AIDungeonPrompts.Application.Helpers;
using AIDungeonPrompts.Application.Queries.GetUser;
using AIDungeonPrompts.Domain.Entities;
using AIDungeonPrompts.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AIDungeonPrompts.Application.Commands.UpdatePrompt
{
	public class UpdatePromptCommand : IRequest
	{
		[Display(Name = "Author's Note")]
		public string? AuthorsNote { get; set; }

		public string? Description { get; set; }
		public int Id { get; set; }
		public string? Memory { get; set; }

		[Display(Name = "NSFW?")]
		public bool Nsfw { get; set; }

		public int? OwnerId { get; set; }

		public int? ParentId { get; set; }

		[Display(Name = "Prompt")]
		public string PromptContent { get; set; } = string.Empty;

		[Display(Name = "Tags (comma delimited)")]
		public string PromptTags { get; set; } = string.Empty;

		public string? Quests { get; set; }

		public bool SaveDraft { get; set; }

		public byte[]? ScriptZip { get; set; }
		public string Title { get; set; } = string.Empty;

		public string? NovelAiScenario { get; set; }
		public string? HoloAiScenario { get; set; }


		[Display(Name = "World Info")]
		public List<UpdatePromptCommandWorldInfo> WorldInfos { get; set; } = new List<UpdatePromptCommandWorldInfo>
		{
			new UpdatePromptCommandWorldInfo()
		};
	}

	public class UpdatePromptCommandHandler : IRequestHandler<UpdatePromptCommand>
	{
		private readonly ICurrentUserService _currentUserService;
		private readonly IAIDungeonPromptsDbContext _dbContext;

		public UpdatePromptCommandHandler(IAIDungeonPromptsDbContext dbContext, ICurrentUserService currentUserService)
		{
			_dbContext = dbContext;
			_currentUserService = currentUserService;
		}

		public async Task<Unit> Handle(UpdatePromptCommand request, CancellationToken cancellationToken = default)
		{
			if (!_currentUserService.TryGetCurrentUser(out GetUserViewModel? user))
			{
				return Unit.Value;
			}

			Prompt? prompt = await _dbContext.Prompts
				.Include(e => e.PromptTags)
				.Include(e => e.WorldInfos)
				.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

			var isOwner = user!.Id == prompt.OwnerId;
			var canEditField = isOwner ? isOwner : (user.Role & RoleEnum.FieldEdit) != 0;
			var canEditTags = isOwner ? isOwner : (user.Role & RoleEnum.TagEdit) != 0;

			if (canEditField)
			{
				var isDraft = !prompt.ParentId.HasValue
				              && (isOwner
					              ? request.SaveDraft
					              : prompt.IsDraft);
				prompt.AuthorsNote = request.AuthorsNote?.Replace("\r\n", "\n");
				prompt.DateEdited = DateTime.UtcNow;
				prompt.Memory = request.Memory?.Replace("\r\n", "\n");
				prompt.Nsfw = request.Nsfw;
				prompt.PromptContent = request.PromptContent.Replace("\r\n", "\n");
				prompt.Quests = request.Quests?.Replace("\r\n", "\n");
				prompt.Title = request.Title.Replace("\r\n", "\n");
				prompt.Description = request.Description?.Replace("\r\n", "\n");
				prompt.WorldInfos = new List<WorldInfo>();
				prompt.IsDraft = isDraft;
				prompt.PublishDate ??= isDraft ? null : (DateTime?)DateTime.UtcNow;
				prompt.ScriptZip = isOwner
					? request.ScriptZip ?? prompt.ScriptZip
					: prompt.ScriptZip;
				prompt.NovelAiScenario = string.IsNullOrWhiteSpace(request.NovelAiScenario) ? null : request.NovelAiScenario;
				prompt.HoloAiScenario = string.IsNullOrWhiteSpace(request.HoloAiScenario) ? null :  request.HoloAiScenario;

				foreach (var worldInfo in request.WorldInfos)
				{
					if (string.IsNullOrWhiteSpace(worldInfo.Entry) || string.IsNullOrWhiteSpace(worldInfo.Keys))
					{
						continue;
					}

					prompt.WorldInfos.Add(new WorldInfo
					{
						DateCreated = DateTime.UtcNow,
						Entry = worldInfo.Entry.Replace("\r\n", "\n"),
						Keys = worldInfo.Keys.Replace("\r\n", "\n"),
						Prompt = prompt
					});
				}
			}

			if (canEditTags)
			{
				prompt.PromptTags = new List<PromptTag>();
				var promptTags =
					request.PromptTags.Split(',').Select(p => p.Trim().ToLower()).Distinct();
				foreach (var promptTag in promptTags)
				{
					if (string.IsNullOrWhiteSpace(promptTag))
					{
						continue;
					}

					if (string.Equals(promptTag, "nsfw", StringComparison.OrdinalIgnoreCase))
					{
						prompt.Nsfw = true;
						continue;
					}

					Tag? tag = await _dbContext.Tags.FirstOrDefaultAsync(e =>
						EF.Functions.ILike(e.Name, NpgsqlHelper.SafeIlike(promptTag), NpgsqlHelper.EscapeChar));
					if (tag == null)
					{
						prompt.PromptTags.Add(new PromptTag {Prompt = prompt, Tag = new Tag {Name = promptTag}});
					}
					else
					{
						prompt.PromptTags.Add(new PromptTag {Prompt = prompt, Tag = tag});
					}
				}
			}

			_dbContext.Prompts.Update(prompt);
			await _dbContext.SaveChangesAsync(cancellationToken);
			return Unit.Value;
		}
	}
}
