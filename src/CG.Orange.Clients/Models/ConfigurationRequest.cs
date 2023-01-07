﻿
namespace CG.Orange.Clients.Models
{
    /// <summary>
    /// This class represents a request for a configuration from the ORANGE 
    /// microservice.
    /// </summary>
    internal class ConfigurationRequest
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

        /// <summary>
        /// This property contains the optional client identifier for the caller.
        /// </summary>
        [MaxLength(200)]
        public string? ClientId { get; set; }

        #endregion
    }
}
