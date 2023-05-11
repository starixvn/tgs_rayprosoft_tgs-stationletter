using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.StationLetterService;

public static class StationLetterUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
	{
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};
}
