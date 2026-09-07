The reason these are Enums and not Classes/Records etc.
-------------------------------------------------------

Keeping enums for low-level API constants like BlendMode and EnableCap is generally
recommended, especially when wrapping low-level APIs like OpenGL.

 - Performance: enums are lightweight value types. They map directly to the underlying
   int values required by the OpenGL API, ensuring high performance in hot paths like
   rendering loops.

 - Idiomatic C#: Using enum for sets of related constants is standard and idiomatic. It
   provides type safety and better readability than raw int constants throughout the codebase.
 - Existing Usage: The framework already heavily relies on these enums in method signatures
   (e.g., SpriteBatch, IBatch, IGLBindings). Refactoring them into classes or a SmartEnum
   pattern would require significant changes across the entire framework, potentially
   introducing object allocation overhead.


If extra functionality is needed:
---------------------------------

If I find yourself frequently needing to perform complex logic on these values, the best
option is to not refactor the enum itself. Instead, create a static helper class to house
the functionality.

This approach keeps the benefits of the enum while providing extensibility.

    // Keep the enum for API compatibility
    public enum BlendMode
    {
        // ...
    }
    
    // Add logic in a static helper class
    public static class BlendModeExtensions
    {
        public static string ToDescription(this BlendMode mode)
        {
            // Add your custom logic here
        }
    }


