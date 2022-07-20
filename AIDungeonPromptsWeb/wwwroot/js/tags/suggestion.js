const tags = new Bloodhound({
	datumTokenizer: datum => Bloodhound.tokenizers.whitespace(datum.value),
	queryTokenizer: Bloodhound.tokenizers.whitespace,
	remote: {
		url: "/similar-tags",
		prepare: (query, settings) => {
			settings.type = "POST";
			settings.data = { "tag": query };
			
			return settings;
		},
		transform: (response) => {
			$(".bootstrap-tagsinput").removeClass("loading");

			if (!response.matched) return [];
			return response.similarTags.map((e) => e.tag + " (" + e.numPrompts + ")");
		},
	},
});

const tagsElem = $(".tags-input");

tagsElem.tagsinput({
	trimValue: true,
	freeInput: true,
	cancelConfirmKeysOnEmpty: document.location.pathname === "/",
	confirmKeys: [
		13, // Enter
		44, // Comma
	],
	typeaheadjs: {
		limit: 10,
		source: tags,
	},
	tagClass: "badge badge-primary",
});

// Loading animation
const input = tagsElem.tagsinput("input");
input.bind("typeahead:asyncrequest", (ev, q, d) => {
	$(".bootstrap-tagsinput").addClass("loading");
});
const end = (ev, q, d) => {
	$(".bootstrap-tagsinput").removeClass("loading");
};
input.bind("typeahead:asyncreceive", end);
input.bind("typeahead:asynccancel", end);

// Fix typeahead cache
tagsElem.on("itemAdded", () => {
	input.typeahead("val", "");
});

$(".bootstrap-tagsinput").addClass("form-control");
