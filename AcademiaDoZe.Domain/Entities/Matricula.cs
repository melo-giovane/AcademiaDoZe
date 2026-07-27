using AcademiaDoZe.Domain.Enums;//Giovane Melo
using System;
using System.Collections.Generic;
using System.Text;


namespace AcademiaDoZe.Domain.Entities;

internal class Matricula : Entity
{
    public Aluno Aluno { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    // public Objetivo Objetivo { get; private set; } 
    public MatriculaRestricoes Restricoes { get; private set; }

    public Matricula(int id, Aluno aluno, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim, MatriculaRestricoes restricoes) : base(id)
    {
        Aluno = aluno;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Restricoes = restricoes;
    }

}
