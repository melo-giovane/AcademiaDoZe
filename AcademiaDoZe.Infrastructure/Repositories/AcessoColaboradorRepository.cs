using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoColaboradorRepository : AcessoRepository, IAcessoColaboradorRepository
{
    private const int PessoaTipo = 1;
    public AcessoColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }
    private static string BaseSelectQuery => $"""
        SELECT
        ac.id_acesso AS id_acesso_colaborador, ac.data_hora, movimento.entrada,
        c.id_colaborador, c.cpf, c.nome AS colaborador_nome, c.nascimento, c.telefone, c.email,
        c.logradouro_id, c.numero, c.complemento, c.senha, c.foto, c.admissao, c.tipo, c.vinculo,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso ac
        LEFT JOIN tb_acesso_movimento movimento ON movimento.id_acesso = ac.id_acesso
        INNER JOIN tb_colaborador c ON ac.pessoa_id = c.id_colaborador
        INNER JOIN tb_logradouro l ON c.logradouro_id = l.id_logradouro
        WHERE ac.pessoa_tipo = {PessoaTipo}
        """;

    public static AcessoColaborador Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso_colaborador");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");
            var colaborador = ColaboradorRepository.Map(reader, "colaborador_nome");
            var result = AcessoColaborador.Criar(id, colaborador, dataHora, LerEntrada(reader));
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso de colaborador ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_COLABORADOR", $"Erro ao mapear dados do acesso de colaborador: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de colaborador por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de colaboradores: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Adicionar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        return await AdicionarAsync(entity, entity.Entrada, cancellationToken)
            ?? throw new InfrastructureException("ACESSO_COLABORADOR_NAO_DISPONIVEL", "A entrada ou saída do colaborador não está disponível.");
    }

    public Task<AcessoColaborador?> Registrar(AcessoColaborador acesso, bool entrada, CancellationToken cancellationToken = default)
    {
        return AdicionarAsync(acesso, entrada, cancellationToken);
    }

    private async Task<AcessoColaborador?> AdicionarAsync(AcessoColaborador entity, bool? entrada, CancellationToken cancellationToken)
    {
        try
        {
            var registro = await AdicionarAcesso(PessoaTipo, entity.ColaboradorId, entity.DataHora, entrada, cancellationToken);
            if (registro is not { } gravado) return null;
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, gravado.Id);
            if (entity.DataHora != gravado.DataHora)
                typeof(AcessoColaborador).GetProperty(nameof(AcessoColaborador.DataHora))?.SetValue(entity, gravado.DataHora);
            if (entity.Entrada != entrada)
                typeof(AcessoColaborador).GetProperty(nameof(AcessoColaborador.Entrada))?.SetValue(entity, entrada);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_COLABORADOR", $"Erro ao adicionar acesso para o colaborador ID {entity.ColaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador> Atualizar(AcessoColaborador entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"UPDATE tb_acesso SET pessoa_id = @ColaboradorId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = {PessoaTipo}";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@ColaboradorId", entity.ColaboradorId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de colaborador encontrado com o ID {entity.Id} para atualização.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_COLABORADOR", $"Erro ao atualizar acesso de colaborador ID {entity.Id}: {ex.Message}", ex);
        }
    }

    public async Task<bool> Remover(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"DELETE FROM tb_acesso WHERE id_acesso = @Id AND pessoa_tipo = {PessoaTipo}";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_COLABORADOR", $"Erro ao remover acesso de colaborador ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoColaborador>> ObterAcessosPorColaboradorPeriodo(int? colaboradorId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filtros = new List<string>();
            if (colaboradorId.HasValue) filtros.Add("ac.pessoa_id = @ColaboradorId");
            if (inicio.HasValue) filtros.Add("ac.data_hora >= @Inicio");
            if (fim.HasValue) filtros.Add("ac.data_hora < @FimExclusivo");
            string where = filtros.Count > 0 ? $" AND {string.Join(" AND ", filtros)}" : string.Empty;
            string query = $"{BaseSelectQuery}{where} ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (colaboradorId.HasValue) command.AddParameter("@ColaboradorId", colaboradorId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@FimExclusivo", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoColaborador>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos de colaboradores por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoColaborador?> ObterUltimoAcesso(int colaboradorId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND ac.pessoa_id = @ColaboradorId ORDER BY ac.data_hora DESC, ac.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do colaborador ID {colaboradorId}: {ex.Message}", ex);
        }
    }

    public async Task<TimeSpan> ObterHorasTrabalhadasNoDia(int colaboradorId, DateOnly data, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"SELECT data_hora FROM tb_acesso WHERE pessoa_tipo = {PessoaTipo} AND pessoa_id = @ColaboradorId AND data_hora >= @Inicio AND data_hora < @Fim ORDER BY data_hora";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@ColaboradorId", colaboradorId, DbType.Int32);
            command.AddParameter("@Inicio", data.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            command.AddParameter("@Fim", data.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var registros = new List<DateTime>();
            while (await reader.ReadAsync(cancellationToken))
            {
                registros.Add(reader.GetDateTimeValue("data_hora"));
            }
            var total = TimeSpan.Zero;
            for (int i = 0; i + 1 < registros.Count; i += 2)
            {
                total += registros[i + 1] - registros[i];
            }
            return total;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORAS_TRABALHADAS", $"Erro ao obter horas trabalhadas do colaborador ID {colaboradorId} em {data:yyyy-MM-dd}: {ex.Message}", ex);
        }
    }
}
