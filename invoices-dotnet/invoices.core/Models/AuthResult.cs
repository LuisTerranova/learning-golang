using System;

namespace invoices.core.Models;

public record AuthResult(bool Success, string? Token, string? RefreshToken, DateTime? ExpiresAt, string? ErrorMessage);
