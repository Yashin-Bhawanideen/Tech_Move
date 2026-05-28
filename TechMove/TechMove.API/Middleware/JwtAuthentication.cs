using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TechMove.API.Middleware
{
    /// <summary>
    /// Custom JWT Authentication Middleware
    /// Validates JWT tokens on each request
    /// </summary>
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;

        public JwtAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger and Auth endpoints
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.Contains("swagger") || path.Contains("api/auth")))
            {
                await _next(context);
                return;
            }

            // Get token from Authorization header
            var token = GetTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                // Validate and attach user to context
                var principal = ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                    _logger.LogDebug("User authenticated successfully");
                }
                else
                {
                    _logger.LogWarning("Invalid token provided");
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Invalid or expired token" });
                    return;
                }
            }
            else
            {
                // Check if endpoint requires authentication
                var endpoint = context.GetEndpoint();
                var authorizeAttribute = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();

                if (authorizeAttribute != null)
                {
                    _logger.LogWarning("Unauthorized access attempt to {Path}", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Authorization token required" });
                    return;
                }
            }

            await _next(context);
        }

        private string? GetTokenFromHeader(HttpContext context)
        {
            // Check Authorization header
            if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                var token = authHeader.ToString();
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return token.Substring("Bearer ".Length).Trim();
                }
                return token;
            }

            // Also check query string for token (useful for file downloads)
            if (context.Request.Query.TryGetValue("token", out var queryToken))
            {
                return queryToken.ToString();
            }

            return null;
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "TechMoveSecretKey2026ForDevelopmentOnly123!");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Token has invalid signature");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return null;
            }
        }
    }

    /// <summary>
    /// Extension method to register JWT middleware
    /// </summary>
    public static class JwtAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtAuthenticationMiddleware>();
        }
    }
}