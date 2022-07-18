using FluentValidation;

namespace AIDungeonPrompts.Application.Queries.SimilarTag
{
	public class SimilarTagQueryValidator : AbstractValidator<SimilarTagQuery>
	{
		public SimilarTagQueryValidator()
		{
			RuleFor(e => e.Tag).NotEmpty();
		}
	}
}
