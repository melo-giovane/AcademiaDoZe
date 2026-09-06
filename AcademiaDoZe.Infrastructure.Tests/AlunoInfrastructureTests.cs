using AcademiaDoZe.Domain.Entities;//Giovane Melo  
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Exceptions;
using AcademiaDoZe.Infrastructure.Repositories;

namespace AcademiaDoZe.Infrastructure.Tests;

public class AlunoInfrastructureTests : TestBase
{
    private readonly LogradouroRepository _logradouroRepo;
    private readonly AlunoRepository _AlunoRepo;

    public AlunoInfrastructureTests()
    {
        _logradouroRepo = new LogradouroRepository(ConnectionString, DatabaseType);
        _AlunoRepo = new AlunoRepository(ConnectionString, DatabaseType);
    }

    internal static async Task<Aluno> CriarEInserirAlunoAsync(AlunoRepository AlunoRepo, LogradouroRepository logradouroRepo)
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;
        var AlunoResult = Aluno.Criar(
        id: 0,
        nome: "Aluno Teste " + Guid.NewGuid().ToString("N")[..5],
        cpf: GerarCpf(),
        dataNascimento: new DateOnly(1995, 5, 15),
        telefone: GerarTelefone(),
        email: GerarEmail(),
        endereco: logradouro,
        numero: "200",
        complemento: "Sala 2",
        senha: "SenhaValida123",
        foto: foto);
        if (AlunoResult.IsFailure)
        {
            throw new Exception($"Falha ao criar Aluno: {string.Join(", ", AlunoResult.Notifications.Select(n => n.Mensagem))}");
        }
        return await AlunoRepo.Adicionar(AlunoResult.Value!);
    }

    [Fact]
    public async Task Aluno_Adicionar_E_ObterPorId_Sucesso()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        Assert.NotNull(Aluno);
        Assert.True(Aluno.Id > 0);
        var obtido = await _AlunoRepo.ObterPorId(Aluno.Id);
        Assert.NotNull(obtido);
        Assert.Equal(Aluno.Id, obtido.Id);
        Assert.Equal(Aluno.Cpf.Valor, obtido.Cpf.Valor);
        Assert.Equal(Aluno.Nome, obtido.Nome);
        Assert.Equal(Aluno.Email.Valor, obtido.Email.Valor);
        Assert.NotNull(obtido.Endereco);
        Assert.Equal(Aluno.Endereco.LogradouroId, obtido.Endereco.LogradouroId);
    }
    [Fact]
    public async Task Aluno_ObterPorId_RetornaNuloQuandoInexistente()
    {
        var obtido = await _AlunoRepo.ObterPorId(999999);
        Assert.Null(obtido);
    }
    [Fact]
    public async Task Aluno_ObterTodos_Sucesso()
    {
        await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var todos = await _AlunoRepo.ObterTodos();
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }
    [Fact]
    public async Task Aluno_Atualizar_Sucesso()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 5, 6, 7, 8 }).Value!;
        var alunoResult = Aluno.Criar(
        0, "Aluno Original", GerarCpf(), new DateOnly(1995, 5, 15),
        GerarTelefone(), GerarEmail(), logradouro, "200", "Sala 2",
        "Senha123", foto);
        var aluno = await _AlunoRepo.Adicionar(alunoResult.Value!);
        var novoNome = "Aluno Editado " + Guid.NewGuid().ToString("N")[..5];
        var AlunoAtualizado = Aluno.Criar(
        id: aluno.Id,
        nome: novoNome,
        cpf: aluno.Cpf.Valor,
        dataNascimento: aluno.DataNascimento,
        telefone: aluno.Telefone.Valor,
        email: aluno.Email.Valor,
        endereco: logradouro,
        numero: "300",
        complemento: "Sala 3",
        senha: aluno.Senha.Valor,
        foto: aluno.Foto
        ).Value!;
        var resultado = await _AlunoRepo.Atualizar(AlunoAtualizado);
        Assert.NotNull(resultado);
        Assert.Equal(novoNome, resultado.Nome);
        var noBanco = await _AlunoRepo.ObterPorId(aluno.Id);
        Assert.NotNull(noBanco);
        Assert.Equal(novoNome, noBanco.Nome);
    }
    [Fact]
    public async Task Aluno_Atualizar_LancaExcecaoQuandoInexistente()
    {
        var logradouro = await LogradouroInfrastructureTests.CriarEInserirLogradouroAsync(_logradouroRepo);
        var foto = Arquivo.Criar(new byte[] { 1, 2 }).Value!;
        var AlunoInexistente = Aluno.Criar(
        id: 999999,
        nome: "Inexistente",
        cpf: GerarCpf(),
        dataNascimento: new DateOnly(1990, 1, 1),
        telefone: GerarTelefone(),
        email: GerarEmail(),
        endereco: logradouro,
        numero: "1",
        complemento: "",
        senha: "SenhaValida123",
        foto: foto
        ).Value!;
        var ex = await Assert.ThrowsAsync<InfrastructureException>(() => _AlunoRepo.Atualizar(AlunoInexistente));
        Assert.Equal("REGISTRO_NAO_ENCONTRADO", ex.ErrorCode);
    }
    [Fact]
    public async Task Aluno_Remover_Sucesso()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var removido = await _AlunoRepo.Remover(Aluno.Id);
        Assert.True(removido);
        var noBanco = await _AlunoRepo.ObterPorId(Aluno.Id);
        Assert.Null(noBanco);
    }

    [Fact]
    public async Task Aluno_Remover_RetornaFalseQuandoInexistente()
    {
        var removido = await _AlunoRepo.Remover(999999);
        Assert.False(removido);
    }
    [Fact]
    public async Task Aluno_ObterPorCpf_SucessoENulo()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var obtido = await _AlunoRepo.ObterPorCpf(Aluno.Cpf);
        Assert.NotNull(obtido);
        Assert.Equal(Aluno.Id, obtido.Id);
        var cpfInexistente = Cpf.Criar(GerarCpf()).Value!;
        var naoObtido = await _AlunoRepo.ObterPorCpf(cpfInexistente);
        Assert.Null(naoObtido);
    }
    [Fact]
    public async Task Aluno_ObterPorEmail_SucessoENulo()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var obtido = await _AlunoRepo.ObterPorEmail(Aluno.Email);
        Assert.NotNull(obtido);
        Assert.Equal(Aluno.Id, obtido.Id);
        var emailInexistente = Email.Criar(GerarEmail()).Value!;
        var naoObtido = await _AlunoRepo.ObterPorEmail(emailInexistente);
        Assert.Null(naoObtido);
    }
    [Fact]
    public async Task Aluno_CpfJaExiste_ValidacaoCorreta()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var existe = await _AlunoRepo.CpfJaExiste(Aluno.Cpf);
        Assert.True(existe);
        var existeIgnorandoId = await _AlunoRepo.CpfJaExiste(Aluno.Cpf, Aluno.Id);
        Assert.False(existeIgnorandoId);
        var cpfInedito = Cpf.Criar(GerarCpf()).Value!;
        var existeInedito = await _AlunoRepo.CpfJaExiste(cpfInedito);
        Assert.False(existeInedito);
    }
    [Fact]
    public async Task Aluno_EmailJaExiste_ValidacaoCorreta()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var existe = await _AlunoRepo.EmailJaExiste(Aluno.Email);
        Assert.True(existe);
        var existeIgnorandoId = await _AlunoRepo.EmailJaExiste(Aluno.Email, Aluno.Id);
        Assert.False(existeIgnorandoId);
        var emailInedito = Email.Criar(GerarEmail()).Value!;
        var existeInedito = await _AlunoRepo.EmailJaExiste(emailInedito);
        Assert.False(existeInedito);
    }

    [Fact]
    public async Task Aluno_TrocarSenha_SucessoEFalha()
    {
        var Aluno = await CriarEInserirAlunoAsync(_AlunoRepo, _logradouroRepo);
        var novaSenha = Senha.Criar("NovaSenhaColab123").Value!;
        var alterou = await _AlunoRepo.TrocarSenha(Aluno.Id, novaSenha);
        Assert.True(alterou);
        var atualizado = await _AlunoRepo.ObterPorId(Aluno.Id);
        Assert.NotNull(atualizado);
        Assert.Equal("NovaSenhaColab123", atualizado.Senha.Valor);
        var alterouInexistente = await _AlunoRepo.TrocarSenha(999999, novaSenha);
        Assert.False(alterouInexistente);
    }

}