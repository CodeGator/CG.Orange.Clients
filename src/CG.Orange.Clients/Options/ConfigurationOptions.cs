
namespace CG.Orange.Clients.Options;

/// <summary>
/// This class represent options for reading from the ORANGE configuration
/// microservice.
/// </summary>
public class ConfigurationOptions 
{
    // *******************************************************************
    // Fields.
    // *******************************************************************

    #region Fields

    /// <summary>
    /// This field backs the <see cref="ConfigurationOptions.Url"/> property.
    /// </summary>
    internal protected string _url = null!;

    #endregion

    // *******************************************************************
    // Properties.
    // *******************************************************************

    #region Properties

    /// <summary>
    /// This property contains the endpointL for the ORANGE configuration 
    /// microservice.
    /// </summary>
    [Required]
    [Url]
    public string Url 
    {
        get { return _url; }
        set
        {
            if (!value.Trim().EndsWith("/"))
            {
                _url = $"{value.Trim()}/";
            }
            else
            {
                _url = value.Trim();
            }
        } 
    }

    /// <summary>
    /// This property contains the application name to use for the connection.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string Application { get; set; } = null!;

    /// <summary>
    /// This property contains an optional environment name to use for 
    /// the connection.
    /// </summary>
    [MaxLength(64)]
    public string? Environment { get; set; }

    /// <summary>
    /// This property contains the client identifier to use for the connection.
    /// </summary>
    [Required]
    [MaxLength(64)]
    public string ClientId { get; set; } = null!;

    /// <summary>
    /// This property contains the client secret to use for the connection.
    /// </summary>
    [MaxLength(64)]
    public string ClientSecret { get; set; } = null!;

    /// <summary>
    /// This property indicates whether the provider should watch for changes
    /// in the data and automatically reload whenever anything changes.
    /// </summary>
    public bool ReloadOnChange { get; set; }

    /// <summary>
    /// This property contains the timeout value to use for the connection.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// This property contains a logger for the connection.
    /// </summary>
    public ILogger? Logger { get; set; }

    #endregion

    // *******************************************************************
    // Constructors.
    // *******************************************************************

    #region Constructors

    /// <summary>
    /// This constructor creates a new instance of the <see cref="ConfigurationOptions"/>
    /// class.
    /// </summary>
    public ConfigurationOptions()
    {
        // Create any defaults.
        Url = "https://localhost:7145/";
    }

    #endregion
}
