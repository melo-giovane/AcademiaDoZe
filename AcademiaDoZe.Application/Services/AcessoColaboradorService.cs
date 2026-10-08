using AcademiaDoZe.Application.DTOs;//Giovane Melo
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Domain.Repositories;
namespace AcademiaDoZe.Application.Services;

public class AcessoColaboradorService : IAcessoColaboradorService
{
    private readonly Func<IAcessoColaboradorRepository> _acessoRepoFactory;
    private readonly Func<IColaboradorRepository> _colaboradorRepoFactory;
    public AcessoColaboradorService(Func<IAcessoColaboradorRepository> acessoRepoFactory, Func<IColaboradorRepository> colaboradorRepoFactory)
    {
        _acessoRepoFactory = acessoRepoFactory ?? throw new ArgumentNullException(nameof(acessoRepoFactory));
        _colaboradorRepoFactory = colaboradorRepoFactory ?? throw new ArgumentNullException(nameof(colaboradorRepoFactory));
    }
    public async Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return null;
        var colaborador = await _colaboradorRepoFactory().ObterPorId(acesso.ColaboradorId, cancellationToken)
        ?? throw new InvalidOperationException($"Colaborador associado ao acesso {acesso.Id} não encontrado.");
        return acesso.ToDto(colaborador.ToDto());
    }
    public async Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        var acessos = await _acessoRepoFactory().ObterTodos(cancellationToken);
        return await EnriquecerComColaboradoresAsync(acessos, cancellationToken);
    }
    public async Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        var colaborador = await _colaboradorRepoFactory().ObterPorId(acessoDto.ColaboradorAcesso.Id, cancellationToken)
        ?? throw new InvalidOperationException($"Colaborador com ID {acessoDto.ColaboradorAcesso.Id} não encontrado.");
        var acesso = acessoDto.ToEntity(colaborador);
        var adicionado = await _acessoRepoFactory().Adicionar(acesso, cancellationToken);
        return adicionado.ToDto(colaborador.ToDto());
    }
    public async Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        var existente = await _acessoRepoFactory().ObterPorId(acessoDto.Id, cancellationToken)
        ?? throw new KeyNotFoundException($"Acesso com ID {acessoDto.Id} não encontrado.");
        var colaborador = await _colaboradorRepoFactory().ObterPorId(acessoDto.ColaboradorAcesso.Id, cancellationToken)
        ?? throw new InvalidOperationException($"Colaborador com ID {acessoDto.ColaboradorAcesso.Id} não encontrado.");
        var acesso = acessoDto.ToEntity(colaborador);
        var atualizado = await _acessoRepoFactory().Atualizar(acesso, cancellationToken);
        return atualizado.ToDto(colaborador.ToDto());
    }
    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterPorId(id, cancellationToken);
        if (acesso == null) return false;
        return await _acessoRepoFactory().Remover(id, cancellationToken);
    }
    public async Task<IEnumerable<AcessoColaboradorDto>> ObterAcessosPorColaboradorPeriodoAsync(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        var acessos = await _acessoRepoFactory().ObterAcessosPorColaboradorPeriodo(colaboradorId, inicio, fim, cancellationToken);
        return await EnriquecerComColaboradoresAsync(acessos, cancellationToken);
    }
    public async Task<AcessoColaboradorDto?> ObterUltimoAcessoAsync(int colaboradorId, CancellationToken cancellationToken = default)
    {
        var acesso = await _acessoRepoFactory().ObterUltimoAcesso(colaboradorId, cancellationToken);
        if (acesso == null) return null;
        var colaborador = await _colaboradorRepoFactory().ObterPorId(acesso.ColaboradorId, cancellationToken)
        ?? throw new InvalidOperationException($"Colaborador associado ao acesso {acesso.Id} não encontrado.");
        return acesso.ToDto(colaborador.ToDto());
    }
    public async Task<TimeSpan> ObterHorasTrabalhadasNoDiaAsync(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        return await _acessoRepoFactory().ObterHorasTrabalhadasNoDia(colaboradorId, data, cancellationToken);
    }

    private async Task<IEnumerable<AcessoColaboradorDto>> EnriquecerComColaboradoresAsync(IEnumerable<Domain.Entities.AcessoColaborador> acessos, CancellationToken cancellationToken)
    {
        var colaboradorRepo = _colaboradorRepoFactory();
        var cache = new Dictionary<int, ColaboradorDto>();
        var resultado = new List<AcessoColaboradorDto>();
        foreach (var acesso in acessos)
        {
            if (!cache.TryGetValue(acesso.ColaboradorId, out var colaboradorDto))
            {
                var colaborador = await colaboradorRepo.ObterPorId(acesso.ColaboradorId, cancellationToken)
                ?? throw new InvalidOperationException($"Colaborador associado ao acesso {acesso.Id} não encontrado.");
                colaboradorDto = colaborador.ToDto();
                cache[acesso.ColaboradorId] = colaboradorDto;
            }
            resultado.Add(acesso.ToDto(colaboradorDto));
        }
        return resultado;
    }
}
