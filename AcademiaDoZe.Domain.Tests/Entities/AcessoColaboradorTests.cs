using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoColaboradorTests
{
    private static Logradouro GetValidLogradouro() => Logradouro.Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;
    private static Arquivo GetValidArquivo() => Arquivo.Criar(new byte[] { 1, 2, 3 }).Value!;
    private static Colaborador GetValidColaborador() => Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", string.Empty, "Abcdef", GetValidArquivo(),
    DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
    ColaboradorTipo.Atendente,
    ColaboradorVinculo.CLT
    ).Value!;
    [Theory(DisplayName = "AcessoColaborador: colaborador nulo -> COLABORADOR_INVALIDO")]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Falhar_Criacao_Quando_ColaboradorEhNulo(bool colaboradorNull)
    {
        var colaborador = colaboradorNull ? null : GetValidColaborador();
        var result = AcessoColaborador.Criar(1, colaborador!, DateTime.Today.AddHours(10));
        if (colaboradorNull)
        {
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Notifications);
            Assert.Contains(result.Notifications, n => n.Mensagem == "COLABORADOR_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
        }
    }
    [Fact(DisplayName = "AcessoColaborador: data e hora ausentes -> DATA_HORA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataHoraAusente()
    {
        var colaborador = GetValidColaborador();
        var result = AcessoColaborador.Criar(1, colaborador, default);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INVALIDA");
    }
    [Theory(DisplayName = "AcessoColaborador: preserva acesso sem movimento informado")]
    [InlineData(10)]
    [InlineData(14)]
    public void Deve_Criar_Com_Sucesso_Quando_HorarioValido(int hour)
    {
        var colaborador = GetValidColaborador();
        var dataHora = DateTime.Today.AddHours(hour); // 10:00 or 14:00
        var result = AcessoColaborador.Criar(1, colaborador, dataHora);
        Assert.True(result.IsSuccess);
        Assert.Equal(colaborador.Id, result.Value!.ColaboradorId);
        Assert.Equal(dataHora, result.Value.DataHora);
        Assert.Null(result.Value.Entrada);
    }
    [Theory(DisplayName = "AcessoColaborador: preserva entrada e saída a qualquer hora do dia")]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(6, true)]
    [InlineData(22, false)]
    [InlineData(23, true)]
    public void Deve_Preservar_Movimento_Em_Qualquer_Horario(int hora, bool entrada)
    {
        var colaborador = GetValidColaborador();
        var dataHora = DateTime.Today.AddHours(hora);
        var result = AcessoColaborador.Criar(1, colaborador, dataHora, entrada);
        Assert.True(result.IsSuccess);
        Assert.Equal(dataHora, result.Value!.DataHora);
        Assert.Equal(entrada, result.Value.Entrada);
    }

}
