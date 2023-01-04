
namespace CG.Orange.Clients.Sources
{
    /// <summary>
    /// This interface represent a data source that reads from the 
    /// ORANGE configuration microservice.
    /// </summary>
    public interface IOrangeConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// This property contains the options for the source.
        /// </summary>
        ConfigurationOptions Options { get; }
    }
}
