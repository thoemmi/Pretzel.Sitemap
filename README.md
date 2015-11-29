# Pretzel.Sitemap

This is a plugin for [Pretzel](https://github.com/Code52/pretzel), a static site generation tool following (more or less) the same conventions as [Jekyll](https://github.com/mojombo/jekyll).

The purpose of this plugin is to create a `sitemap.xml` file, a [site map](https://en.wikipedia.org/wiki/Site_map) with a list of all posts and pages.

### Background

Generally Pretzel recommends to use a template for sitemaps (it provides templates for both [liquid](https://github.com/Code52/pretzel/blob/master/src/Pretzel.Logic/Resources/Liquid/Sitemap.liquid) and [Razor](https://github.com/Code52/pretzel/blob/master/src/Pretzel.Logic/Resources/Razor/Sitemap.cshtml)), but it doesn't respect paginated pages. That's because paginated pages are not added to `site.Pages` but created transiently.

I wrote this plugin using the same technique as `JekyllEngineBase.ProcessFile`:
for each post and page it adds an `url` entry to the sitemap. Additionally it checks the page's front-matter if `paginate` is specified, and adds relevant URLs to the sitemap too. 

### Installation

Copy `Pretzel.Sitemap.csx` to the `_plugin` folder at the root of your site folder.