using AcademiaDoZe.Application.DTOs;//Giovane Melo
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
namespace AcademiaDoZe.Application.Services;

public class AcessoAlunoService : IAcessoAlunoService
{
    private readonly Func<IAcessoAlunoRepository> _acessoRepoFactory;
    private readonly Func<IAlunoRepository> _alunoRepoFactory;
    public AcessoAlunoService(Func<IAcessoAlunoRepository> acessoRepoFactory, Func<IAlunoRepository> alunoRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _alunoRepoFactory = alunoRepoFactory ?? throw new ArgumentNullException(nameof(alunoRepoFactory));
    }
    public async Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return null;
        var aluno = await _alunoRepoFactory().ObterPorId(acesso.AlunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno associado ao acesso {acesso.Id} não encontrado.");
        return acesso.ToDto(aluno.ToDto());
    }
    public async Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _acessoRepoFactory().ObterTodos(cancellationToken);
        return await EnriquecerComAlunosAsync(acessos, cancellationToken);
    }
    public async Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        var aluno = await _alunoRepoFactory().ObterPorId(acessoDto.AlunoAcesso.Id, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno com ID {acessoDto.AlunoAcesso.Id} não encontrado.");
        var acesso = acessoDto.ToEntity(aluno);
        var adicionado = await _acessoRepoFactory().Adicionar(acesso, cancellationToken);
        return adicionado.ToDto(aluno.ToDto());
    }
    public async Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        var existente = await _acessoRepoFactory().ObterPorId(acessoDto.Id, cancellationToken)
        ?? throw new KeyNotFoundException($"Acesso com ID {acessoDto.Id} não encontrado.");
        var aluno = await _alunoRepoFactory().ObterPorId(acessoDto.AlunoAcesso.Id, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno com ID {acessoDto.AlunoAcesso.Id} não encontrado.");
        var acesso = acessoDto.ToEntity(aluno);
        var atualizado = await _acessoRepoFactory().Atualizar(acesso, cancellationToken);
        return atualizado.ToDto(aluno.ToDto());
    }
    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return false;
        return await _acessoRepoFactory().Remover(id, cancellationToken);
    }
    public async Task<IEnumerable<AcessoAlunoDto>> ObterAcessosPorAlunoPeriodoAsync(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var acessos = await _acessoRepoFactory().ObterAcessosPorAlunoPeriodo(alunoId, inicio, fim, cancellationToken);
        return await EnriquecerComAlunosAsync(acessos, cancellationToken);
    }
    public async Task<AcessoAlunoDto?> ObterUltimoAcessoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterUltimoAcesso(alunoId, cancellationToken);
        if (acesso == null) return null;
        var aluno = await _alunoRepoFactory().ObterPorId(acesso.AlunoId, cancellationToken)
        ?? throw new InvalidOperationException($"Aluno associado ao acesso {acesso.Id} não encontrado.");
        return acesso.ToDto(aluno.ToDto());
    }
    public async Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory().EstaNaAcademia(alunoId, cancellationToken);
    }
    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMesAsync(int mes, CancellationToken cancellationToken = default)
    {
        if (mes < 1 || mes > 12)
            throw new ArgumentException("Mês deve estar entre 1 e 12.", nameof(mes));
        return await _acessoRepoFactory().ObterHorarioMaisProcuradoPorMes(mes, cancellationToken);
    }
    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMesAsync(int mes, CancellationToken cancellationToken = default)
    {
        if (mes < 1 || mes > 12)
            throw new ArgumentException("Mês deve estar entre 1 e 12.", nameof(mes));
        return await _acessoRepoFactory().ObterPermanenciaMediaPorMes(mes, cancellationToken);
    }
    public async Task<IEnumerable<AlunoDto>> ObterAlunosSemAcessoNosUltimosDiasAsync(int dias, CancellationToken cancellationToken = default)
    {
        if (dias <= 0)
            throw new ArgumentException("O número de dias deve ser maior que zero.", nameof(dias));
        var alunos = await _acessoRepoFactory().ObterAlunosSemAcessoNosUltimosDias(dias, cancellationToken);
        return [.. alunos.Select(a => a.ToDto())];
    }

    private async Task<IEnumerable<AcessoAlunoDto>> EnriquecerComAlunosAsync(IEnumerable<Domain.Entities.AcessoAluno> acessos, CancellationToken cancellationToken)
    {
        var alunoRepo = _alunoRepoFactory();
        var cache = new Dictionary<int, AlunoDto>();
        var resultado = new List<AcessoAlunoDto>();
        foreach (var acesso in acessos)
        {
            if (!cache.TryGetValue(acesso.AlunoId, out var alunoDto))
            {
                var aluno = await alunoRepo.ObterPorId(acesso.AlunoId, cancellationToken)
                ?? throw new InvalidOperationException($"Aluno associado ao acesso {acesso.Id} não encontrado.");
                alunoDto = aluno.ToDto();
                cache[acesso.AlunoId] = alunoDto;
            }
            resultado.Add(acesso.ToDto(alunoDto));
        }
        return resultado;
    }
}
