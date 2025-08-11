using System.ComponentModel.DataAnnotations;

namespace TimesynqServer.DTO.Request.Account
{
    /// <summary>
    /// Represents the required information for a sign up request.
    /// </summary>
    public sealed class SignUpRequestDTO
    {
        /// <summary>
        /// The user's unique display name.
        /// </summary>
        public required string Username { get; init; }

        /// <summary>
        /// The user's email address. Is only allowed one associated account, and will receive confirmation emails and password reset links/codes.
        /// </summary>
        [EmailAddress]
        public required string Email { get; init; }

        /// <summary>
        /// The user's password. Must be at least 12 characters.
        /// </summary>
        public required string Password { get; init; }
    }
}
