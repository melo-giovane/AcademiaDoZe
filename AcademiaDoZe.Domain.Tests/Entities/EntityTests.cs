using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.Exceptions;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class EntityTests
{
    private sealed class FakeEntity : Entity
    {
        public FakeEntity(int id) : base(id) { }
    }

    [Theory(DisplayName = "Entity: id negativo -> DomainException ID_NEGATIVO")]
    [InlineData(-1)]
    public void Deve_Lancar_DomainException_Quando_IdNegativo(int id)
    {
        var ex = Assert.Throws<DomainException>(() => new FakeEntity(id));
        Assert.Equal("ID_NEGATIVO", ex.Message);
    }
}
