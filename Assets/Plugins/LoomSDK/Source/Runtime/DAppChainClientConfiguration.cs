namespace Loom.Client
{
    public sealed class DAppChainClientConfiguration
    {
        /// <summary>
        /// Specifies the amount of time after which a call will time out.
        /// </summary>
        public int CallTimeout { get; set; } = 5000;

        /// <summary>
        /// Specifies the amount of time after which a static will time out.
        /// </summary>
        public int StaticCallTimeout { get; set; } = 5000;

        /// <summary>
        /// Whether clients will attempt to connect automatically when in Disconnected state
        /// before communicating.
        /// </summary>
        public bool AutoReconnect { get; set; } = true;
    }
}
