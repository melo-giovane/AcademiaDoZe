using AcademiaDoZe.Domain.Common;
using System; //Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateOnly DataHora { get; private set; }

    private AcessoColaborador(int id, Colaborador colaborador, DateOnly dataHora) : base(id)
    {
        Colaborador = colaborador;
        DataHora = dataHora;
    }

    public static Result<AcessoColaborador> Criar(int id, Colaborador colaborador, DateOnly dataHora)
    {
        var notifications = new List<Notification>();
        if (colaborador is null)
            notifications.Add(new Notification("Colaborador", "COLABORADOR_OBRIGATORIO"));
        if (dataHora == default)
            notifications.Add(new Notification("Datahora", "DATAHORA_OBRIGATORIA"));
        if (notifications.Count != 0)
            return Result<AcessoColaborador>.Failure(notifications);

        var acessoColaborador = new AcessoColaborador(id, colaborador!, dataHora);

        return Result<AcessoColaborador>.Success(acessoColaborador);
    }
}
