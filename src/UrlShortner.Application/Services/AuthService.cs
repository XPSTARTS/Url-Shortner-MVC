// src/UrlShortner.Application/Services/AuthService.cs
using UrlShortner.Domain.Entities;
using UrlShortner.Domain.Enums;
using UrlShortner.Domain.Interfaces;
using UrlShortner.Application.Common;

namespace UrlShortner.Application.Services;

/// <summary>
/// Orchestrates the complete authentication flow.
/// </summary>
public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordService _passwordService;
    private readonly OtpService _otpService;
    private readonly EmailService _emailService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;  // ← ADD THIS
    private readonly IRedisCacheService _redisCache;                  
    public AuthService(
        IUserRepository userRepository,
        PasswordService passwordService,
        OtpService otpService,
        EmailService emailService,
        JwtTokenService jwtTokenService,
        RefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IRedisCacheService redisCache)                    
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _otpService = otpService;
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;  // ← ADD THIS
        _redisCache = redisCache;                           
    }

    /// <summary>
    /// Step 1: Register - validates and sends OTP.
    /// </summary>
    public async Task<AuthResult> InitiateRegistrationAsync(string email, string password)
    {
        // Validate email format
        if (!IsValidEmail(email))
            return AuthResult.Failure("Invalid email format.");

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email))
            return AuthResult.Failure("Email is already registered.");

        // Validate password strength
        if (password.Length < 8)
            return AuthResult.Failure("Password must be at least 8 characters.");

        // Generate and send OTP
        var otp = await _otpService.GenerateOtpAsync(email, OtpPurpose.Register);
        await _emailService.SendOtpEmailAsync(email, otp, "Register");

        return AuthResult.Success("Verification code sent to your email.");
    }

    /// <summary>
    /// Step 2: Complete registration after OTP verification.
    /// </summary>
    public async Task<AuthResult> CompleteRegistrationAsync(string email, string password, string otp, string? ipAddress = null)
    {
        // Verify OTP
        var otpValid = await _otpService.VerifyOtpAsync(email, otp, OtpPurpose.Register);
        if (!otpValid)
            return AuthResult.Failure("Invalid or expired verification code.");

        // Create user
        var user = new User
        {
            Email = email.ToLower(),
            PasswordHash = _passwordService.HashPassword(password),
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateAsync(user);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(userId, email);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(userId, ipAddress);

        return AuthResult.Success("Registration successful!", accessToken, refreshToken);
    }

    /// <summary>
    /// Step 1: Login - validates credentials and sends OTP.
    /// </summary>
    public async Task<AuthResult> InitiateLoginAsync(string email, string password)
    {
        // Find user
        var user = await _userRepository.GetByEmailAsync(email.ToLower());
        if (user == null)
            return AuthResult.Failure("Invalid email or password.");

        // Verify password
        if (!_passwordService.VerifyPassword(password, user.PasswordHash))
            return AuthResult.Failure("Invalid email or password.");

        // Check if email is verified
        if (!user.EmailVerified)
            return AuthResult.Failure("Email not verified. Please register again.");

        // Generate and send OTP for 2FA
        var otp = await _otpService.GenerateOtpAsync(email, OtpPurpose.Login);
        await _emailService.SendOtpEmailAsync(email, otp, "Login");

        return AuthResult.Success("Verification code sent to your email.");
    }

    /// <summary>
    /// Step 2: Complete login after OTP verification.
    /// </summary>
    public async Task<AuthResult> CompleteLoginAsync(string email, string otp, string? ipAddress = null, string? deviceInfo = null)
    {
        // Verify OTP
        var otpValid = await _otpService.VerifyOtpAsync(email, otp, OtpPurpose.Login);
        if (!otpValid)
            return AuthResult.Failure("Invalid or expired verification code.");

        // Get user
        var user = await _userRepository.GetByEmailAsync(email.ToLower());
        if (user == null)
            return AuthResult.Failure("User not found.");

        // Update last login
        await _userRepository.UpdateLastLoginAsync(user.Id);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, email);
        var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, ipAddress, deviceInfo);

        return AuthResult.Success("Login successful!", accessToken, refreshToken);
    }

    /// <summary>
    /// Refreshes an expired access token.
    /// </summary>
    public async Task<AuthResult> RefreshAccessTokenAsync(int userId, string refreshToken, string? ipAddress = null)
    {
        // Validate refresh token
        var isValid = await _refreshTokenService.ValidateRefreshTokenAsync(userId, refreshToken);
        if (!isValid)
            return AuthResult.Failure("Invalid or expired refresh token.");

        // Get user
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return AuthResult.Failure("User not found.");

        // Rotate refresh token (revoke old, issue new)
        var newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, userId, ipAddress);
        if (newRefreshToken == null)
            return AuthResult.Failure("Failed to rotate refresh token.");

        // Generate new access token
        var newAccessToken = _jwtTokenService.GenerateAccessToken(userId, user.Email);

        return AuthResult.Success("Token refreshed.", newAccessToken, newRefreshToken);
    }

    /// <summary>
    /// Step 1: Initiates password reset by sending OTP.
    /// </summary>
    public async Task<AuthResult> InitiatePasswordResetAsync(string email)
    {
        // Check if user exists
        var user = await _userRepository.GetByEmailAsync(email.ToLower());

        // IMPORTANT: Don't reveal if email exists or not!
        // Always show the same message to prevent email enumeration
        if (user == null)
            return AuthResult.Success("If an account exists with this email, a reset code has been sent.");

        // Check if email is verified
        if (!user.EmailVerified)
            return AuthResult.Success("If an account exists with this email, a reset code has been sent.");

        // Generate and send OTP
        var otp = await _otpService.GenerateOtpAsync(email, OtpPurpose.ResetPassword);
        await _emailService.SendOtpEmailAsync(email, otp, "ResetPassword");

        return AuthResult.Success("If an account exists with this email, a reset code has been sent.");
    }

    /// <summary>
    /// Step 2: Verifies OTP for password reset.
    /// </summary>
    // src/UrlShortner.Application/Services/AuthService.cs
    public async Task<AuthResult> VerifyResetOtpAsync(string email, string otp)
    {
        var user = await _userRepository.GetByEmailAsync(email.ToLower());
        if (user == null)
            return AuthResult.Failure("Invalid or expired reset code.");

        var otpValid = await _otpService.VerifyOtpAsync(email, otp, OtpPurpose.ResetPassword);
        if (!otpValid)
            return AuthResult.Failure("Invalid or expired reset code.");

        // Generate a temporary reset token
        var resetToken = Guid.NewGuid().ToString("N");

        // Store in Redis with 5-minute expiry
        await _redisCache.SetRefreshTokenAsync(
            $"reset:{email.ToLower()}:{resetToken}",
            "valid",
            TimeSpan.FromMinutes(5));

        // RETURN THE TOKEN - This is what the controller uses!
        return AuthResult.Success(
            "Code verified. You can now reset your password.",
            resetToken,  // ← This goes into AccessToken field
            resetToken); // ← This goes into RefreshToken field
    }

    /// <summary>
    /// Step 3: Resets the password and revokes all sessions.
    /// </summary>
    public async Task<AuthResult> ResetPasswordAsync(string email, string resetToken, string newPassword)
    {
        // Validate the reset token
        var tokenKey = $"reset:{email.ToLower()}:{resetToken}";
        var tokenValid = await _redisCache.KeyExistsAsync(tokenKey);

        if (!tokenValid)
            return AuthResult.Failure("Reset link has expired. Please request a new one.");

        // Get user
        var user = await _userRepository.GetByEmailAsync(email.ToLower());
        if (user == null)
            return AuthResult.Failure("User not found.");

        // Validate password strength
        if (newPassword.Length < 8)
            return AuthResult.Failure("Password must be at least 8 characters.");

        // Hash new password
        var newPasswordHash = _passwordService.HashPassword(newPassword);

        // Update password in database
        await _userRepository.UpdatePasswordAsync(user.Id, newPasswordHash);

        // SECURITY: Revoke ALL refresh tokens (force logout on all devices)
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

        // Delete the reset token
        await _redisCache.RemoveRefreshTokenAsync(tokenKey);

        // Delete any unused OTPs for this email
        await _redisCache.RemoveOtpAsync($"otp:resetpassword:{email.ToLower()}");

        return AuthResult.Success("Password has been reset successfully. Please login with your new password.");
    }

    /// <summary>
    /// Change password for logged-in user (requires current password).
    /// </summary>
    public async Task<AuthResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return AuthResult.Failure("User not found.");

        // Verify current password
        if (!_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
            return AuthResult.Failure("Current password is incorrect.");

        // Validate new password
        if (newPassword.Length < 8)
            return AuthResult.Failure("New password must be at least 8 characters.");

        // Hash and update
        var newHash = _passwordService.HashPassword(newPassword);
        await _userRepository.UpdatePasswordAsync(user.Id, newHash);

        // Revoke all refresh tokens (force re-login on all devices)
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

        return AuthResult.Success("Password changed successfully. Please login again.");
    }

    /// <summary>
    /// Logout - revoke refresh token.
    /// </summary>
    public async Task LogoutAsync(int userId, string refreshToken)
    {
        await _refreshTokenService.RevokeRefreshTokenAsync(userId, refreshToken);
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}