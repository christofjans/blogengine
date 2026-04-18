# BlogEngine

BlogEngine is a simple blog engine. It takes a bunch of markdown files as its input and produces a static website as its output.


Posts are markdown (`*.md`) files that contain the following JSON front matter:

```json
{"title": "Calling Javascript from Blazor", "date": "2020-12-12"}
```

If you want the markdown to be processed but *not* show up in the RSS feed, you can add `"norss": true` to the front matter:

```json
{"title": "About me", "date": "2020-12-09", "norss": true}
```

If the post contains Latex, you can add `"math": true` to the front matter:

```json
{"title": "Math post", "date": "2020-12-09", "math": true}
```

This will cause some javascript to be included to render the Latex correctly.

Posts are converted to HTML and enriched with `post.template.html` to create the final output.
If `norss` is set to `true`, the the template used is `nopost.template.html` instead.

The blog index is generated from `index.md` and enriched with `index.template.html`.

Asset files (like images) are simply copied to the output folder.