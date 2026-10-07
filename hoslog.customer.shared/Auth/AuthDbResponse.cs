namespace hoslog.customer.shared.Auth;

public sealed class AuthDbResponse : CommonDBResponse
{
    public bool isSuccess { get; set; }
    public bool isValid { get; set; }
}