using NHRMS.Application.Abstractions;
using NHRMS.Domain;

namespace NHRMS.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_should_not_reference_infrastructure_or_api()
    {
        var domainAssembly = typeof(BaseEntity).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, assembly => assembly.Name == "NHRMS.Infrastructure");
        Assert.DoesNotContain(referencedAssemblies, assembly => assembly.Name == "NHRMS.Api");
    }

    [Fact]
    public void Application_should_not_reference_api()
    {
        var applicationAssembly = typeof(IUnitOfWork).Assembly;
        var referencedAssemblies = applicationAssembly.GetReferencedAssemblies();

        Assert.DoesNotContain(referencedAssemblies, assembly => assembly.Name == "NHRMS.Api");
    }
}
