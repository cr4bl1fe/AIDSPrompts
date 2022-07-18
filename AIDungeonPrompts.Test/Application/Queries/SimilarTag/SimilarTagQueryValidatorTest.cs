using System.Threading.Tasks;
using AIDungeonPrompts.Application.Queries.SimilarTag;
using FluentValidation.Results;
using Xunit;

namespace AIDungeonPrompts.Test.Application.Queries.SimilarTag
{
	public class SimilarTagQueryValidatorTest
	{
		private readonly SimilarTagQueryValidator _validator;

		public SimilarTagQueryValidatorTest()
		{
			_validator = new SimilarTagQueryValidator();
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("	")]
		public async Task ValidateAsync_ReturnsNotValid_WhenTagIsEmpty(string tag)
		{
			//arrange
			var query = new SimilarTagQuery(tag);

			//act
			ValidationResult? actual = await _validator.ValidateAsync(query);

			//assert
			Assert.False(actual.IsValid);
		}

		[Fact]
		public async Task ValidateAsync_ReturnsValid_WhenTagHasValue()
		{
			//arrange
			var query = new SimilarTagQuery("Value");

			//act
			ValidationResult? actual = await _validator.ValidateAsync(query);

			//assert
			Assert.True(actual.IsValid);
		}
	}
}
