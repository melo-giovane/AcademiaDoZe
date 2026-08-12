using AcademiaDoZe.Domain.Common;
using System; //Giovane Melo    
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoAluno : Entity
{
    public Aluno Aluno { get; private set; }
    public DateOnly DataHora { get; private set; }

    private AcessoAluno(int id, Aluno aluno, DateOnly dataHora): base(id)
    {
        Aluno = aluno;
        DataHora = dataHora;
    }

    public static Result<AcessoAluno> Criar(int id, Aluno aluno, DateOnly dataHora)
    {
       var notifications = new List<Notification>();

       if (aluno is null)
           notifications.Add(new Notification("Aluno", "ALUNO_OBRIGATORIO"));

       if (dataHora == default)
           notifications.Add(new Notification("DataHora", "DATA_HORA_OBRIGATORIO"));

       if (notifications.Count != 0)
           return Result<AcessoAluno>.Failure(notifications);

       var acessoAluno = new AcessoAluno(id, aluno!, dataHora);
       return Result<AcessoAluno>.Success(acessoAluno);
    }
}
