using AcademiaDoZe.Domain.Entities;//Giovane Melo
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public class AcessoAlunoRepository : AcessoRepository, IAcessoAlunoRepository
{
    private const int PessoaTipo = 0;
    public AcessoAlunoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }
    private static string BaseSelectQuery => $"""
        SELECT
        aa.id_acesso AS id_acesso_aluno, aa.data_hora, movimento.entrada,
        a.id_aluno, a.cpf, a.nome AS aluno_nome, a.nascimento, a.telefone, a.email, a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
        l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
        FROM tb_acesso aa
        LEFT JOIN tb_acesso_movimento movimento ON movimento.id_acesso = aa.id_acesso
        INNER JOIN tb_aluno a ON aa.pessoa_id = a.id_aluno
        INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
        WHERE aa.pessoa_tipo = {PessoaTipo}
        """;

    public static AcessoAluno Map(DbDataReader reader)
    {
        try
        {
            int id = reader.GetInt32Value("id_acesso_aluno");
            DateTime dataHora = reader.GetDateTimeValue("data_hora");
            var aluno = AlunoRepository.Map(reader, "aluno_nome");
            var result = AcessoAluno.Criar(id, aluno, dataHora, LerEntrada(reader));
            if (result.IsFailure)
            {
                throw new InfrastructureException("ERRO_DOMINIO_MAPEAMENTO", $"Erro de domínio ao mapear acesso de aluno ID {id}: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
            }
            return result.Value!;
        }
        catch (Exception ex) when (ex is not InfrastructureException)
        {
            throw new InfrastructureException("ERRO_MAPEAMENTO_ACESSO_ALUNO", $"Erro ao mapear dados do acesso de aluno: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterPorId(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND aa.id_acesso = @Id";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", id, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_POR_ID", $"Erro ao obter acesso de aluno por ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterTodos(CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} ORDER BY aa.data_hora DESC, aa.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_TODOS", $"Erro ao obter todos os acessos de alunos: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Adicionar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        return await AdicionarAsync(entity, entity.Entrada, cancellationToken)
            ?? throw new InfrastructureException("ACESSO_ALUNO_NAO_DISPONIVEL", "A entrada ou saída do aluno não está disponível.");
    }

    public Task<AcessoAluno?> Registrar(AcessoAluno acesso, bool entrada, CancellationToken cancellationToken = default)
    {
        return AdicionarAsync(acesso, entrada, cancellationToken);
    }

    private async Task<AcessoAluno?> AdicionarAsync(AcessoAluno entity, bool? entrada, CancellationToken cancellationToken)
    {
        try
        {
            var registro = await AdicionarAcesso(PessoaTipo, entity.AlunoId, entity.DataHora, entrada, cancellationToken);
            if (registro is not { } gravado) return null;
            var idProperty = typeof(Entity).GetProperty("Id");
            idProperty?.SetValue(entity, gravado.Id);
            if (entity.DataHora != gravado.DataHora)
                typeof(AcessoAluno).GetProperty(nameof(AcessoAluno.DataHora))?.SetValue(entity, gravado.DataHora);
            if (entity.Entrada != entrada)
                typeof(AcessoAluno).GetProperty(nameof(AcessoAluno.Entrada))?.SetValue(entity, entrada);
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ADICIONAR_ACESSO_ALUNO", $"Erro ao adicionar acesso para o aluno ID {entity.AlunoId}: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno> Atualizar(AcessoAluno entity, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"UPDATE tb_acesso SET pessoa_id = @AlunoId, data_hora = @DataHora WHERE id_acesso = @Id AND pessoa_tipo = {PessoaTipo}";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Id", entity.Id, DbType.Int32);
            command.AddParameter("@AlunoId", entity.AlunoId, DbType.Int32);
            command.AddParameter("@DataHora", entity.DataHora, DbType.DateTime);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            if (rowsAffected == 0)
            {
                throw new InfrastructureException("REGISTRO_NAO_ENCONTRADO", $"Nenhum acesso de aluno encontrado com o ID {entity.Id} para atualização.");
            }
            return entity;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_ATUALIZAR_ACESSO_ALUNO", $"Erro ao atualizar acesso de aluno ID {entity.Id}: {ex.Message}", ex);
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
            throw new InfrastructureException("ERRO_REMOVER_ACESSO_ALUNO", $"Erro ao remover acesso de aluno ID {id}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AcessoAluno>> ObterAcessosPorAlunoPeriodo(int? alunoId = null, DateOnly? inicio = null, DateOnly? fim = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filtros = new List<string>();
            if (alunoId.HasValue) filtros.Add("aa.pessoa_id = @AlunoId");
            if (inicio.HasValue) filtros.Add("aa.data_hora >= @Inicio");
            if (fim.HasValue) filtros.Add("aa.data_hora < @FimExclusivo");
            string where = filtros.Count > 0 ? $" AND {string.Join(" AND ", filtros)}" : string.Empty;
            string query = $"{BaseSelectQuery}{where} ORDER BY aa.data_hora DESC, aa.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            if (alunoId.HasValue) command.AddParameter("@AlunoId", alunoId.Value, DbType.Int32);
            if (inicio.HasValue) command.AddParameter("@Inicio", inicio.Value.ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            if (fim.HasValue) command.AddParameter("@FimExclusivo", fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DbType.DateTime);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessos = new List<AcessoAluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                acessos.Add(Map(reader));
            }
            return acessos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ACESSOS_PERIODO", $"Erro ao obter acessos de alunos por período: {ex.Message}", ex);
        }
    }

    public async Task<AcessoAluno?> ObterUltimoAcesso(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            string query = $"{BaseSelectQuery} AND aa.pessoa_id = @AlunoId ORDER BY aa.data_hora DESC, aa.id_acesso DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@AlunoId", alunoId, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ULTIMO_ACESSO", $"Erro ao obter último acesso do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<bool> EstaNaAcademia(int alunoId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await ObterPresencaAtual(PessoaTipo, alunoId, cancellationToken);
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_VERIFICAR_PRESENCA", $"Erro ao verificar presença do aluno ID {alunoId}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<TimeOnly, int>> ObterHorarioMaisProcuradoPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            string horaExpr = GetDateHourExpression("data_hora");
            string mesExpr = GetDateMonthExpression("data_hora");
            string query = $"SELECT {horaExpr} AS hora, COUNT(*) AS qtd FROM tb_acesso WHERE pessoa_tipo = {PessoaTipo} AND {mesExpr} = @Mes GROUP BY {horaExpr} ORDER BY qtd DESC";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var resultado = new Dictionary<TimeOnly, int>();
            while (await reader.ReadAsync(cancellationToken))
            {
                int hora = reader.GetInt32Value("hora");
                int qtd = reader.GetInt32Value("qtd");
                resultado[new TimeOnly(hora, 0)] = qtd;
            }
            return resultado;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_HORARIO_MAIS_PROCURADO", $"Erro ao obter horário mais procurado no mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<int, TimeSpan>> ObterPermanenciaMediaPorMes(int mes, CancellationToken cancellationToken = default)
    {
        try
        {
            string mesExpr = GetDateMonthExpression("data_hora");
            string query = $"SELECT pessoa_id AS aluno_id, data_hora FROM tb_acesso WHERE pessoa_tipo = {PessoaTipo} AND {mesExpr} = @Mes ORDER BY pessoa_id, data_hora";
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@Mes", mes, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var acessosPorAluno = new Dictionary<int, List<DateTime>>();
            while (await reader.ReadAsync(cancellationToken))
            {
                int alunoId = reader.GetInt32Value("aluno_id");
                DateTime dataHora = reader.GetDateTimeValue("data_hora");
                if (!acessosPorAluno.TryGetValue(alunoId, out var lista))
                {
                    lista = [];
                    acessosPorAluno[alunoId] = lista;
                }
                lista.Add(dataHora);
            }
            var totalDuracao = TimeSpan.Zero;
            int totalPares = 0;
            foreach (var lista in acessosPorAluno.Values)
            {
                for (int i = 0; i + 1 < lista.Count; i += 2)
                {
                    totalDuracao += lista[i + 1] - lista[i];
                    totalPares++;
                }
            }
            var media = totalPares > 0 ? TimeSpan.FromTicks(totalDuracao.Ticks / totalPares) : TimeSpan.Zero;
            return new Dictionary<int, TimeSpan> { [mes] = media };
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_PERMANENCIA_MEDIA", $"Erro ao obter permanência média no mês {mes}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Aluno>> ObterAlunosSemAcessoNosUltimosDias(int dias, CancellationToken cancellationToken = default)
    {
        try
        {
            string currentDate = GetCurrentDateFunction();
            string limite = GetDateAddDaysExpression(currentDate, "@DiasNegativos");
            string query = $"""
                SELECT
                a.id_aluno, a.cpf, a.nome, a.nascimento, a.telefone, a.email,
                a.logradouro_id, a.numero, a.complemento, a.senha, a.foto,
                l.id_logradouro, l.cep, l.nome AS logradouro_nome, l.bairro, l.cidade, l.estado, l.pais
                FROM tb_aluno a
                INNER JOIN tb_logradouro l ON a.logradouro_id = l.id_logradouro
                WHERE a.id_aluno NOT IN (
                    SELECT DISTINCT aa.pessoa_id FROM tb_acesso aa
                    WHERE aa.pessoa_tipo = {PessoaTipo} AND aa.data_hora >= {limite}
                )
                ORDER BY a.nome
                """;
            await using var command = await CreateCommandAsync(query, cancellationToken);
            command.AddParameter("@DiasNegativos", -dias, DbType.Int32);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var alunos = new List<Aluno>();
            while (await reader.ReadAsync(cancellationToken))
            {
                alunos.Add(AlunoRepository.Map(reader));
            }
            return alunos;
        }
        catch (DbException ex)
        {
            throw new InfrastructureException("ERRO_OBTER_ALUNOS_SEM_ACESSO", $"Erro ao obter alunos sem acesso nos últimos {dias} dias: {ex.Message}", ex);
        }
    }
}
