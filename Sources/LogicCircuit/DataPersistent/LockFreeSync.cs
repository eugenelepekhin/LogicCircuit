using System;

namespace LogicCircuit.DataPersistent {
	/// <summary>
	/// Implements lock-free synchronization primitives.
	/// </summary>
	/// <remarks>
	/// References:
	/// [1] Standard ECMA-334. C# Language Specification. 4th edition (June 2006).
	/// 	http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-334.pdf
	/// [2] Standard ECMA-335. Common Language Infrastructure (CLI). 4th edition (June 2006). Partition I.
	/// 	http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-335.pdf
	/// [3] Vance Morrison. Memory Models: Understand the Impact of Low-Lock Techniques in Multithreaded Apps, 2005.
	/// 	http://msdn.microsoft.com/en-us/magazine/cc163715.aspx
	/// [4] Intel® 64 and IA-32 Architectures Software Developer's Manual. Volume 3A: System Programming Guide, Part 1, Section 7.2.2
	/// 	(Memory Ordering in P6 and More Recent Processor Families).
	/// 	http://download.intel.com/design/processor/manuals/253668.pdf
	/// [5] _ReadWriteBarrier, _WriteBarrier, _ReadBarrier.
	/// 	http://msdn.microsoft.com/en-us/library/ms254271.aspx
	/// </remarks>
	internal static class LockFreeSync {
		private static volatile int volatileDummy;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
		private static int dummy;

		/// <summary>
		/// Forces all previous reads to complete before any subsequent read is started.
		/// </summary>
		/// <remarks>
		/// Similar to LFENCE Pentium instruction and _ReadBarrier in Visual C++.
		/// </remarks>
		public static void ReadBarrier() {
			ReadWriteBarrier();
		}

		/// <summary>
		/// Forces all previous writes to complete before any subsequent write is started.
		/// </summary>
		/// <remarks>
		/// Similar to SFENCE Pentium instruction and _WriteBarrier in Visual C++.
		/// </remarks>
		public static void WriteBarrier() {
			// .NET Framework 2.0 runtime memory model guarantees that "writes cannot move past
			// other writes from the same thread", so this is effectively a no-op [3]. Note that
			// the ECMA standard does not provide such a guarantee.
		}

		/// <summary>
		/// Forces all previous memory accesses to complete before any subsequent memory access is started.
		/// </summary>
		/// <remarks>
		/// Similar to MFENCE Pentium instruction and _ReadWriteBarrier in Visual C++.
		/// </remarks>
		public static void ReadWriteBarrier() {
			// Note: The order of the following two operations is important and cannot be reversed.
			// The operations are guaranteed to be executed in this order, because they reference
			// the same volatile field.
			// C# level:  [1, §10.10, Execution order]
			// CLI level: [2, §12.6.4, Optimization]
			// CPU level:
			//   Reads may be reordered with older writes to different locations but not with older
			//   writes to the same location [4].

			// A volatile write is guaranteed to happen AFTER all previous reads and writes.
			// C# level: [1, §17.4.3, Volatile fields]
			// CLI level: [2, §12.6.7, Volatile reads and writes][3].
			// CPU level:
			//   This is translated to a normal write on x86, because:
			//   1) Writes are not reordered with other writes (with some exceptions) [4].
			//   2) Writes are not reordered with older reads [4].
			volatileDummy = 0;

			// A volatile read is guaranteed to happen BEFORE all following reads and writes.
			// [1, §17.4.3, Volatile fields][2, §12.6.7, Volatile reads and writes][3].
			// This is translated to a normal read on x86, because:
			//   1) Reads are not reordered with other reads [4].
			//   2) Writes are not reordered with older reads [4].
			//
			// Note: If you read into a local variable, the operation will be optimized away
			// by the JITter. This is a bug, because "an optimizing compiler that converts CIL
			// to native code shall not remove any volatile operation" [2, §12.6.7, Volatile
			// reads and writes].
			dummy = volatileDummy;
		}
	}
}
