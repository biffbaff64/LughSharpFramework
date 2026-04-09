What level of font support should I provide in my 2d game framework?
--------------------------------------------------------------------

A robust 2D game framework should provide a tiered approach to font support, prioritising performance for pixel art
while offering flexibility for high-resolution UIs. At a minimum, your framework must handle bitmap fonts, but modern
standards demand support for signed distance fields (SDF) or TrueType (TTF) rendering for sharp text at any resolution.

Here is the recommended level of font support, broken down by necessity:

Essential Support (Minimum Viable Product)
------------------------------------------

- Bitmap Font Rendering (.fnt / BMFont): This is non-negotiable for 2D games. It involves rendering characters into a
  texture atlas at a fixed size. It is the fastest method and essential for pixel-perfect, retro-style games.
- SpriteBatch Integration: The framework must be able to draw individual character quads quickly, typically using a
  SpriteBatch system, to ensure high performance.
- Basic ASCII Support: Initial support for standard ASCII characters (32-126) is standard.

Required Support (Modern Framework Standards)
---------------------------------------------

- Unicode/UTF-8 Support: Essential for localisation (e.g. Japanese/Chinese/Arabic). The framework must handle characters
  outside the standard ASCII range.
- TrueType/OpenType (.ttf / .otf) Loading: Allow loading font files directly, rather than relying solely on pre-rendered
  atlases. This is vital for flexible UI design.
- Font Kerning & Metrics: Support for ascender, descender, and lineGap metrics, as well as pair-kerning (adjusting space
  between specific letters like "VA").

Advanced Support (Optional but Recommended)
-------------------------------------------

- Signed Distance Fields (SDF / MSDF): This technique allows high-quality scaling of fonts without the blurriness of
  traditional scaling, enabling sharp text at any size from a single texture atlas.
- Dynamic Atlas Generation: Generating font textures at runtime rather than pre-processing them. This is crucial if you
  need to support dynamic text or localization that isn't known at development time.
- Rich Text Rendering: Support for basic markup (e.g., <color>, <bold>) to easily style text in-game.

Summary Recommendation

| Feature 	           | Importance	  | Use Case                                  |
   |---------------------|--------------|-------------------------------------------|
| Bitmap Fonts (.fnt) | Required	    | Pixel art, retro style, high performance. |
| TTF Loading	        | Required	    | High-res UI, non-pixelated games.         |
| UTF-8 Support	      | Required	    | Localization (foreign languages).         |
| SDF/MSDF	           | Recommended	 | Scalable, sharp UI text.                  |
| Rich Text Markup	   | Optional	    | Dynamic dialogue, colored items.          |

Technical Tips
--------------
- Scaling: If not using SDF, stick to integer scaling (2x, 3x) to reduce blurriness. If you must scale, use pixel-perfect
fonts designed for that specific size.
- Anti-aliasing: Ensure the rendering system allows for alpha-testing/blending to maintain crispness.
- Outline/Shadow: Provide an easy way to add outlines or shadows to text, as this is essential for readability against
busy backgrounds. 

https://www.reddit.com/r/gamedev/comments/suyht3/what_should_we_know_about_fonts/

https://blog.cranksoftware.com/what-font-types-should-you-use-in-your-embedded-gui-development-project

https://forum.gamemaker.io/index.php?threads/how-do-you-create-crisp-high-definition-graphics-and-fonts-in-gms.85499/

https://www.youtube.com/watch?v=BvFkUgMUnlc&t=45s
