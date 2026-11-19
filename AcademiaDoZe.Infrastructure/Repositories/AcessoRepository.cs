using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class AcessoRepository : BaseRepository
{
    protected AcessoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    protected async Task<(int Id, DateTime DataHora)?> AdicionarAcesso(int pessoaTipo, int pessoaId, DateTime dataHora, bool? entrada, CancellationToken cancellationToken)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        if (entrada.HasValue)
        {
            // A comparação precisa usar a mesma precisão que a coluna data_hora armazena.
            dataHora = _databaseType switch
            {
                DatabaseType.MySql => dataHora.AddTicks(-(dataHora.Ticks % TimeSpan.TicksPerSecond)),
                DatabaseType.SqlServer => DateTime.SpecifyKind(new SqlDateTime(dataHora).Value, dataHora.Kind),
                _ => dataHora
            };
        }
        // A conferência do estado e a gravação precisam ocorrer na mesma transação.
        var isolamento = _databaseType == DatabaseType.Sqlite ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted;
        await using var transaction = entrada.HasValue
            ? await connection.BeginTransactionAsync(isolamento, cancellationToken)
            : null;
        if (transaction != null && entrada is bool registrarEntrada)
        {
            if (!await BloquearPessoa(connection, transaction, pessoaTipo, pessoaId, cancellationToken))
                return null;

            var estado = await ObterEstadoAcesso(connection, transaction, pessoaTipo, pessoaId, dataHora, cancellationToken);
            if (estado.Dentro == registrarEntrada || estado.UltimoAcesso > dataHora)
                return null;
        }

        string query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
        await using var command = DbProvider.CreateCommand(query, connection);
        command.Transaction = transaction;
        command.AddParameter("@PessoaTipo", pessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", pessoaId, DbType.Int32);
        command.AddParameter("@DataHora", dataHora, DbType.DateTime);
        int id = await command.ExecuteScalarIdAsync(cancellationToken: cancellationToken);
        if (transaction != null && entrada is bool movimento)
        {
            const string inserirMovimento = "INSERT INTO tb_acesso_movimento (id_acesso, entrada) VALUES (@Id, @Entrada)";
            await using var movimentoCommand = DbProvider.CreateCommand(inserirMovimento, connection);
            movimentoCommand.Transaction = transaction;
            movimentoCommand.AddParameter("@Id", id, DbType.Int32);
            movimentoCommand.AddParameter("@Entrada", movimento ? 1 : 0, DbType.Int32);
            await movimentoCommand.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        return (id, dataHora);
    }

    protected static bool? LerEntrada(DbDataReader reader)
    {
        return reader["entrada"] is DBNull ? null : reader.GetInt32Value("entrada") != 0;
    }

    protected async Task<bool> ObterPresencaAtual(int pessoaTipo, int pessoaId, CancellationToken cancellationToken)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        var estado = await ObterEstadoAcesso(connection, null, pessoaTipo, pessoaId, DateTime.Now, cancellationToken);
        return estado.Dentro;
    }

    private async Task<bool> BloquearPessoa(DbConnection connection, DbTransaction transaction, int pessoaTipo, int pessoaId, CancellationToken cancellationToken)
    {
        // A linha da pessoa já existe mesmo quando ainda não há acessos para o dia.
        string tabela = pessoaTipo == 0 ? "tb_aluno" : "tb_colaborador";
        string colunaId = pessoaTipo == 0 ? "id_aluno" : "id_colaborador";
        string query = _databaseType switch
        {
            DatabaseType.SqlServer => $"SELECT {colunaId} FROM {tabela} WITH (UPDLOCK, ROWLOCK) WHERE {colunaId} = @PessoaId",
            DatabaseType.MySql => $"SELECT {colunaId} FROM {tabela} WHERE {colunaId} = @PessoaId FOR UPDATE",
            _ => $"SELECT {colunaId} FROM {tabela} WHERE {colunaId} = @PessoaId"
        };
        await using var command = DbProvider.CreateCommand(query, connection);
        command.Transaction = transaction;
        command.AddParameter("@PessoaId", pessoaId, DbType.Int32);
        return await command.ExecuteScalarAsync(cancellationToken) != null;
    }

    private static async Task<(bool Dentro, DateTime? UltimoAcesso)> ObterEstadoAcesso(
        DbConnection connection, DbTransaction? transaction, int pessoaTipo, int pessoaId, DateTime dataHora, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT acesso.data_hora, movimento.entrada
            FROM tb_acesso acesso
            LEFT JOIN tb_acesso_movimento movimento ON movimento.id_acesso = acesso.id_acesso
            WHERE acesso.pessoa_tipo = @PessoaTipo AND acesso.pessoa_id = @PessoaId
            ORDER BY acesso.data_hora DESC, acesso.id_acesso DESC
            """;
        await using var command = DbProvider.CreateCommand(query, connection);
        command.Transaction = transaction;
        command.AddParameter("@PessoaTipo", pessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", pessoaId, DbType.Int32);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        DateTime? ultimoAcesso = null;
        int registrosLegados = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            var horario = reader.GetDateTimeValue("data_hora");
            ultimoAcesso ??= horario;

            // Os acessos antigos sem indicação de entrada ou saída alternavam dentro de cada dia.
            bool mesmoDia = horario.Date == dataHora.Date;
            if (LerEntrada(reader) is bool entrada && (mesmoDia || registrosLegados == 0))
            {
                bool dentro = registrosLegados % 2 == 0 ? entrada : !entrada;
                return (dentro, ultimoAcesso);
            }
            if (!mesmoDia) break;
            registrosLegados++;
        }
        return (registrosLegados % 2 != 0, ultimoAcesso);
    }
}
