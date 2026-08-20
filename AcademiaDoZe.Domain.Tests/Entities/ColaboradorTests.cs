using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;

    [Theory(DisplayName = "Colaborador: nome vazio -> NOME_OBRIGATORIO")]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Criacao_Quando_NomeVazio(string nome)
    {
        var result = Colaborador.Criar(
        1,
        nome,
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        ColaboradorTipo.Atendente,
        ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory(DisplayName = "Colaborador: criação bem-sucedida com nomes válidos (trim aplicado)")]
    [InlineData(" Fulano de Tal ")]
    [InlineData("Beltrano")]
    public void Deve_Criar_Com_Sucesso_Quando_NomeValido(string nome)
    {
        var result = Colaborador.Criar(
        1,
        nome,
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        ColaboradorTipo.Atendente,
        ColaboradorVinculo.CLT
        );
        Assert.True(result.IsSuccess);
        Assert.Equal(nome.Trim(), result.Value!.Nome);
    }

    [Theory(DisplayName = "Colaborador: data nascimento -> obrigatoriedade e idade mínima")]
    [InlineData("default", "DATA_NASCIMENTO_OBRIGATORIO")]
    [InlineData("under12", "DATA_NASCIMENTO_MINIMA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataNascimentoInvalida(string scenario, string expectedMessage)
    {
        DateOnly? data = scenario == "default" ? default(DateOnly?) : DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        var dataParam = data.HasValue ? data.Value : default(DateOnly);
        var result = Colaborador.Criar(
        1,
        "Fulano",
        "529.982.247-25",
        dataParam,
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        ColaboradorTipo.Atendente,
        ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expectedMessage);
    }

    [Theory(DisplayName = "Colaborador: data admissão -> obrigatoriedade e não pode ser futura")]
    [InlineData("default", "DATA_ADMISSAO_OBRIGATORIO")]
    [InlineData("future", "DATA_ADMISSAO_MAIOR_QUE_ATUAL")]
    public void Deve_Falhar_Criacao_Quando_DataAdmissaoInvalida(string scenario, string expectedMessage)
    {
        DateOnly dataAdmissao = scenario == "default" ? default : DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var result = Colaborador.Criar(
        1,
        "Fulano",
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        dataAdmissao,
        ColaboradorTipo.Atendente,
        ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == expectedMessage);
    }

    [Theory(DisplayName = "Colaborador: tipo inválido -> TIPO_COLABORADOR_INVALIDO")]
    [InlineData(999)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_TipoInvalido(int tipoValue)
    {
        var tipo = (ColaboradorTipo)tipoValue;
        var result = Colaborador.Criar(
        1,
        "Fulano",
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        tipo,
        ColaboradorVinculo.CLT
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "TIPO_COLABORADOR_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador: vinculo inválido -> VINCULO_COLABORADOR_INVALIDO")]
    [InlineData(999)]
    [InlineData(-1)]
    public void Deve_Falhar_Criacao_Quando_VinculoInvalido(int vinculoValue)
    {
        var vinculo = (ColaboradorVinculo)vinculoValue;
        var result = Colaborador.Criar(
        1,
        "Fulano",
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        ColaboradorTipo.Atendente,
        vinculo
        );
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Notifications);
        Assert.Contains(result.Notifications, n => n.Mensagem == "VINCULO_COLABORADOR_INVALIDO");
    }

    [Theory(DisplayName = "Colaborador: administrador exige CLT -> ADMINISTRADOR_CLT_INVALIDO")]
    [InlineData(ColaboradorVinculo.Estagio, true)]
    [InlineData(ColaboradorVinculo.CLT, false)]
    public void Deve_Validar_Administrador_Somente_CLT(ColaboradorVinculo vinculo, bool expectFailure)
    {
        var result = Colaborador.Criar(
        1,
        "Fulano",
        "529.982.247-25",
        DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
        "(11) 91234-5678",
        "user@example.com",
        GetValidLogradouro(),
        "123",
        "",
        "Abcdef",
        GetValidArquivo(),
        DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
        ColaboradorTipo.Administrador,
        vinculo
        );
        Assert.Equal(expectFailure, result.IsFailure);
        if (expectFailure)
        {
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "ADMINISTRADOR_CLT_INVALIDO");
        }
    }
}
