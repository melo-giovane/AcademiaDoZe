using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.ValueObjects;
namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    private static Logradouro GetValidLogradouro() => Logradouro

    .Criar(1, "12345-678", "Rua Teste", "Bairro", "Cidade", "SP", "Brasil").Value!;

    private static Arquivo GetValidArquivo() => Arquivo
    .Criar
    (new byte[] { 1, 2, 3 }).Value!;

    private static Aluno GetValidAluno() => Aluno
    .Criar(

    1,
    "João",
    "529.982.247-25",
    DateOnly
    .FromDateTime
    (DateTime.Today.AddYears(-20)),

    "(11) 91234-5678",
    "user@example.com",
    GetValidLogradouro(),
    "123",
    string.Empty,
    "Abcdef",
    GetValidArquivo()
    ).Value!;
    [Theory(DisplayName = "AcessoAluno: aluno nulo -> ALUNO_INVALIDO")]
    [InlineData
    (true)]
    [InlineData
    (false)]
    public void Deve_Falhar_Criacao_Quando_AlunoEhNulo
    (bool alunoNull)

    {
        var aluno = alunoNull ? null : GetValidAluno();
        var result = AcessoAluno

        .Criar(1, aluno!, DateTime.Today.AddHours(10));

        if (alunoNull)
        {
            Assert
            .True
            (result.IsFailure);

            Assert
            .NotEmpty
            (result.Notifications);

            Assert
            .Contains
            (result.Notifications,
            n =>
            n.Mensagem == "ALUNO_INVALIDO");

        }
        else
        {
            Assert
            .True
            (result.IsSuccess);

        }
    }
    [Fact(DisplayName = "AcessoAluno: data e hora ausentes -> DATA_HORA_INVALIDA")]
    public void Deve_Falhar_Criacao_Quando_DataHoraAusente()
    {
        var aluno = GetValidAluno();
        var result = AcessoAluno.Criar(1, aluno, default);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INVALIDA");
    }
    [Theory(DisplayName = "AcessoAluno: preserva acesso sem movimento informado")]
    [InlineData(10)]
    [InlineData(11)]
    public void Deve_Criar_Com_Sucesso_Quando_HorarioValido
    (int hour)

    {
        var aluno = GetValidAluno();
        var dataHora = DateTime.Today.AddHours

        (hour); // 10:00 or 11:00

        var result = AcessoAluno

        .Criar(1, aluno, dataHora);

        Assert
        .True
        (result.IsSuccess);
        Assert
        .Equal
        (aluno.Id, result.Value!.AlunoId);

        Assert
        .Equal
        (dataHora, result.Value.DataHora);
        Assert.Null(result.Value.Entrada);
    }
    [Theory(DisplayName = "AcessoAluno: preserva entrada e saída a qualquer hora do dia")]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(6, true)]
    [InlineData(22, false)]
    [InlineData(23, true)]
    public void Deve_Preservar_Movimento_Em_Qualquer_Horario(int hora, bool entrada)
    {
        var aluno = GetValidAluno();
        var dataHora = DateTime.Today.AddHours(hora);
        var result = AcessoAluno.Criar(1, aluno, dataHora, entrada);
        Assert.True(result.IsSuccess);
        Assert.Equal(dataHora, result.Value!.DataHora);
        Assert.Equal(entrada, result.Value.Entrada);
    }

}
