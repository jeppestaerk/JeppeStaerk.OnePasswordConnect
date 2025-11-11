using FluentAssertions;
using FluentAssertions.Extensibility;

[assembly: AssertionEngineInitializer(
    typeof(JeppeStaerk.OnePasswordConnect.Sdk.Tests.FluentAssertionsLicenseInitializer),
    nameof(JeppeStaerk.OnePasswordConnect.Sdk.Tests.FluentAssertionsLicenseInitializer.AcknowledgeLicense))]

namespace JeppeStaerk.OnePasswordConnect.Sdk.Tests;

/// <summary>
/// Initializer to suppress FluentAssertions license warning for non-commercial open-source use.
/// </summary>
public static class FluentAssertionsLicenseInitializer
{
    public static void AcknowledgeLicense()
    {
        License.Accepted = true;
    }
}
