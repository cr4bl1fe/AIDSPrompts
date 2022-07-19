using System.Threading.Tasks;
using AIDungeonPrompts.Application.Queries.SimilarTag;
using AIDungeonPrompts.Domain.Entities;
using AIDungeonPrompts.Test.Collections.Database;
using Xunit;

namespace AIDungeonPrompts.Test.Application.Queries.SimilarTag
{
	public class SimilarTagQueryHandlerTest : DatabaseFixtureTest
	{
		private readonly SimilarTagQueryHandler _handler;

		public SimilarTagQueryHandlerTest(DatabaseFixture fixture) : base(fixture)
		{
			_handler = new SimilarTagQueryHandler(DbContext);
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(5)]
		public async Task Handle_ReturnsMatches_WhenTagsMatch(int expectedAmount)
		{
			//arrange
			for (var i = 0; i < expectedAmount; i++)
			{
				DbContext.Tags.Add(new Tag {Name = "TestTag" + i});
			}

			await DbContext.SaveChangesAsync();
			var query = new SimilarTagQuery("TestTag");

			//act
			SimilarTagQueryViewModel? actual = await _handler.Handle(query);

			//assert
			Assert.True(actual.Matched);
			Assert.Equal(expectedAmount, actual.SimilarTags.Count);
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(5)]
		public async Task Handle_ReturnsNoMatches_WhenTagsDontMatch(int expectedAmount)
		{
			//arrange
			for (var i = 0; i < expectedAmount; i++)
			{
				DbContext.Tags.Add(new Tag { Name = "TestTag" + i });
			}

			await DbContext.SaveChangesAsync();
			var query = new SimilarTagQuery("NonMatchingTag");

			//act
			SimilarTagQueryViewModel? actual = await _handler.Handle(query);

			//assert
			Assert.False(actual.Matched);
			Assert.Empty(actual.SimilarTags);
		}

		[Theory]
		[InlineData("<")]
		[InlineData("()")]
		[InlineData("|")]
		[InlineData("!")]
		[InlineData("!()<|")]
		public async Task Handle_ReturnsNoMatches_WhenInputIsInvalid(string input)
		{
			//arrange
			var query = new SimilarTagQuery(input);

			//act
			SimilarTagQueryViewModel? actual = await _handler.Handle(query);

			//assert
			Assert.False(actual.Matched);
			Assert.Empty(actual.SimilarTags);
		}
	}
}
