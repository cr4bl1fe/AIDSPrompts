using System.ComponentModel.DataAnnotations;

namespace AIDungeonPrompts.Web.Models
{
	public class SimilarTagQueryModel
	{
		[Required]
		public string Tag { get; set; } = string.Empty;
	}
}
