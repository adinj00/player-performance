namespace PlayerPerformance.Api.Authentication;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
internal sealed class AllowPasswordChangeRequiredAttribute : Attribute
{
}
