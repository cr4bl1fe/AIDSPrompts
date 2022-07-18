using AIDungeonPrompts.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace AIDungeonPrompts.Persistence.Configurations
{
	public class TagConfiguration : IEntityTypeConfiguration<Tag>
	{
		public void Configure(EntityTypeBuilder<Tag> builder)
		{
			builder.HasKey(e => e.Id);
			builder.Property(e => e.Name).IsRequired();
			builder.HasGeneratedTsVectorColumn(e => e.SearchVector, "english", e => new { e.Name })
				.HasIndex(e => e.SearchVector)
				.HasMethod("GIN");
		}
	}
}
