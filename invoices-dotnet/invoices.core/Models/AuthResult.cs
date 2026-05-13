using System;

namespace invoices.core.Models;

public record AuthResult(bool Success, string? Token, DateTime? ExpiresAt, string? ErrorMessage);
