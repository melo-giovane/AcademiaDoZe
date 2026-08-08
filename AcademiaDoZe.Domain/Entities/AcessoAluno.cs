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
}
