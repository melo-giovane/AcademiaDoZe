using AcademiaDoZe.Domain.Enums;//Giovane Melo
using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;


namespace AcademiaDoZe.Domain.Entities;

public class Matricula : Entity
{
    public Aluno AlunoMatricula { get; private set; }
    public MatriculaPlano Plano { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly DataFim { get; private set; }
    public String Objetivo { get; private set; } 
    public MatriculaRestricoes Restricoes { get; private set; }
    public string ObservacoesMedicas { get; private set; }
    public Arquivo? LaudoMedico { get; private set; }

    private Matricula(int id, Aluno alunoMatricula, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas, 
        Arquivo? laudoMedico,string observacoesMedicas) : base(id)
    {
        AlunoMatricula = alunoMatricula;
        Plano = plano;
        DataInicio = dataInicio;
        DataFim = dataFim;
        Restricoes = restricoesMedicas;
        Objetivo = objetivo;
        LaudoMedico = laudoMedico;
        ObservacoesMedicas = observacoesMedicas;
    }

}
