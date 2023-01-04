
namespace CG.Orange.Clients.Models;

/// <summary>
/// This class represents a request to log into the ORANGE configuration
/// microservice, using client credentials.
/// </summary>
internal class ClientLoginRequest
{
    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the client identifier for the request.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// This property contains the client secret for the request.
    /// </summary>
    [MaxLength(64)]
    public string ClientSecret { get; set; } = null!;

    #endregion
}

