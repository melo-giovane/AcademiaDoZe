using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using AcademiaDoZe.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Tests;

public class LogradouroCepLookupTests
{
    [Theory]
    [InlineData("01001000", "01001000")]
    [InlineData("01001-000", "01001000")]
    [InlineData("01001000", "01001-000")]
    [InlineData(" 01001-000 ", "01001-000")]
    [InlineData("01.001-000", "01001000")]
    public async Task Busca_E_VerificacaoDeDuplicidade_AceitamCepComMascara(string salvo, string buscado)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var setup = connection.CreateCommand();
        setup.CommandText = """
            CREATE TABLE tb_logradouro (
                id_logradouro INTEGER PRIMARY KEY, cep TEXT, nome TEXT,
                bairro TEXT, cidade TEXT, estado TEXT, pais TEXT);
            INSERT INTO tb_logradouro VALUES
                (1, @Cep, 'Praça da Sé', 'Sé', 'São Paulo', 'SP', 'Brasil');
            """;
        setup.Parameters.AddWithValue("@Cep", salvo);
        await setup.ExecuteNonQueryAsync();
        await using var repository = new MemoryRepository(connection);
        var cep = Cep.Criar(buscado).Value!;

        var encontrado = await repository.ObterPorCep(cep);

        Assert.NotNull(encontrado);
        Assert.Equal(1, encontrado.Id);
        Assert.Equal("01001000", encontrado.Cep.Valor);
        Assert.True(await repository.CepJaExiste(cep));
        Assert.False(await repository.CepJaExiste(cep, 1));
        var inexistente = Cep.Criar("99999999").Value!;
        Assert.Null(await repository.ObterPorCep(inexistente));
        Assert.False(await repository.CepJaExiste(inexistente));
    }

    private sealed class MemoryRepository(SqliteConnection connection)
        : LogradouroRepository(connection.ConnectionString, DatabaseType.Sqlite)
    {
        protected override Task<DbConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<DbConnection>(connection);
    }
}
