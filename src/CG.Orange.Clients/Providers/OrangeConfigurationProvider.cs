
namespace CG.Orange.Clients.Providers
{
    /// <summary>
    /// This class represents a custom provider for reading remote configuration 
    /// settings from the ORANGE configuration microservice.
    /// </summary>
    internal class OrangeConfigurationProvider : ConfigurationProvider
    {
        // *******************************************************************
        // Properties.
        // *******************************************************************

        #region Properties

        /// <summary>
        /// This property contains a reference to the provider's source.
        /// </summary>
        internal protected IOrangeConfigurationSource Source { get; } = null!;

        /// <summary>
        /// This property contains an optional reference to a signalR hub.
        /// </summary>
        internal protected HubConnection? Hub { get; set; }

        #endregion

        // *******************************************************************
        // Constructors.
        // *******************************************************************

        #region Constructors

        /// <summary>
        /// This constructor creates a new instance of the <see cref="OrangeConfigurationProvider"/>
        /// class.
        /// </summary>
        /// <param name="source">The configuration source to use for the provider.</param>
        public OrangeConfigurationProvider(
            IOrangeConfigurationSource source
            )
        {
            // Validate the arguments before attempting to use them.
            Guard.Instance().ThrowIfNull(source, nameof(source));

            // Save the reference.
            Source = source;
        }

        #endregion

        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method loads (or reloads) the data for the provider.
        /// </summary>
        public override void Load()
        {
#if DEBUG
            // Start the stopwatch.
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Entering the {name} method.",
                    nameof(Load)
                    );

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Clearing the data collection."
                    );

                // Clear any cached data.
                Data.Clear();

                // Log what we are about to do.
                Source.Options.Logger?.LogInformation(
                    "Logging into the configuration microservice."
                    );

                // Log into the microservice.
                var accessToken = FetchAccessToken();

                // Did we succeed?
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogInformation(
                        "Fetching settings for application: {app}, environment: {env}",
                        Source.Options.Application,
                        Source.Options.Environment
                        );

                    // Fetch the settings.
                    var data = FetchSettings(accessToken);

