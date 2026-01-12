using Unity.Collections;

namespace Core.Utilities
{
    /// <summary>
    /// Helpers for Unity.Collections FixedString types.
    /// Keep conversions centralized to avoid allocations and API drift.
    /// </summary>
    public static class FixedStringUtils
    {
        public static FixedString64Bytes FromInt(int value)
        {
            FixedString64Bytes fs = default;
            fs.Append(value);
            return fs;
        }
    }
}
