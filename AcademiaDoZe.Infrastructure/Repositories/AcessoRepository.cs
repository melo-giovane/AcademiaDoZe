using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories;

public abstract class AcessoRepository : BaseRepository
{
    protected AcessoRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType)
    {
    }

    protected async Task<int?> AdicionarAcesso(int pessoaTipo, int pessoaId, DateTime dataHora, bool? entrada, CancellationToken cancellationToken)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        // A conferência do estado e a gravação precisam ocorrer na mesma transação.
        var isolamento = _databaseType == DatabaseType.Sqlite ? IsolationLevel.Serializable : IsolationLevel.ReadCommitted;
        await using var transaction = entrada.HasValue
            ? await connection.BeginTransactionAsync(isolamento, cancellationToken)
            : null;
        if (transaction != null && entrada is bool registrarEntrada)
        {
            if (!await BloquearPessoa(connection, transaction, pessoaTipo, pessoaId, cancellationToken)
                || !await PodeRegistrar(connection, transaction, pessoaTipo, pessoaId, dataHora, registrarEntrada, cancellationToken))
                return null;
        }

        string query = FormatInsertQuery("INSERT INTO tb_acesso (pessoa_tipo, pessoa_id, data_hora) VALUES (@PessoaTipo, @PessoaId, @DataHora)");
        await using var command = DbProvider.CreateCommand(query, connection);
        command.Transaction = transaction;
        command.AddParameter("@PessoaTipo", pessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", pessoaId, DbType.Int32);
        command.AddParameter("@DataHora", dataHora, DbType.DateTime);
        int id = await command.ExecuteScalarIdAsync(cancellationToken: cancellationToken);
        if (transaction != null)
            await transaction.CommitAsync(cancellationToken);
        return id;
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

    private static async Task<bool> PodeRegistrar(DbConnection connection, DbTransaction transaction, int pessoaTipo, int pessoaId, DateTime dataHora, bool entrada, CancellationToken cancellationToken)
    {
        const string query = """
            SELECT COUNT(*) AS total, MAX(data_hora) AS ultimo_acesso
            FROM tb_acesso
            WHERE pessoa_tipo = @PessoaTipo AND pessoa_id = @PessoaId AND data_hora >= @Inicio AND data_hora < @Fim
            """;
        await using var command = DbProvider.CreateCommand(query, connection);
        command.Transaction = transaction;
        command.AddParameter("@PessoaTipo", pessoaTipo, DbType.Int32);
        command.AddParameter("@PessoaId", pessoaId, DbType.Int32);
        command.AddParameter("@Inicio", dataHora.Date, DbType.DateTime);
        command.AddParameter("@Fim", dataHora.Date.AddDays(1), DbType.DateTime);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int total = reader.GetInt32Value("total");
        return (total % 2 == 0) == entrada && (total == 0 || reader.GetDateTimeValue("ultimo_acesso") <= dataHora);
    }
}
