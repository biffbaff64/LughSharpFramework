I. Core Buffer Class Structure and Relationships:
=================================================

ByteBuffer:
===========

 - Foundation: Serves as the base, byte-level buffer. It manages the underlying byte[] and Memory<byte>.
 - Storage: Uses a byte[] as its backing array (_backingArray) and exposes it through Memory<byte> (_memory).
 - Capacity: Measured in bytes. Represents the total allocated byte size.
 - Position: _position (int) tracks the current read/write position (in bytes).
 - Limit: _limit (int) marks the boundary for read operations (set by Flip()).
   Length: _length (int) tracks the number of elements of their specific type currently "valid" or "written".
 - Endianness: IsBigEndian (bool) flag to indicate byte order (Big-Endian or Little-Endian).

Key Responsibilities:
---------------------
 - Managing raw byte storage and capacity.
 - Tracking Position and Limit.
 - Handling endianness via IsBigEndian.
 - Providing basic byte-level PutByte, GetByte operations (and potentially sequential versions without index that
 - update position).
 - Providing type-specific GetShort, PutShort, GetInt, PutInt, GetFloat, PutFloat methods that operate at byte
 - offsets and use BinaryPrimitives with endianness awareness.
 - Resize(int extraCapacity) method to dynamically increase capacity.
 - Clear() method to reset Position and Limit (and optionally zero out content).
 - Flip() method to switch from write to read mode.


ShortBuffer, IntBuffer, FloatBuffer:
====================================

Type-Specific Views:
--------------------

Provide type-safe views of an underlying ByteBuffer, specialized for short, int, and float values respectively.
Backed by ByteBuffer: Each type-specific buffer holds a reference to a ByteBuffer instance (_byteBuffer). They
do not have their own backing arrays.
Capacity: Measured in number of elements of their specific type (shorts, ints, floats). Derived from the
ByteBuffer's byte capacity.
Length: _length (int) tracks the number of elements of their specific type currently "valid" or "written".
Endianness: Inherit the IsBigEndian setting from their underlying ByteBuffer.

Key Responsibilities:
---------------------

Providing type-safe GetShort, PutShort (for ShortBuffer), GetInt, PutInt (for IntBuffer), GetFloat, PutFloat
(for FloatBuffer) methods. These methods operate on indices of their respective types and delegate to the
ByteBuffer's type-specific methods, calculating byte offsets.
Calculating and exposing Capacity in units of their specific type.
Managing Length to track the number of valid elements.
Clear() method to reset Length and delegate Clear() to the underlying ByteBuffer.
Flip() method to delegate Flip() to the underlying ByteBuffer (and optionally update Length to reflect readable
element count).


II. Backing Array and Memory Access:
====================================

byte[] for ByteBuffer: ByteBuffer uses byte[] (_backingArray) as its core storage.
Memory<byte> for Efficient Access: ByteBuffer exposes its backing array through a Memory<byte> (_memory) for
efficient access, especially when combined with Span<byte>.
No Separate Backing Arrays in Type-Specific Buffers: ShortBuffer, IntBuffer, FloatBuffer do not have their own
arrays. They operate on the ByteBuffer's Memory<byte>.


III. Capacity, Length, Position, and Limit Properties:
======================================================

ByteBuffer:
-----------

Capacity: Total allocated byte size (read-only after constructor).
Length: Number of bytes currently considered valid/written (read-write, managed by Put... and Clear()). Starts at 0.
Position: Current byte read/write position (read-write). Starts at 0, incremented by PutByte, GetByte (sequential
versions). Reset to 0 by Clear() and Flip().
Limit: Boundary for read operations (read-write). Set to Capacity initially (or 0), set to Position by Flip().

ShortBuffer, IntBuffer, FloatBuffer:
------------------------------------

Capacity: Total capacity in units of their type (read-only, calculated from ByteBuffer.Capacity).
Length: Number of elements of their type currently considered valid/written (read-write, managed by Put... and
Clear()). Starts at 0.
No Position or Limit (in basic implementation): For simplicity, these type-specific buffers in our design primarily
rely on index-based access (GetShort(index), etc.) and Length for tracking data extent. You could potentially add
Position and Limit concepts to type-specific buffers for more advanced features if needed later, but for the core
implementation, focusing on index-based access and Length is sufficient.


IV. Endianness Handling:
========================

IsBigEndian Flag in ByteBuffer: A bool property in ByteBuffer controls endianness. Set in the constructor.
BinaryPrimitives for Type Conversions: ByteBuffer's GetShort, PutShort, GetInt, PutInt, GetFloat, PutFloat
methods use BinaryPrimitives.Read...Endian and BinaryPrimitives.Write...Endian methods to handle byte conversions,
dynamically choosing between Little-Endian and Big-Endian versions based on the IsBigEndian flag.
Endianness Inheritance: Type-specific buffers inherit the IsBigEndian setting from their underlying ByteBuffer.


