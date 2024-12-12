using Microsoft.AspNetCore.Authentication;

namespace C8yServices.Authentication.Bearer;

/// <summary>
/// Options class provides information needed to control bearer authentication handler behavior
/// </summary>
#pragma warning disable S2094 // Classes should not be empty
public sealed class BearerAuthenticationOptions : AuthenticationSchemeOptions;
#pragma warning restore S2094 // Classes should not be empty
