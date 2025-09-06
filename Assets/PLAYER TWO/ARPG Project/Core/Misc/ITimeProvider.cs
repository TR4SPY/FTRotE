using System;

namespace PLAYERTWO.ARPGProject
{
    /// <summary>
    /// Provides access to the current UTC time.
    /// </summary>
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// Default time provider returning <see cref="DateTime.UtcNow"/>.
    /// </summary>
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
