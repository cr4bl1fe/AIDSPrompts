using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIDungeonPrompts.Application.Abstractions.DbContexts;
using AIDungeonPrompts.Domain.Entities;
using AIDungeonPrompts.Web.HostedServices.Abstracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AIDungeonPrompts.Web.HostedServices
{
	public class ApplicationLogCleanerCronJob : CronJobHostedService
	{
		private readonly ILogger<ApplicationLogCleanerCronJob> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public ApplicationLogCleanerCronJob(
			ILogger<ApplicationLogCleanerCronJob> logger,
			IServiceScopeFactory serviceScopeFactory
		) : base("0 1 * * *", TimeZoneInfo.Local, logger)
		{
			_logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public override async Task DoWork(CancellationToken cancellationToken)
		{
			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Running Job");
			using IServiceScope? services = _serviceScopeFactory.CreateScope();
			using IAIDungeonPromptsDbContext? dbContext =
				services.ServiceProvider.GetRequiredService<IAIDungeonPromptsDbContext>();
			if (dbContext == null)
			{
				_logger.LogWarning($"{nameof(ApplicationLogCleanerCronJob)}: Could not get DbContext from services");
				return;
			}

			List<ApplicationLog>? logsToRemove = await dbContext
				.ApplicationLogs
				.Where(e => e.TimeStamp < DateTime.UtcNow.AddDays(-7))
				.ToListAsync(cancellationToken);
			_logger.LogInformation($"Removing {logsToRemove.Count} logs.");
			dbContext.ApplicationLogs.RemoveRange(logsToRemove);
			await dbContext.SaveChangesAsync(cancellationToken);

			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Job Complete");
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Starting");
			await base.StartAsync(cancellationToken);
			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Started");
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Stopping");
			await base.StopAsync(cancellationToken);
			_logger.LogInformation($"{nameof(ApplicationLogCleanerCronJob)} Stopped");
		}
	}
}
