using System; //Giovane Melo    
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

internal class AcessoAluno : Entity
{
    Aluno Aluno { get; set; }
    DateOnly Chegada { get; set; }
    DateOnly Saida { get; set; }

    public AcessoAluno(int id, Aluno aluno, DateOnly chegada, DateOnly saida): base(id)
    {
        Aluno = aluno;
        Chegada = chegada;
        Saida = saida;
    }
}
