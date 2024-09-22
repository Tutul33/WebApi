# Technical Documentation: JWT Authentication & Custom Middleware in ASP.NET Core 5 Web API

Table of Contents:

Overview

Prerequisites

JWT Authentication & Authorization

Implementation Details

JWT Settings in appsettings.json

Custom JWT Middleware

Token Service

Securing Controllers

Global Error Handling Middleware

Testing the API

Conclusion

# Overview
This documentation provides an overview and technical details for implementing JWT authentication and authorization in an ASP.NET Core 5 Web API, along with a custom JWT middleware to handle token validation. The application separates public and private resources, with secure access to private resources granted only through valid JWT tokens.

# Prerequisites
   
ASP.NET Core 5.0

Visual Studio 2019/2022 or VS Code

.NET Core SDK 5.x

Postman or Curl (for testing APIs)

# JWT Authentication & Authorization
   
JWT (JSON Web Token) is a secure method for transmitting information between parties as a JSON object. It is commonly used for authorization in web applications. This implementation uses a custom middleware to manually validate and extract claims from the token.

# Implementation Details
## JWT Settings in appsettings.json
   
The JWT secret key and other settings are stored in the appsettings.json file. This secret key is used to sign the JWT tokens for secure communication.

{
  "JwtSettings": {
    "Secret": "YourSecretKeyHere"
  }
}

Secret: A unique key used for signing and validating the JWT tokens. This key should be kept secure and confidential.

## Custom JWT Middleware

The JWT Middleware is responsible for:

Extracting the token from the Authorization header.

Validating the token and attaching the authenticated user to the HttpContext if the token is valid.

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secretKey;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _secretKey = configuration.GetValue<string>("JwtSettings:Secret");
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null)
        {
            AttachUserToContext(context, token);
        }

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var username = jwtToken.Claims.First(x => x.Type == "username").Value;

            // Attach user to context on successful JWT validation
            context.Items["User"] = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username)
            }));
        }
        catch
        {
            // Token validation failed, request will not be authenticated
        }
    }
}

# Token Service

The TokenService is responsible for generating JWT tokens for authenticated users. It fetches the secret key from the configuration and creates a token with claims, including the username.

Code:
public class TokenService
{
    private readonly string _secretKey;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration.GetValue<string>("JwtSettings:Secret");
    }

    public string GenerateToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("username", username) }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
# Securing Controllers

Controllers are separated into public and private endpoints. The private endpoints require a valid JWT token for access.

# PublicController – No JWT required:

    [AllowAnonymous]  // No authentication required for public access
    
    [Route("api/[controller]")]
    
    [ApiController]
    
    public class LoginController : ControllerBase
    {
        IConfiguration _configuration = null;
        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("info")]
        public IActionResult GetPublicInfo()
        {
            return Ok(new { message = "This is public information accessible to everyone." });
        }

        [HttpGet("generateToken")]
        public IActionResult GenerateToken(string userName)
        {
            TokenService tokenService = new TokenService(_configuration);
            var key=tokenService.GenerateToken(userName);
            return Ok(new { message = key });
        }
    }
    
# PrivateController – JWT required:
   
    [Authorize(Policy = "PrivateAccess")]  // Authentication required for private access

    [Route("api/[controller]")]
    
    [ApiController]
    
    public class UsersController : ControllerBase
    {
        public UsersController()
        {

        }

        [HttpGet("info")]
        public IActionResult GetPrivateInfo()
        {
            return Ok(new { message = "This is private information accessible to authenticated users." });
        }
    }
    
# Global Error Handling Middleware

The global error handler is used to manage exceptions and return consistent error responses across the API.

Example Global Error Handling Middleware:

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var response = new { message = "An unexpected error occurred.", details = exception.Message };
        return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}
# Testing the API

# Public Endpoint Testing

Endpoint: GET /api/public/info

Authorization: None required.

Expected Response:

{
  "message": "This is public information."
}

# Private Endpoint Testing

Endpoint: GET /api/private/info

Authorization: Bearer <JWT Token>

Expected Response:

{
  "message": "Hello <username>, this is private information."
}
# Conclusion

This implementation demonstrates how to build a secure ASP.NET Core 5 Web API with JWT authentication using custom middleware. By storing the JWT secret key in appsettings.json, leveraging middleware for token validation, and segregating public and private API resources, the API provides a robust solution for managing secure access.

This technical documentation can be expanded upon based on project requirements, but it covers the essential aspects of JWT integration in an ASP.NET Core Web API.
