
namespace CG.Orange.Clients.Models
{
    /// <summary>
    /// This class represents a request for settings to the ORANGE configuration
    /// microservice from the <see cref="OrangeConfigurationProvider"/> provider.
    /// </summary>
    internal class SettingsRequest
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains the application for the request.
        /// </summary>
        [Required]
        public string Application { get; set; } = null!;

        /// <summary>
        /// This property contains the optional environment for the request.
        /// </summary>
        [Required]
        public string? Environment { get; set; }

        #endregion
    }
}
