namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Marks a controller (or a single action) so its endpoints are only mapped in the
/// Development environment. Consumed by the <see cref="DevelopmentOnlyControllerConvention"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class DevelopmentOnlyAttribute : Attribute;