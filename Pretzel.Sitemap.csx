#r  System.Xml.Linq.dll

using System.Xml.Linq;
using Pretzel.Logic.Extensions;

public class SitemapTransform : ITransform {
    private static readonly XNamespace _xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
    private static readonly XNamespace _xsi = "http://www.w3.org/2001/XMLSchema-instance";
	
    private readonly IFileSystem _fileSystem;
	
    [ImportingConstructor]
	public SitemapTransform(IFileSystem fileSystem) {
		_fileSystem = fileSystem;
    }

    public void Transform(SiteContext siteContext) {
		if (!siteContext.Config.ContainsKey("url"))
		{
			Tracing.Error("You must specify \"url\" in _config.yml to you the Pretzel.Sitemap.csx plugin.");
			return;
		}
		var siteUrl = siteContext.Config["url"];
		
		var urlsetElement = new XElement(_xmlns + "urlset",
			new XAttribute("xmlns", _xmlns),
			new XAttribute(XNamespace.Xmlns + "xsi", _xsi),
			new XAttribute(_xsi + "schemaLocation",
				"http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"));

		foreach (var post in siteContext.Posts) {
			urlsetElement.Add(new XElement(_xmlns + "url",
				new XElement(_xmlns + "loc", siteUrl + post.Url),
				new XElement(_xmlns + "lastmod", GetDate(post)),
				new XElement(_xmlns + "priority", "0.8")));
		}

		foreach (var page in siteContext.Pages.Where(p => !(p is NonProcessedPage))) {
			urlsetElement.Add(new XElement(_xmlns + "url",
				new XElement(_xmlns + "loc", siteUrl + SanitizeUrl(page.Url)),
				new XElement(_xmlns + "lastmod", GetDate(page)),
				new XElement(_xmlns + "priority", page.Id == "/" ? "1.0" : "0.7")));

			object paginateObj;
			if (page.Bag.TryGetValue("paginate", out paginateObj)) {
				var totalPages = (int) Math.Ceiling(siteContext.Posts.Count/Convert.ToDouble(paginateObj));

				var paginateLink = "/page/:page/index.html";
				if (page.Bag.ContainsKey("paginate_link")) {
					paginateLink = Convert.ToString(page.Bag["paginate_link"]);
				}

				for (var i = 2; i <= totalPages; i++) {
					urlsetElement.Add(new XElement(_xmlns + "url",
						new XElement(_xmlns + "loc", siteUrl + SanitizeUrl(paginateLink.Replace(":page", Convert.ToString(i)))),
						new XElement(_xmlns + "lastmod", GetDate(page)),
						new XElement(_xmlns + "priority", "0.7")));
				}
			}
		}

		var doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), urlsetElement);
		using (var stream = _fileSystem.File.Create(_fileSystem.Path.Combine(siteContext.OutputFolder, "sitemap.xml"))) {
			doc.Save(stream);
		}
	}

	private static string GetDate(Page p) {
		return p.Date.ToString("yyyy-MM-ddTHH:mm:sszzz");
	}

	private static string SanitizeUrl(string url) {
		if (url.EndsWith("index.html")) {
			url = url.Substring(0, url.Length - "index.html".Length);
		}
		if (!url.StartsWith("/")) {
			url = "/" + url;
		}
		return url;
	}
}
