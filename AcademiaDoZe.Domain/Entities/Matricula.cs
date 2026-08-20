using AcademiaDoZe.Domain.Common;
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

    public static Result<Matricula> Criar(int id, Aluno alunoMatricula, MatriculaPlano plano, DateOnly dataInicio, DateOnly dataFim, string objetivo, MatriculaRestricoes restricoesMedicas, 
        Arquivo? laudoMedico,string observacoesMedicas)
    {
        var notifications = new List<Notification>();
        if(alunoMatricula is null)
            notifications.Add(new Notification("ALUNO", "ALUNO_OBRIGATORIO"));
        if(!Enum.IsDefined(plano))
            notifications.Add(new Notification("PLANO", "PLANO_INVALIDO"));
        if (dataInicio < DateOnly.FromDateTime(DateTime.Today))
            notifications.Add(new Notification("DATAINICIO", "DATAINICIO_INVALIDA"));
        if (dataFim < DateOnly.FromDateTime(DateTime.Today))
            notifications.Add(new Notification("DATAFIM", "DATAFIM_INVALIDA"));
        if (!Enum.IsDefined(restricoesMedicas))
            notifications.Add(new Notification("RESTRICOES", "RESTRICOES_INVALIDAS"));

        if(notifications.Count != 0)
            return Result<Matricula>.Failure(notifications);

        var matricula = new Matricula(id, alunoMatricula!, plano, dataInicio, dataFim, objetivo, restricoesMedicas, laudoMedico, observacoesMedicas);
        return Result<Matricula>.Success(matricula);
        
    }

}
