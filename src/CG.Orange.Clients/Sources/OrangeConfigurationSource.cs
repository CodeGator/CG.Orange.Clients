
namespace CG.Orange.Clients.Sources
{
    /// <summary>
    /// This class is a default implementation of the <see cref="IOrangeConfigurationSource"/>
    /// interface.
    /// </summary>
    internal class OrangeConfigurationSource : IOrangeConfigurationSource
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains the options for the source.
        /// </summary>
        public ConfigurationOptions Options { get; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="OrangeConfigurationSource"/>
        /// class.
        /// </summary>
        /// <param name="options">The options to use for the source.</param>
        public OrangeConfigurationSource(
            ConfigurationOptions options
            )
        {
            // Validate the parameters before attempting to use them.
            Guard.Instance().ThrowIfNull(options, nameof(options));

            // Save the references.
            Options = options;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method is called by the builder to create a provider.
        /// </summary>
        /// <param name="builder">The builder to use for the operation.</param>
        /// <returns>An <see cref="IConfigurationProvider"/> object.</returns>
        public IConfigurationProvider Build(
            IConfigurationBuilder builder
            )
        {
            // Validate the arguments before attempting to use them.
            Guard.Instance().ThrowIfNull(builder, nameof(builder));

            // Create the provider.
            return new OrangeConfigurationProvider(this);
        }

        #endregion
    }
}
