namespace CinemaManagement.Helpers
{
    public static class YoutubeHelper
    {
        public static string ToEmbedUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            if (url.Contains("youtube.com/watch"))
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var videoId = query["v"];
                return $"https://www.youtube.com/embed/{videoId}";
            }

            if (url.Contains("youtu.be/"))
            {
                var id = url.Split("youtu.be/")[1].Split('?')[0];
                return $"https://www.youtube.com/embed/{id}";
            }

            if (url.Contains("youtube.com/embed"))
                return url;

            return null;
        }
    }
}
