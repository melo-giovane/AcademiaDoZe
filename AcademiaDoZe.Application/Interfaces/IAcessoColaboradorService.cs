using AcademiaDoZe.Application.DTOs;//Giovane Melo
namespace AcademiaDoZe.Application.Interfaces;
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas aos acessos (entradas/saídas) de colaboradores.
/// </summary>
public interface IAcessoColaboradorService
{
    /// <summary>
    /// Obtém um registro de acesso pelo seu identificador único.
    /// </summary>
    Task<AcessoColaboradorDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os registros de acesso cadastrados.
    /// </summary>
    Task<IEnumerable<AcessoColaboradorDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Registra um novo acesso de colaborador.
    /// </summary>
    Task<AcessoColaboradorDto> AdicionarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atualiza um registro de acesso existente.
    /// </summary>
    Task<AcessoColaboradorDto> AtualizarAsync(AcessoColaboradorDto acessoDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove um registro de acesso pelo identificador.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém acessos filtrados por colaborador e período (todos os parâmetros opcionais).
    /// </summary>
    Task<IEnumerable<AcessoColaboradorDto>> ObterAcessosPorColaboradorPeriodoAsync(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém o último acesso registrado para um colaborador.
    /// </summary>
    Task<AcessoColaboradorDto?> ObterUltimoAcessoAsync(int colaboradorId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Calcula as horas trabalhadas por um colaborador em uma data específica.
    /// </summary>
    Task<TimeSpan> ObterHorasTrabalhadasNoDiaAsync(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default);
}
