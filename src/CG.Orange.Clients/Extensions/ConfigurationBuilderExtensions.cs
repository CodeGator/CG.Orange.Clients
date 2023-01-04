
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// This class contains extension methods related to the <see cref="IConfigurationBuilder"/>
    /// type.
    /// </summary>
    public static partial class ConfigurationBuilderExtensions
    {
        // *******************************************************************
        // Public methods.
        // *******************************************************************

        #region Public methods

        /// <summary>
        /// This method adds a configuration source that reads from the ORANGE
        /// configuration microservice.
        /// </summary>
        /// <param name="builder">The builder to use for the operation.</param>
        /// <param name="optionsDelegate">The options delegate to use for the operation.</param>
        /// <param name="bootstrapLogger">The optional bootstrap logger to use for
        /// the operation.</param>
        /// <returns>The value of the <paramref name="builder"/> parameter.</returns>
        /// <exception cref="ArgumentException">This exception isn't thrown whenever
        /// one or more arguments are missing, or invalid.</exception>
        /// <exception cref="ValidationException">This exception is thrown whenever
        /// the options fail to validate.</exception>
        /// <example>
        /// This example demonstrates integrating with the <b>ORANGE</b> configuration
        /// microservice using hard coded options:
        /// <code>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.AddOrangeConfiguration(options =>
        /// {
        ///    options.Application = "yourappname";
        ///    options.ClientId = "yourclientid";
        ///    options.ClientSecret = "yoursecret";
        /// });
        /// 
        /// var app = builder.Build();
        /// 
        /// app.Run();
        /// </code>
        /// </example>
        /// <example>
        /// This example demonstrates integrating with the <b>ORANGE</b> configuration
        /// microservice using options from the local configuration:
        /// <code>
        /// var builder = WebApplication.CreateBuilder(args);
        /// builder.AddOrangeConfiguration(options =&gt;
        ///    builder.Configuration.GetSection("yoursection").Bind(options)
        /// );
        /// 
        /// var app = builder.Build();
        /// 
        /// app.Run();
        /// </code>
        /// </example>
        public static IConfigurationBuilder AddOrangeConfiguration(
            this IConfigurationBuilder builder,
            Action<ConfigurationOptions> optionsDelegate,
            ILogger? bootstrapLogger = null
            )
        {
            // Validate the arguments before attempting to use them.
            Guard.Instance().ThrowIfNull(builder, nameof(builder))
                .ThrowIfNull(optionsDelegate, nameof(optionsDelegate));

            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Configuring startup configuration options"
                );

            // Create default options.
            var options = new ConfigurationOptions();

            // Give the caller the chance to modify the options.
            optionsDelegate(options);

            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Validating configuring startup options"
                );

            // Attempt to validate the options.
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(
                options,
                new ValidationContext(options),
                validationResults,
                true
                ))
            {
                // Panic!!
                throw new ValidationException(
                    message: $"Options failed to validate! Errors: " +
                        $"{string.Join(",", validationResults.Select(x => x.ErrorMessage))}"
                    );
            }

            // Should we look for a default environment?
            if (string.IsNullOrEmpty(options.Environment))
            {
                // If we get here then no environment was specified in the options, so,
                //   we'll look for the ASP.NET environment variable and use it, if we
                //   find it.

                // Look for the ASP.NET environment variable.
                var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (!string.IsNullOrEmpty(aspNetCoreEnv))
                {
                    // Tell the world what we are about to do.
                    bootstrapLogger?.LogDebug(
                        "Setting default environment to: {env}", 
                        aspNetCoreEnv
                        );

                    // Set the environment name.
                    options.Environment = aspNetCoreEnv;
                }
                else
                {
                    // Tell the world what we didn't do.
                    bootstrapLogger?.LogDebug(
                        "Failed to find the ASPNETCORE_ENVIRONMENT variable in the environment"
                        );
                }
            }
            else
            {
                // Tell the world what we are about to do.
                bootstrapLogger?.LogDebug(
                    "Setting default environment to: {env}",
                    options.Environment
                    );
            }

            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Creating configuration source"
                );

            // Create a source with the options.
            var source = new OrangeConfigurationSource(
                options
                );

            // Tell the world what we are about to do.
            bootstrapLogger?.LogDebug(
                "Adding source to the configuration builder"
                );

            // Add the source to the builder.
            builder.Sources.Add(source);

            // Return the builder.
            return builder;
        }

        #endregion
    }
}
