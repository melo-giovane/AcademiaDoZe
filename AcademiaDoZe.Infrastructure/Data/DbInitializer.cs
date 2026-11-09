using AcademiaDoZe.Infrastructure.Exceptions;//Giovane Melo
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;
namespace AcademiaDoZe.Infrastructure.Data;

public static class DbInitializer
{
    private static readonly ConcurrentDictionary<string, bool> _bancosInicializados = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _inicializacaoLocks = new();
    public static async Task InicializarAsync(string connectionString, DatabaseType databaseType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;
        var key = $"{databaseType}:{connectionString}";
        await InicializarUmaVezAsync(key, async token =>
        {
            var scriptSql = ObterScript(databaseType);
            try
            {
                await using var connection = DbProvider.CreateConnection(connectionString, databaseType);
                await connection.OpenAsync(token);
                await using var command = DbProvider.CreateCommand(scriptSql, connection);
                await command.ExecuteNonQueryAsync(token);
            }
            catch (DbException ex)
            {
                throw new InfrastructureException("ERRO_INICIALIZAR_BANCO", $"Erro ao inicializar banco de dados: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    internal static async Task InicializarUmaVezAsync(string key, Func<CancellationToken, Task> inicializar, CancellationToken cancellationToken)
    {
        if (_bancosInicializados.ContainsKey(key)) return;
        // Cada banco tem sua própria trava. Não remover a trava: pode haver chamadas aguardando.
        var gate = _inicializacaoLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (_bancosInicializados.ContainsKey(key)) return;
            await inicializar(cancellationToken);
            // Falhas e cancelamentos não entram no cache, permitindo uma nova tentativa.
            _bancosInicializados.TryAdd(key, true);
        }
        finally
        {
            gate.Release();
        }
    }
    public static string ObterScript(DatabaseType databaseType)
    {
        var nomeScript = DbProvider.GetScriptName(databaseType);
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
        .FirstOrDefault(r => r.EndsWith(nomeScript, StringComparison.OrdinalIgnoreCase))
        ?? throw new InfrastructureException("SCRIPT_EMBARCADO_NAO_ENCONTRADO", $"Script SQL embarcado '{nomeScript}' não encontrado.");
        using var stream = assembly.GetManifestResourceStream(resourceName)
        ?? throw new InfrastructureException("ERRO_LEITURA_SCRIPT", $"Erro ao carregar o fluxo do script embarcado '{nomeScript}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

}
