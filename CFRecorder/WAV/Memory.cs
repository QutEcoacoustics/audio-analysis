using System;
using System.Runtime.InteropServices;

namespace CFRecorder
{
    /// <summary>
    /// Contains methods for allocating and freeing memory.
    /// </summary>
    public class Memory
    {
        /// <summary>
        /// Allocates fixed memory. The return value is a pointer to the memory object.
        /// </summary>
        public const uint LMEM_FIXED = 0x0000;
        /// <summary>
        /// Initializes memory contents to zero.
        /// </summary>
        public const uint LMEM_ZEROINIT = 0x0040;
        /// <summary>
        /// Enables the memory allocation to move if it cannot be allocated in place.
        /// </summary>
        public const uint LMEM_MOVEABLE = 0x0002;
        /// <summary>
        /// Allows modification of the attributes of the memory object.
        /// </summary>
        public const uint LMEM_MODIFY = 0x0080;

        /// <summary>
        /// This function allocates the specified number of bytes from the heap. In
        /// the linear Microsoft® Windows® CE application programming interface (API)
        /// environment, there is no difference between the local heap and the global
        /// heap.
        /// </summary>
        /// <param name="uFlags">[in] Specifies how to allocate memory. If zero is
        /// specified, the default is the LMEM_FIXED flag. This parameter is a combination
        /// of LMEM_FIXED and LMEM_ZEROINIT. </param>
        /// <param name="uBytes">[in] Specifies the number of bytes to allocate.</param>
        /// <returns>A handle to the newly allocated memory object indicates success.
        /// NULL indicates failure. To get extended error information,
        /// call GetLastError.</returns>
        [DllImport("coredll.dll")]
        extern public static IntPtr LocalAlloc(uint uFlags, uint uBytes);

        /// <summary>
        /// This function frees the specified local memory object and invalidates its handle.
        /// </summary>
        /// <param name="hMem">Handle to the local memory object. This handle is returned
        /// by either the LocalAlloc or LocalReAlloc function.</param>
        /// <returns>NULL indicates success. A handle to the local memory object indicates
        /// failure. To get extended error information, call GetLastError.</returns>
        [DllImport("coredll.dll")]
        extern public static IntPtr LocalFree(IntPtr hMem);

        /// <summary>
        /// This function changes the size or the attributes of a specified local
        /// memory object. The size can increase or decrease.
        /// </summary>
        /// <param name="hMem">[in] Handle to the local memory object to be reallocated.
        /// This handle is returned by either the LocalAlloc or LocalReAlloc function.</param>
        /// <param name="uBytes">[in] New size, in bytes, of the memory block. If fuFlags
        /// specifies the LMEM_MODIFY flag, this parameter is ignored.</param>
        /// <param name="fuFlags">[in] Flag that specifies how to reallocate the local
        /// memory object. If the LMEM_MODIFY flag is specified, this parameter modifies
        /// the attributes of the memory object, and the uBytes parameter is ignored.
        /// Otherwise, this parameter controls the reallocation of the memory object.
        /// The LMEM_MODIFY can be combined with LMEM_MOVEABLE.
        /// If this parameter does not specify LMEM_MODIFY, this parameter can be any
        /// combination of LMEM_MOVEABLE and LMEM_ZEROINIT.</param>
        /// <returns>A handle to the reallocated memory object indicates success.
        /// NULL indicates failure. To get extended error information, call
        /// GetLastError.</returns>
        [DllImport("coredll.dll")]
        extern public static IntPtr LocalReAlloc(IntPtr hMem, uint uBytes, uint fuFlags);

        /// <summary>
        /// Run a test of the Memory class.
        /// </summary>
        /// <param name="showLine">Delegate called to show debug information</param>


    }
}
