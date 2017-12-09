// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// ------------------------------------------------------------------------------
// Changes to this file must follow the http://aka.ms/api-review process.
// ------------------------------------------------------------------------------

namespace System.Runtime.CompilerServices
{
    public static partial class Unsafe
    {
        // Add new
        public static ref readonly T AddByteOffsetReadOnly<T>(in T source, System.IntPtr byteOffset) { throw null; }
        public static ref readonly T AddReadOnly<T>(in T source, int elementOffset) { throw null; }
        public static ref readonly T AddReadOnly<T>(in T source, System.IntPtr elementOffset) { throw null; }
        public static ref readonly T AsRefReadOnly<T>(in T source) { throw null; }
        public static ref readonly TTo AsReadOnly<TFrom, TTo>(in TFrom source) { throw null; }
        public static ref readonly T SubtractByteOffsetReadOnly<T>(in T source, System.IntPtr byteOffset) { throw null; }
        public static ref readonly T SubtractReadOnly<T>(in T source, int elementOffset) { throw null; }
        public static ref readonly T SubtractReadOnly<T>(in T source, System.IntPtr elementOffset) { throw null; }

        // Consider changing existing (problem this will not require `ref` when using do the `in` which makes it a bit weird
        public static bool AreSame<T>(in T left, in T right) { throw null; }
        public static System.IntPtr ByteOffset<T>(in T origin, in T target) { throw null; }
        public static void CopyBlock(ref byte destination, in byte source, uint byteCount) { }
        public static void CopyBlockUnaligned(ref byte destination, in byte source, uint byteCount) { }
        public unsafe static void Copy<T>(void* destination, in T source) { }
        public static T ReadUnaligned<T>(in byte source) { throw null; }

        public static ref T AddByteOffset<T>(ref T source, System.IntPtr byteOffset) { throw null; }
        public static ref T Add<T>(ref T source, int elementOffset) { throw null; }
        public unsafe static void* Add<T>(void* source, int elementOffset) { throw null; }
        public static ref T Add<T>(ref T source, System.IntPtr elementOffset) { throw null; }
        public static bool AreSame<T>(ref T left, ref T right) { throw null; }
        public unsafe static void* AsPointer<T>(ref T value) { throw null; }
        public unsafe static ref T AsRef<T>(void* source) { throw null; }
        public static ref T AsRef<T>(in T source) { throw null; }
        public static T As<T>(object o) where T : class { throw null; }
        public static ref TTo As<TFrom, TTo>(ref TFrom source) { throw null; }
        public static System.IntPtr ByteOffset<T>(ref T origin, ref T target) { throw null; }
        public static void CopyBlock(ref byte destination, ref byte source, uint byteCount) { }
        public unsafe static void CopyBlock(void* destination, void* source, uint byteCount) { }
        public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount) { }
        public unsafe static void CopyBlockUnaligned(void* destination, void* source, uint byteCount) { }
        public unsafe static void Copy<T>(void* destination, ref T source) { }
        public unsafe static void Copy<T>(ref T destination, void* source) { }
        public static void InitBlock(ref byte startAddress, byte value, uint byteCount) { }
        public unsafe static void InitBlock(void* startAddress, byte value, uint byteCount) { }
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount) { }
        public unsafe static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount) { }
        public unsafe static T Read<T>(void* source) { throw null; }
        public unsafe static T ReadUnaligned<T>(void* source) { throw null; }
        public static T ReadUnaligned<T>(ref byte source) { throw null; }
        public static int SizeOf<T>() { throw null; }
        public static ref T SubtractByteOffset<T>(ref T source, System.IntPtr byteOffset) { throw null; }
        public static ref T Subtract<T>(ref T source, int elementOffset) { throw null; }
        public unsafe static void* Subtract<T>(void* source, int elementOffset) { throw null; }
        public static ref T Subtract<T>(ref T source, System.IntPtr elementOffset) { throw null; }
        public unsafe static void Write<T>(void* destination, T value) { }
        public unsafe static void WriteUnaligned<T>(void* destination, T value) { }
        public static void WriteUnaligned<T>(ref byte destination, T value) { }
    }
}
