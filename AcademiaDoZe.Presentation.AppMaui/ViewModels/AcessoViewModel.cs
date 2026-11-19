using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private IEnumerable<AlunoDto> _alunos = [];
    private IEnumerable<ColaboradorDto> _colaboradores = [];
    private DateOnly _dataHistorico;

    public string[] TiposPessoa { get; } = ["Aluno", "Colaborador"];
    public ObservableCollection<PessoaDto> Pessoas { get; } = [];
    public ObservableCollection<AcessoRegistro> Registros { get; } = [];

    private string _tipoPessoa = "Aluno";
    public string TipoPessoa
    {
        get => _tipoPessoa;
        set
        {
            if (SetProperty(ref _tipoPessoa, value))
                FiltrarPessoas();
        }
    }
    private string _busca = string.Empty;
    public string Busca
    {
        get => _busca;
        set
        {
            if (SetProperty(ref _busca, value))
                FiltrarPessoas();
        }
    }
    private PessoaDto? _pessoaSelecionada;
    public PessoaDto? PessoaSelecionada
    {
        get => _pessoaSelecionada;
        set
        {
            if (SetProperty(ref _pessoaSelecionada, value))
            {
                Mensagem = string.Empty;
                AtualizarAcao();
            }
        }
    }
    private string _mensagem = string.Empty;
    public string Mensagem
    {
        get => _mensagem;
        private set => SetProperty(ref _mensagem, value);
    }
    private bool EstaNaAcademia => PessoaSelecionada != null && Registros.FirstOrDefault(registro =>
        registro.Pessoa.Id == PessoaSelecionada.Id && registro.Pessoa.GetType() == PessoaSelecionada.GetType())?.Entrada == true;
    public string Situacao => PessoaSelecionada == null ? string.Empty : EstaNaAcademia ? "Dentro da academia" : "Fora da academia";
    public string Acao => EstaNaAcademia ? "Registrar saída" : "Registrar entrada";

    public AcessoViewModel(IAlunoService alunoService, IColaboradorService colaboradorService,
        IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService)
    {
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _acessoAlunoService = acessoAlunoService;
        _acessoColaboradorService = acessoColaboradorService;
        Title = "Controle de Acesso";
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            _dataHistorico = default;
            AtualizarAcao();
            Mensagem = string.Empty;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var alunosTask = _alunoService.ObterTodosAsync(cts.Token);
            var colaboradoresTask = _colaboradorService.ObterTodosAsync(cts.Token);
            await Task.WhenAll(alunosTask, colaboradoresTask);
            _alunos = await alunosTask;
            _colaboradores = await colaboradoresTask;
            FiltrarPessoas();
            await CarregarRegistrosAsync(DateOnly.FromDateTime(DateTime.Today), cts.Token);
        }
        catch (Exception ex)
        {
            Mensagem = $"Erro ao carregar os acessos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            AtualizarAcao();
        }
    }

    private bool PodeRegistrar() => !IsBusy && PessoaSelecionada != null && _dataHistorico != default;

    [RelayCommand(CanExecute = nameof(PodeRegistrar))]
    private async Task RegistrarAsync()
    {
        var pessoa = PessoaSelecionada;
        if (pessoa == null || !PodeRegistrar()) return;
        var dataHora = DateTime.Now;
        if (DateOnly.FromDateTime(dataHora) != _dataHistorico)
        {
            await CarregarAsync();
            return;
        }

        bool entrada = !EstaNaAcademia;
        try
        {
            IsBusy = true;
            AtualizarAcao();
            Mensagem = string.Empty;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            AcessoRegistro registro;
            if (pessoa is AlunoDto aluno)
            {
                var acesso = await _acessoAlunoService.RegistrarAsync(aluno.Id, dataHora, entrada, cts.Token);
                registro = new AcessoRegistro(acesso.Id, acesso.AlunoAcesso, acesso.DataHora, entrada);
            }
            else
            {
                var acesso = await _acessoColaboradorService.RegistrarAsync(pessoa.Id, dataHora, entrada, cts.Token);
                registro = new AcessoRegistro(acesso.Id, acesso.ColaboradorAcesso, acesso.DataHora, entrada);
            }
            Registros.Insert(0, registro);
            Mensagem = $"{registro.Movimento} registrada às {registro.DataHora:HH:mm:ss}.";
        }
        catch (Exception ex)
        {
            Mensagem = ex.Message;
        }
        finally
        {
            IsBusy = false;
            AtualizarAcao();
        }
    }

    private void FiltrarPessoas()
    {
        string busca = (Busca ?? string.Empty).Trim();
        string cpf = string.Concat(busca.Where(char.IsDigit));
        var compararNome = CultureInfo.CurrentCulture.CompareInfo;
        IEnumerable<PessoaDto> pessoas = TipoPessoa == "Aluno" ? _alunos : _colaboradores;
        PessoaSelecionada = null;
        Pessoas.Clear();
        foreach (var pessoa in pessoas.OrderBy(pessoa => pessoa.Nome))
        {
            if (compararNome.IndexOf(pessoa.Nome, busca, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0
                || (cpf.Length > 0 && string.Concat(pessoa.Cpf.Where(char.IsDigit)).Contains(cpf)))
                Pessoas.Add(pessoa);
        }
    }

    private async Task CarregarRegistrosAsync(DateOnly data, CancellationToken cancellationToken)
    {
        var alunosTask = _acessoAlunoService.ObterAcessosPorAlunoPeriodoAsync(inicio: data, fim: data, cancellationToken: cancellationToken);
        var colaboradoresTask = _acessoColaboradorService.ObterAcessosPorColaboradorPeriodoAsync(inicio: data, fim: data, cancellationToken: cancellationToken);
        await Task.WhenAll(alunosTask, colaboradoresTask);
        var alunos = (await alunosTask).Select(acesso => new AcessoRegistro(acesso.Id, acesso.AlunoAcesso, acesso.DataHora));
        var colaboradores = (await colaboradoresTask).Select(acesso => new AcessoRegistro(acesso.Id, acesso.ColaboradorAcesso, acesso.DataHora));
        var registros = alunos.Concat(colaboradores)
            .GroupBy(registro => (registro.Pessoa.GetType(), registro.Pessoa.Id))
            .SelectMany(grupo => grupo.OrderBy(registro => registro.DataHora).ThenBy(registro => registro.Id)
                .Select((registro, indice) => registro with { Entrada = indice % 2 == 0 }))
            .OrderByDescending(registro => registro.DataHora).ThenByDescending(registro => registro.Id);
        Registros.Clear();
        foreach (var registro in registros)
            Registros.Add(registro);
        _dataHistorico = data;
    }

    private void AtualizarAcao()
    {
        OnPropertyChanged(nameof(Situacao));
        OnPropertyChanged(nameof(Acao));
        RegistrarCommand.NotifyCanExecuteChanged();
    }
}

public record AcessoRegistro(int Id, PessoaDto Pessoa, DateTime DataHora, bool Entrada = false)
{
    public string TipoPessoa => Pessoa is AlunoDto ? "Aluno" : "Colaborador";
    public string Movimento => Entrada ? "Entrada" : "Saída";
}
