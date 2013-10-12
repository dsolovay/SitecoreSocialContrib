using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace scSocialContrib.Instagram.ImageUtil
{
	public class ThubmnailExtractor
	{
		private readonly string _jsonArrayOfPhotos;

		public ThubmnailExtractor(string jsonArrayOfPhotos)
		{
			_jsonArrayOfPhotos = jsonArrayOfPhotos;
		}

		public IEnumerable<ImageDto> GetThumbnails()
		{
			var array = JArray.Parse(_jsonArrayOfPhotos);

			JEnumerable<JToken> jEnumerable = array.Children();

			var thumbnailsAsJsonObjects = jEnumerable.Select(t => t["images"]).Select(t => t["thumbnail"]);
			return thumbnailsAsJsonObjects.Select(thumb => JsonConvert.DeserializeObject<ImageDto>(thumb.ToString())).ToList();
		}
	}
}