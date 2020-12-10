# blogengine

### index.html

```
BLOG    ABOUT

daten   Titlen
        Summaryn

daten_1 Titlen_1
        Summaryn_1
```


### about.html

```
BLOG    ABOUT

yadayada
```

### post.html

```
BLOG    ABOUT

titlen_1    Title
titlen_2    yadayda
```

---

* all markdown files (posts or not) with associated images go in input directory
    * about.md, index.template.html, post.template.html, about.template.html, nonpost.template.html are treated specially
* convert.exe [inputdir] [outputdir] converts .md to .html and copies all files to output dir
* posts have yyy-MM-dd as first non-blank line and `# title` as second non-blank line
* md files that DON't have the previous setup aren't posts but are still converted.
* use [Handlebars.net](https://www.nuget.org/packages/Handlebars.Net/) for templating
* use [markdig](https://github.com/lunet-io/markdig) for markdown .
* use [KaTex](https://katex.org/) ?
* use [rss](https://www.nuget.org/packages/WilderMinds.RssSyndication/) ?
* usecases uses System.IO.Abstractions ?

