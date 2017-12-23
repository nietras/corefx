﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if !netstandard
using Internal.Runtime.CompilerServices;
#endif

namespace System
{
    internal static partial class SpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Sort<T, TComparer>(
            this Span<T> span, TComparer comparer)
            where TComparer : IComparer<T>
        {
            //if (comparer == null)
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.comparer);

            ArraySortHelper<T, TComparer>.Default.Sort(span, comparer);
        }

        internal static class SpanSortHelper<T, TComparer>
            where TComparer : IComparer<T>
        {
            internal static void Sort(ref T spanStart, int length, TComparer comparer)
            {
                int lo = 0;
                int hi = length - 1;
                // If length == 0, hi == -1, and loop will not be entered
                while (lo <= hi)
                {
                    // PERF: `lo` or `hi` will never be negative inside the loop,
                    //       so computing median using uints is safe since we know 
                    //       `length <= int.MaxValue`, and indices are >= 0
                    //       and thus cannot overflow an uint. 
                    //       Saves one subtraction per loop compared to 
                    //       `int i = lo + ((hi - lo) >> 1);`
                    int i = (int)(((uint)hi + (uint)lo) >> 1);

                    // TODO: We probably need to add `ref readonly`/`in` methods e.g. `AddReadOnly` to unsafe
                    //int c = comparable.CompareTo(Unsafe.Add(ref spanStart, i));
                    //if (c == 0)
                    //{
                    //    return i;
                    //}
                    //else if (c > 0)
                    //{
                    //    lo = i + 1;
                    //}
                    //else
                    //{
                    //    hi = i - 1;
                    //}
                }
                // If none found, then a negative number that is the bitwise complement
                // of the index of the next element that is larger than or, if there is
                // no larger element, the bitwise complement of `length`, which
                // is `lo` at this point.
                //return ~lo;
            }


            internal static void IntrospectiveSort(ref T spanStart, int length, TComparer comparer)
            {
                if (length < 2)
                    return;

                var depthLimit = 2 * IntrospectiveSortUtilities.FloorLog2PlusOne(length);
                IntroSort(ref spanStart, 0, length - 1, depthLimit, comparer);
            }

            private static void IntroSort(ref T keys, int lo, int hi, int depthLimit, TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                while (hi > lo)
                {
                    int partitionSize = hi - lo + 1;
                    if (partitionSize <= IntrospectiveSortUtilities.IntrosortSizeThreshold)
                    {
                        if (partitionSize == 1)
                        {
                            return;
                        }
                        if (partitionSize == 2)
                        {
                            // No indeces equal here!
                            SwapIfGreater(ref keys, comparer, lo, hi);
                            return;
                        }
                        if (partitionSize == 3)
                        {
                            // No indeces equal here! Many indeces can be reused here...
                            SwapIfGreater(ref keys, comparer, lo, hi - 1);
                            SwapIfGreater(ref keys, comparer, lo, hi);
                            SwapIfGreater(ref keys, comparer, hi - 1, hi);
                            return;
                        }

                        InsertionSort(ref keys, lo, hi, comparer);
                        return;
                    }

                    if (depthLimit == 0)
                    {
                        Heapsort(ref keys, lo, hi, comparer);
                        return;
                    }
                    depthLimit--;

                    // We should never reach here, unless > 3 elements due to partition size
                    int p = PickPivotAndPartition(ref keys, lo, hi, comparer);
                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
                    IntroSort(ref keys, p + 1, hi, depthLimit, comparer);
                    hi = p - 1;
                }
            }

            private static int PickPivotAndPartition(ref T keys, int lo, int hi, TComparer comparer)
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three.  But also partition them, since we've done the comparison.
                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int i = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                SwapIfGreater(ref keys, comparer, lo, middle);  // swap the low with the mid point
                SwapIfGreater(ref keys, comparer, lo, hi);   // swap the low with the high
                SwapIfGreater(ref keys, comparer, middle, hi); // swap the middle with the high

                ref var pivot = ref Unsafe.Add(ref keys, middle);
                // Swap in different way
                Swap(ref keys, middle, hi - 1);
                int left = lo, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

                while (left < right)
                {
                    // TODO: Would be good to update local ref here
                    while (comparer.Compare(Unsafe.Add(ref keys, ++left), pivot) < 0)
                        ;
                    // TODO: Would be good to update local ref here
                    while (comparer.Compare(pivot, Unsafe.Add(ref keys, --right)) < 0)
                        ;

                    if (left >= right)
                        break;

                    // Indeces cannot be equal here
                    Swap(ref keys, left, right);
                }

                // Put pivot in the right location.
                Swap(ref keys, left, (hi - 1));
                return left;
            }

            private static void Heapsort(ref T keys, int lo, int hi, TComparer comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                int n = hi - lo + 1;
                for (int i = n / 2; i >= 1; --i)
                {
                    DownHeap(ref keys, i, n, lo, comparer);
                }
                for (int i = n; i > 1; --i)
                {
                    Swap(ref keys, lo, lo + i - 1);
                    DownHeap(ref keys, 1, i - 1, lo, comparer);
                }
            }

            private static void DownHeap(ref T keys, int i, int n, int lo, TComparer comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);

                ref T d = ref Unsafe.Add(ref keys, lo + i - 1);
                T v = d;
                int child;
                while (i <= n / 2)
                {
                    child = 2 * i;
                    // TODO: Local ref updates needed
                    //ref var l = ref Unsafe.Add(ref keys, lo + child - 1);
                    //ref var r = ref Unsafe.Add(ref keys, lo + child);
                    if (child < n && 
                        comparer.Compare(Unsafe.Add(ref keys, lo + child - 1), 
                            Unsafe.Add(ref keys, lo + child)) < 0)
                    {
                        child++;
                    }
                    ref T c = ref Unsafe.Add(ref keys, lo + child - 1);
                    if (!(comparer.Compare(d, c) < 0))
                        break;
                    //keys[lo + i - 1] = keys[lo + child - 1];
                    d = c;
                    i = child;
                }
                //keys[lo + i - 1] = d;
                d = v;
            }

            private static void InsertionSort(T[] keys, int lo, int hi, Comparison<T> comparer)
            {
                Debug.Assert(keys != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi >= lo);
                Debug.Assert(hi <= keys.Length);

                int i, j;
                T t;
                for (i = lo; i < hi; i++)
                {
                    j = i;
                    t = keys[i + 1];
                    while (j >= lo && comparer(t, keys[j]) < 0)
                    {
                        keys[j + 1] = keys[j];
                        j--;
                    }
                    keys[j + 1] = t;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SwapIfGreater(ref T start, TComparer comparer, int i, int j)
            {
                // TODO: Is the a!=b check necessary? Most cases not needed?
                if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref start, i);
                    ref var jElement = ref Unsafe.Add(ref start, j);
                    if (comparer.Compare(iElement, jElement) > 0)
                    {
                        T temp = iElement;
                        iElement = jElement;
                        jElement = temp;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void Swap(ref T start, int i, int j)
            {
                if (i != j)
                {
                    ref var iElement = ref Unsafe.Add(ref start, i);
                    ref var jElement = ref Unsafe.Add(ref start, j);
                    T temp = iElement;
                    iElement = jElement;
                    jElement = temp;
                }
            }
        }

        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct ComparableComparer<T> : IComparer<T>
            where T : IComparable<T>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => x.CompareTo(y);
        }
        // Helper to allow sharing all code via IComparer<T> inlineable
        internal struct ComparisonComparer<T> : IComparer<T>
        {
            readonly Comparison<T> m_comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                m_comparison = comparison;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Compare(T x, T y) => m_comparison(x, y);
        }

        // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
        internal interface IArraySortHelper<TKey, TComparer>
            where TComparer : IComparer<TKey>
        {
            void Sort(Span<TKey> keys, in TComparer comparer);
            //int BinarySearch(Span<TKey> keys, TKey value, IComparer<TKey> comparer);
        }

        internal static class IntrospectiveSortUtilities
        {
            // https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/ArraySortHelper.cs
            // https://github.com/dotnet/coreclr/blob/master/src/classlibnative/bcltype/arrayhelpers.cpp

            // This is the threshold where Introspective sort switches to Insertion sort.
            // Empirically, 16 seems to speed up most cases without slowing down others, at least for integers.
            // Large value types may benefit from a smaller number.
            internal const int IntrosortSizeThreshold = 16;

            internal static int FloorLog2PlusOne(int n)
            {
                int result = 0;
                while (n >= 1)
                {
                    result++;
                    n = n / 2;
                }
                return result;
            }
        }

        internal class ArraySortHelper<T, TComparer>
            : IArraySortHelper<T, TComparer>
            where TComparer : IComparer<T>
        {
            private static volatile IArraySortHelper<T, TComparer> defaultArraySortHelper;

            public static IArraySortHelper<T, TComparer> Default
            {
                get
                {
                    IArraySortHelper<T, TComparer> sorter = defaultArraySortHelper;
                    if (sorter == null)
                        sorter = CreateArraySortHelper();

                    return sorter;
                }
            }

            private static IArraySortHelper<T, TComparer> CreateArraySortHelper()
            {
                if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                {
                    defaultArraySortHelper = (IArraySortHelper<T, TComparer>)
                        RuntimeTypeHandle.Allocate(
                            typeof(GenericArraySortHelper<string, TComparer>).TypeHandle.Instantiate(new Type[] { typeof(T), typeof(TComparer) }));
                }
                else
                {
                    defaultArraySortHelper = new ArraySortHelper<T, TComparer>();
                }
                return defaultArraySortHelper;
            }

            public void Sort(Span<T> keys, TComparer comparer)
            {
                // Add a try block here to detect IComparers (or their
                // underlying IComparables, etc) that are bogus.
                try
                {
                    if (typeof(TComparer) == typeof(IComparer<T>) && comparer == null)
                    {
                        Sort<T, IComparer<T>>(ref keys.DangerousGetPinnableReference(), keys.Length, Comparer<T>.Default);
                    }
                    else
                    {
                        Sort<T, TComparer>(ref keys.DangerousGetPinnableReference(), keys.Length, comparer);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    //IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
                }
                catch (Exception e)
                {
                    throw e;
                    //throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
                }
            }

            //public int BinarySearch(Span<T> array, T value, TComparer comparer)
            //{
            //    try
            //    {
            //        if (comparer == null)
            //        {
            //            comparer = Comparer<T>.Default;
            //        }

            //        return InternalBinarySearch(array, index, length, value, comparer);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
            //    }
            //}

            //internal static void Sort(Span<T> keys, Comparison<T> comparer)
            //{
            //    Debug.Assert(keys != null, "Check the arguments in the caller!");
            //    Debug.Assert(index >= 0 && length >= 0 && (keys.Length - index >= length), "Check the arguments in the caller!");
            //    Debug.Assert(comparer != null, "Check the arguments in the caller!");

            //    // Add a try block here to detect bogus comparisons
            //    try
            //    {
            //        IntrospectiveSort(keys, index, length, comparer);
            //    }
            //    catch (IndexOutOfRangeException)
            //    {
            //        IntrospectiveSortUtilities.ThrowOrIgnoreBadComparer(comparer);
            //    }
            //    catch (Exception e)
            //    {
            //        throw new InvalidOperationException(SR.InvalidOperation_IComparerFailed, e);
            //    }
            //}

            //internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
            //{
            //    Debug.Assert(array != null, "Check the arguments in the caller!");
            //    Debug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            //    int lo = index;
            //    int hi = index + length - 1;
            //    while (lo <= hi)
            //    {
            //        int i = lo + ((hi - lo) >> 1);
            //        int order = comparer.Compare(array[i], value);

            //        if (order == 0)
            //            return i;
            //        if (order < 0)
            //        {
            //            lo = i + 1;
            //        }
            //        else
            //        {
            //            hi = i - 1;
            //        }
            //    }

            //    return ~lo;
            //}

        }

    }
}