V. Key Methods and Operations:
==============================

ByteBuffer Methods:
-------------------

PutByte(byte value) / GetByte() (and indexed versions if needed): Byte-level read/write. Update Position.
PutShort(int byteOffset, short value) / GetShort(int byteOffset): Type-specific operations at byte offsets using
BinaryPrimitives.
PutInt(int byteOffset, int value) / GetInt(int byteOffset): Type-specific operations at byte offsets using
BinaryPrimitives.
PutFloat(int byteOffset, float value) / GetFloat(int byteOffset): Type-specific operations at byte offsets using
BinaryPrimitives.
Resize(int extraCapacity): Dynamically increases capacity, preserves data, updates Capacity and potentially clamps
Position.
Clear(): Resets Position and Limit to 0.
Flip(): Sets Limit = Position, Position = 0.

Type-Specific Buffer Methods:
-----------------------------

PutShort(int index, short value) / GetShort(int index) (ShortBuffer): Type-safe operations on shorts, delegate to
ByteBuffer's PutShort/GetShort using index * sizeof(short) for byte offset. Update Length in PutShort.
PutInt(int index, int value) / GetInt(int index) (IntBuffer): Type-safe operations on ints, delegate to ByteBuffer's
PutInt/GetInt using index * sizeof(int) for byte offset. Update Length in PutInt.
PutFloat(int index, float value) / GetFloat(int index) (FloatBuffer): Type-safe operations on floats, delegate to
ByteBuffer's PutFloat/GetFloat using index * sizeof(float) for byte offset. Update Length in PutFloat.
Clear(): Resets Length to 0 and delegates Clear() to ByteBuffer.
Flip(): Delegates Flip() to ByteBuffer and optionally updates Length based on ByteBuffer.Limit.


VI. Constructors:
=================

ByteBuffer(int capacityInBytes, bool isBigEndian = false): Takes capacity in bytes and optional endianness.
ShortBuffer(int capacityInShorts): Takes capacity in number of shorts. Creates an underlying ByteBuffer with
appropriate byte capacity. Inherits endianness from a default or allows explicit setting if you enhance constructors.
IntBuffer(int capacityInInts): Takes capacity in number of ints. Creates underlying ByteBuffer with appropriate byte
capacity. Inherits endianness.
FloatBuffer(int capacityInFloats): Takes capacity in number of floats. Creates underlying ByteBuffer with appropriate
byte capacity. Inherits endianness.


VII. Error Handling:
====================

Argument Validation: Constructors and property setters validate capacity, position, limit, etc., to prevent invalid
values (ArgumentOutOfRangeException).
Bounds Checking: GetByte, GetShort, GetInt, GetFloat, PutByte, PutShort, PutInt, PutFloat methods perform bounds
checking to prevent IndexOutOfRangeException (or BufferOverflowException for Put operations when capacity is reached).
Potential Overflow Handling in Resize(): Check for integer overflow when calculating new capacity in Resize().


VIII. Efficiency and Type Safety:
=================================

Memory<T> and Span<T>: Utilize Memory<byte> and Span<byte> (and potentially casting to other Span<T>) for 
high-performance, zero-copy access and manipulation of the backing byte array.
Array.Copy(): Use Array.Copy() for efficient data copying in Resize().
BinaryPrimitives: Employ BinaryPrimitives for efficient and endianness-aware conversion between bytes and
primitive types.
Type-Specific Buffer Classes: ShortBuffer, IntBuffer, FloatBuffer enforce type safety at the API level,
preventing accidental misuse and ensuring methods work with the correct data types.


IX. Underlying Principles:
==========================

Delegation: Type-specific buffers delegate core byte storage and endianness handling to ByteBuffer.
Separation of Concerns: ByteBuffer handles low-level byte operations; type-specific buffers provide 
higher-level, type-focused APIs.
User-Friendliness: Constructors and methods are designed to be intuitive and work with capacities and indices 
in units natural to each buffer type (bytes for ByteBuffer, shorts, ints, floats for others).
Performance: Leverage C# features like Span<T>, Memory<T>, and optimized methods like Array.Copy() and 
BinaryPrimitives for efficient buffer operations.


Final Recommendation:
=====================

Review your code against this recap point by point. Ensure you have implemented all the key properties, methods,
and constructor behaviors described above. Pay close attention to endianness handling with BinaryPrimitives,
Length and Capacity management, delegation between buffer classes, and error handling.
Thorough unit testing is essential to verify the correctness and robustness of your buffer implementation.

If you've implemented all of these aspects, you should have a well-designed and functional set of C# buffer
classes that effectively mimic the core concepts of buffers found in other languages and libraries.
