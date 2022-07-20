using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDungeonPrompts.Application.Queries.SimilarTag
{
	public class SimilarTagQueryViewModelTag
	{
		public string Tag { get; set; } = string.Empty;
		public int NumPrompts { get; set; }
		public float Score { get; set; }
	}

	public class SimilarTagQueryViewModel
	{
		public bool Matched { get; set; }

		public List<SimilarTagQueryViewModelTag> SimilarTags { get; set; } =
			new List<SimilarTagQueryViewModelTag>();
	}
}
