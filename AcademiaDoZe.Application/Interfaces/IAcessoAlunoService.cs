using AcademiaDoZe.Application.DTOs;//Giovane Melo
namespace AcademiaDoZe.Application.Interfaces;
/// <summary>
/// Contrato de serviço para operações de negócios relacionadas aos acessos (entradas/saídas) de alunos.
/// </summary>
public interface IAcessoAlunoService
{
    /// <summary>
    /// Obtém um registro de acesso pelo seu identificador único.
    /// </summary>
    Task<AcessoAlunoDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém todos os registros de acesso cadastrados.
    /// </summary>
    Task<IEnumerable<AcessoAlunoDto>> ObterTodosAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Registra um novo acesso de aluno.
    /// </summary>
    Task<AcessoAlunoDto> AdicionarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Atualiza um registro de acesso existente.
    /// </summary>
    Task<AcessoAlunoDto> AtualizarAsync(AcessoAlunoDto acessoDto, CancellationToken cancellationToken = default);
    /// <summary>
    /// Remove um registro de acesso pelo identificador.
    /// </summary>
    Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém acessos filtrados por aluno e período (todos os parâmetros opcionais).
    /// </summary>
    Task<IEnumerable<AcessoAlunoDto>> ObterAcessosPorAlunoPeriodoAsync(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém o último acesso registrado para um aluno.
    /// </summary>
    Task<AcessoAlunoDto?> ObterUltimoAcessoAsync(int alunoId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Verifica se o aluno está atualmente na academia (número ímpar de acessos no dia).
    /// </summary>
    Task<bool> EstaNaAcademiaAsync(int alunoId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém o horário de maior procura no mês informado, agrupado por hora.
    /// </summary>
    Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMesAsync(int mes, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém a permanência média dos alunos no mês informado.
    /// </summary>
    Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMesAsync(int mes, CancellationToken cancellationToken = default);
    /// <summary>
    /// Obtém os alunos que não registraram acesso nos últimos N dias.
    /// </summary>
    Task<IEnumerable<AlunoDto>> ObterAlunosSemAcessoNosUltimosDiasAsync(int dias, CancellationToken cancellationToken = default);
}
