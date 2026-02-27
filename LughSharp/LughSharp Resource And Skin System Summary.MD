<h2>**🛠️ Framework Architecture: Resource & Skin System.**</h2>

<h3>**1. The Core Principles (The "Deviations")**</h3>

- **Decoupled Serialization**: Keep Newtonsoft.Json logic in Converters, not in the core classes. This keeps the OpenGL
  and UI logic "clean" and easier to unit test.

- **Flat JSON Structure:** Group by functional categories (Colors, Drawables, Styles) rather than Java namespaces.

- **DPI-Aware Loading:** Apply scaling at the load point. A 10px margin in JSON becomes 20px in memory on a $2.0x$ scale
  screen.

- **Reference Resolution:** Resources are loaded in a specific hierarchy so that later objects can reference earlier
  ones by name.

---

<h3>**2. Loading Hierarchy (Dependency Graph)**</h3>
To avoid "Resource Not Found" errors, the scheduler should load assets in this specific order:

| Order | Resource Type | Dependencies   | Purpose                                               |
|-------|---------------|----------------|-------------------------------------------------------|
| 1.    | Texture Atlas | Image Files    | Provides the raw pixel regions for everything else.   |
| 2.    | Colors        | None           | Defines the palette (Hex or RGBA).                    |
| 3.    | Fonts         | Files / Scale  | Generates Glyphs (SDF or Bitmap) at the correct DPI.  |
| 4.    | Drawables     | Atlas / Colors | Wraps TextureRegions with logic (NinePatch, Tints).   |
| 5.    | Styles        | All Above      | Combines Drawables, Fonts, and Colors into UI states. |

---

<h3>**3. Key Component Responsibilities**</h3>

**The ResourceManager**

- Holds the dictionaries for every loaded resource.
- Stores the GlobalScale factor.
- Provides the Get<T>(name) method used by converters to resolve "parent" or "color" references.

**The Parent-Merge Logic**

- Uses JObject.Merge during the ReadJson phase.
- Allows for DRY (Don't Repeat Yourself) JSON files.
- Workflow: Check for parent $\rightarrow$ Fetch parent from Registry $\rightarrow$ Clone parent to
  JObject $\rightarrow$ Merge current JSON over it $\rightarrow$ Deserialize.

**The Drawable Converter**

- The "Smart Bridge" between strings and OpenGL textures.
- Translates "up": "btn_bg" into a TextureRegionDrawable by querying the TextureAtlas.

---

<h3>**4. OpenGL Integration**</h3>

- **IDrawable Interface:** Every drawable resource should implement a Draw(SpriteBatch batch, float x, float y, float w,
  float h) method.
- **State Caching:** Styles should store ready-to-use references to TextureIDs and Color objects to minimize overhead
  during the render loop.

---
<h4>(c) Circa64 Software Projects 2026</h4>
---
