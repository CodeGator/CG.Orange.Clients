
namespace CG.Orange.Clients.Providers;

// I copied this from here:
// https://stackoverflow.com/questions/59313256/deserialize-anonymous-type-with-system-text-json

/// <summary>
/// This class utility contains logic to deserialize JSON to anonymous types.
/// </summary>
internal static partial class JsonHelper
{
    // *******************************************************************
    // Public methods.
    // *******************************************************************

    #region Public methods

    /// <summary>
    /// This method deserializes the given JSON to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to use for the operation.</typeparam>
    /// <param name="json">The JSON to use for the operation.</param>
    /// <param name="anonymousTypeObject">The anonymous type to use for 
    /// the operation.</param>
    /// <param name="options">The serialization options to use for the 
    /// operation.</param>
    /// <returns>The deserialized representation of the JSON.</returns>
    public static T? DeserializeAnonymousType<T>(
        string json,
        T anonymousTypeObject,
        JsonSerializerOptions? options = default
        ) => JsonSerializer.Deserialize<T>(json, options);

    #endregion
}