                    // Did we return anything?
                    if (data is not null)
                    {
                        // Log what we are about to do.
                        Source.Options.Logger?.LogDebug(
                            "Adding {count} settings to the collection.",
                            data.Length
                            );

                        // Loop through the values.
                        foreach (var kvp in data)
                        {
                            Data.Add(kvp);
                        }
                    }
                }
                else
                {
                    // Log what happened.
                    Source.Options.Logger?.LogWarning(
                        "Failed to login into the configuration microservice!"
                        );
                }

                // Ensure the back channel is open.
                EnsureBackChannel();

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Leaving the {name} method.",
                    nameof(Load)
                    );
            }
            catch (Exception ex)
            {
                // Log what happened.
                Source.Options.Logger?.LogError(
                    ex,
                    "Failed to load remote settings!"
                    );

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Clearing the data collection."
                    );

                // Clear any data.
                Data.Clear();
            }
            finally
            {
#if DEBUG
                // Stop the stopwatch.
                stopWatch.Stop();

                // Log how long it took
                Source.Options.Logger?.LogDebug(
                    "Load timing: {time}",
                    stopWatch.Elapsed
                    );
#endif
            }
        }

        #endregion

        // *******************************************************************
        // Private methods.
        // *******************************************************************

        #region Private methods

        /// <summary>
        /// This method is called to process change notifications from the 
        /// microservice's SignalR back-channel.
        /// </summary>
        /// <param name="application">The application for the setting.</param>
        /// <param name="environment">The optional environment for the setting.</param>
        private void OnNotification(
            string application,
            string? environment
            )
        {
            // Log what happened.
            Source.Options.Logger?.LogDebug(
                "{name} called for application: {app}, environment: {env}",
                nameof(OnNotification),
                application,
                environment
                );

            var shouldReload = false;

            // We only care if the application matches.
            if (Source.Options.Application == application)
            {
                // Should we also match environments?
                if (Source.Options.Environment is not null)
                {
                    // We only care if the environment matches.
                    if (Source.Options.Environment == environment)
                    {
                        shouldReload = true; // Reload the data.
                    }
                }
                else
                {
                    shouldReload = true; // Reload the data.
                }
            }

            // Should we reload the data?
            if (shouldReload)
            {
                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Reloading data from the {name} callback",
                    nameof(OnNotification)
                    );

                // Load the provider's data.
                Load();
            }

            // Reset any associated change tokens.
            base.OnReload();
        }

        // *******************************************************************

        /// <summary>
        /// This method fetches an access token for the remote configuration
        /// microservice.
        /// </summary>
        /// <returns>An access token for the microservice.</returns>
        private string FetchAccessToken()
        {
#if DEBUG
            // Start the stopwatch.
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating a retry policy"
                    );

                // Create a retry policy, with exponential backoff.
                var policy = HttpPolicyExtensions.HandleTransientHttpError()
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        );

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating an HTTP client"
                    );

                // Create the HTTP client.
                using var client = new HttpClient()
                {
                    // Set the base address.
                    BaseAddress = new Uri(Source.Options.Url),
                };

                // Should we set a timeout?
                if (Source.Options.Timeout is not null)
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogDebug(
                        "Setting the timeout to: {to}",
                        Source.Options.Timeout.Value
                        );

                    // Set the timeout.
                    client.Timeout = Source.Options.Timeout.Value;
                }

                // Use the account endpoint.
                var url = $"api/account/login/client";

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating a JSON payload for the POST to: {url}",
                    url
                    );

                // Create json for the body.
                var jsonContent = JsonContent.Create(
                    new ClientLoginRequest()
                    {
                        ClientId = Source.Options.ClientId,
                        ClientSecret = Source.Options.ClientSecret
                    },
                    options: new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                var retryCount = 1;

                // Perform the login.
                var response = policy.ExecuteAsync(async () =>
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogDebug(
                        "POSTing to: {url}. Attempt# {retry}",
                        url,
                        retryCount++
                        );

                    // Send the POST.
                    var response = await client.PostAsync(
                        url,
                        jsonContent
                        ).ConfigureAwait(false);

                    // Log what happened.
                    Source.Options.Logger?.LogDebug(
                        "{url} returned {code}",
                        url,
                        response.StatusCode
                        );

                    // Did we fail?
                    response.EnsureSuccessStatusCode();

                    // Return the result.
                    return response;
                }).Result;

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Fetching the JSON from the remote service."
                    );

                // Read the remote token.
                var json = response.Content.ReadAsStringAsync().Result;

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Parsing the JSON from {url}",
                    url
                    );

                // Parse the JSON.
                var parsed = JsonHelper.DeserializeAnonymousType(
                    json ?? "",
                    new
                    {
                        access_token = "",
                        expires_in = 0,
                        token_type = "",
                        refresh_token = "",
                        scope = ""
                    });

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Sanity checking the auth token."
                    );

                // Sanity check the results.
                if (parsed is null ||
                    parsed.token_type != "Bearer" ||
                    parsed.expires_in == 0 ||
                    !parsed.scope.Contains("cfg-svc-read"))
                {
                    // No token for you!
                    return "";
                }

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Returning the access token."
                    );

                // Return the access token.
                return parsed.access_token;
            }
            finally
            {
#if DEBUG
                // Stop the stopwatch.
                stopWatch.Stop();

                // Log how long it took
                Source.Options.Logger?.LogDebug(
                    "Access token timing: {time}",
                    stopWatch.Elapsed
                    );
#endif
            }
        }

        // *******************************************************************

        /// <summary>
        /// This method fetches settings from the remote configuration microservice.
        /// </summary>
        /// <param name="accessToken">The access token to use for the operation.</param>
        private KeyValuePair<string, string?>[] FetchSettings(
            string accessToken
            )
        {
#if DEBUG
            // Start the stopwatch.
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Checking for the access token"
                    );

                // Is the access token missing?
                if (string.IsNullOrEmpty(accessToken))
                {
                    // Log what happened.
                    Source.Options.Logger?.LogWarning(
                        "No access token found! Unable to fetch remote settings!"
                        );

                    // No data.
                    return Array.Empty<KeyValuePair<string, string?>>();
                }

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating a retry policy"
                    );

                // Create a retry policy, with exponential backoff.
                var policy = HttpPolicyExtensions.HandleTransientHttpError()
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        );

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating an HTTP client"
                    );

                // Create the HTTP client.
                using var client = new HttpClient()
                {
                    // Set the base address.
                    BaseAddress = new Uri(Source.Options.Url),
                };

                // Should we set a timeout?
                if (Source.Options.Timeout is not null)
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogDebug(
                        "Setting the timeout to: {to}",
                        Source.Options.Timeout.Value
                        );

                    // Set the timeout.
                    client.Timeout = Source.Options.Timeout.Value;
                }

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Setting the bearer token on the HTTP client"
                    );

                // Add the bearer token.
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer", accessToken
                    );

                // Use the settings endpoint.
                var url = $"api/settings";

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Creating a JSON payload for the POST to: {url}",
                    url
                    );

                // Create json for the body.
                var jsonContent = JsonContent.Create(
                    new SettingsRequest()
                    {
                        Application = Source.Options.Application,
                        Environment = Source.Options.Environment
                    },
                    options: new JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                var retryCount = 1;

                // Fetch the settings.
                var response = policy.ExecuteAsync(async () =>
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogDebug(
                        "POSTing to: {url}. Attempt# {retry}",
                        url,
                        retryCount++
                        );

                    // Send the POST.
                    var response = await client.PostAsync(
                        url,
                        jsonContent
                        ).ConfigureAwait(false);

                    // Log what happened.
                    Source.Options.Logger?.LogDebug(
                        "{url} returned {code}",
                        url,
                        response.StatusCode
                        );

                    // Did we fail?
                    response.EnsureSuccessStatusCode();

                    // Return the result.
                    return response;
                }).Result;

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Fetching the JSON from {url}.",
                    url
                    );

                // Is the return media type wonky?
                if (response.Content.Headers.ContentType?.MediaType != MediaTypeNames.Application.Json)
                {
                    // Might be nice to know why we're returning no settings ...

                    // Log what happened.
                    Source.Options.Logger?.LogWarning(
                        "Unexpected return type from: {url}! Expected {t1}, found {t2}!",
                        url,
                        MediaTypeNames.Application.Json,
                        response.Content.Headers.ContentType?.MediaType
                        );
                }

                // Read the remote settings.
                var settings = response.Content.ReadFromJsonAsync<
                    KeyValuePair<string, string?>[]
                    >().Result;

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Returning {count} settings.",
                    settings?.Length
                    );

                // Return the results.
                return settings ?? Array.Empty<KeyValuePair<string, string?>>();
            }
            finally
            {
#if DEBUG
                // Stop the stopwatch.
                stopWatch.Stop();

                // Log how long it took
                Source.Options.Logger?.LogDebug(
                    "Fetch setting timing: {time}",
                    stopWatch.Elapsed
                    );
#endif
            }
        }

        // *******************************************************************

        /// <summary>
        /// This method starts a backchannel for change notifications, using a
        /// SignalR connection back to the remote configuration microservice.
        /// </summary>
        private void EnsureBackChannel()
        {
#if DEBUG
            // Start the stopwatch.
            var stopWatch = new Stopwatch();
            stopWatch.Start();
#endif
            try
            {
                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Checking for the {flag} options flag",
                    nameof(Source.Options.ReloadOnChange)
                    );

                // Should we reload on changes?
                if (Source.Options.ReloadOnChange)
                {
                    // Log what we are about to do.
                    Source.Options.Logger?.LogDebug(
                        "Checking for an existing SignalR hub"
                        );

                    // Should we create a SignalR hub?
                    if (Hub is null)
                    {
                        // Log what we are about to do.
                        Source.Options.Logger?.LogDebug(
                            "Creating a retry policy"
                            );

                        // Create a retry policy, with exponential backoff.
                        var policy = HttpPolicyExtensions.HandleTransientHttpError()
                            .WaitAndRetryAsync(3, retryAttempt =>
                                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                );

                        // Format the address.
                        var address = new Uri(
                            $"{Source.Options.Url}_backchannel"
                            );

                        // Log what we are about to do.
                        Source.Options.Logger?.LogDebug(
                            "Creating a SignalR hub connection builder"
                            );

                        // Create a signalR hub builder.
                        var builder = new HubConnectionBuilder()
                            .WithUrl(address)
                            .WithAutomaticReconnect();

                        // Log what we are about to do.
                        Source.Options.Logger?.LogDebug(
                            "Creating a SignalR hub at: {url}",
                            address
                            );

                        // Create the signalR hub.
                        Hub = builder.Build();

                        // Log what we are about to do.
                        Source.Options.Logger?.LogDebug(
                            "Wiring up a handler for the ChangedSetting event"
                            );

                        // Wire up a back-channel handler.
                        Hub.On(
                            "ChangedSetting",
                            (string application, string? environment) =>
                            {
                                // Log what we are about to do.
                                Source.Options.Logger?.LogInformation(
                                    "Detected the ChangedSetting event!"
                                    );

                                // Something changed!
                                OnNotification(
                                    application,
                                    environment
                                    );
                            });

                        var retryCount = 1;

                        // Startup SignalR.
                        policy.ExecuteAsync(async () =>
                        {
                            // Log what we are about to do.
                            Source.Options.Logger?.LogDebug(
                                "Starting the SignalR hub at: {url}. Attempt# {retry}",
                                address,
                                retryCount++
                                );

                            // Start it baby!
                            await Hub.StartAsync();

                            // Return success.
                            return new HttpResponseMessage(
                                System.Net.HttpStatusCode.OK
                                );
                        }).Wait();
                    }
                    else
                    {
                        // Log what we didn't do.
                        Source.Options.Logger?.LogInformation(
                            "An existing SignalR hub was found"
                            );
                    }
                }
                else
                {
                    // Log what we didn't do.
                    Source.Options.Logger?.LogInformation(
                        "Not standing up a back channel because the {flag} options flag is false",
                        nameof(Source.Options.ReloadOnChange)
                        );
                }
            }
            finally
            {
#if DEBUG
                // Stop the stopwatch.
                stopWatch.Stop();

                // Log what we are about to do.
                Source.Options.Logger?.LogDebug(
                    "Ensure backchannel timing: {time}",
                    stopWatch.Elapsed
                    );
#endif
            }
        }

        #endregion
    }
}
