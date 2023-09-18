namespace SearchService.Application.UnitTests.Common.Mappings;

using System.Runtime.Serialization;
using Application.Mappings;
using AutoMapper;

public class MappingTests
{
    private readonly IConfigurationProvider configuration;
    private readonly IMapper mapper;

    public MappingTests()
    {
        this.configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>());

        this.mapper = this.configuration.CreateMapper();
    }

    [Fact]
    public void ShouldHaveValidConfiguration() => this.configuration.AssertConfigurationIsValid();

    // [Theory]
    // [InlineData(typeof(Type), typeof(Type))]
    // public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    // {
    //     var instance = GetInstanceOf(source);

    //     this.mapper.Map(instance, source, destination);
    // }

    private static object GetInstanceOf(Type type)
    {
        if (type.GetConstructor(Type.EmptyTypes) != null)
        {
            return Activator.CreateInstance(type);
        }

        // Type without parameterless constructor
        return FormatterServices.GetUninitializedObject(type);
    }
}
