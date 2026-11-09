using AcademiaDoZe.Infrastructure.Data;

namespace AcademiaDoZe.Infrastructure.Tests;

public class DbInitializerConcurrencyTests
{
    [Fact]
    public async Task ChamadasSimultaneas_AguardamUmaUnicaInicializacao()
    {
        var key = Guid.NewGuid().ToString();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int execucoes = 0;
        async Task Inicializar(CancellationToken token)
        {
            Interlocked.Increment(ref execucoes);
            await release.Task.WaitAsync(token);
        }
        var chamadas = Enumerable.Range(0, 4)
            .Select(_ => DbInitializer.InicializarUmaVezAsync(key, Inicializar, default)).ToArray();
        try
        {
            Assert.Equal(1, execucoes);
            Assert.All(chamadas, task => Assert.False(task.IsCompleted));
        }
        finally { release.TrySetResult(); }
        await Task.WhenAll(chamadas).WaitAsync(TimeSpan.FromSeconds(5));
        await DbInitializer.InicializarUmaVezAsync(key, Inicializar, default);
        Assert.Equal(1, execucoes);
    }

    [Fact]
    public async Task FalhaOuCancelamento_PermitemNovaTentativa()
    {
        var key = Guid.NewGuid().ToString();
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DbInitializer.InicializarUmaVezAsync(key, _ => throw new InvalidOperationException(), default));
        using var cts = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            DbInitializer.InicializarUmaVezAsync(key, token =>
            {
                cts.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }, cts.Token));
        int execucoes = 0;
        await DbInitializer.InicializarUmaVezAsync(key, _ =>
        {
            execucoes++;
            return Task.CompletedTask;
        }, default).WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal(1, execucoes);
    }

    [Fact]
    public async Task CancelarEspera_NaoCancelaInicializacao_NemBloqueiaOutroBanco()
    {
        var key = Guid.NewGuid().ToString();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var primeira = DbInitializer.InicializarUmaVezAsync(key, _ => release.Task, default);
        try
        {
            using var cts = new CancellationTokenSource();
            var espera = DbInitializer.InicializarUmaVezAsync(key, _ => throw new Exception("Não deve executar"), cts.Token);
            cts.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => espera);
            Assert.False(primeira.IsCompleted);
            await DbInitializer.InicializarUmaVezAsync(Guid.NewGuid().ToString(), _ => Task.CompletedTask, default)
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally { release.TrySetResult(); }
        await primeira.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
