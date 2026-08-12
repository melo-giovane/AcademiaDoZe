using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using AcademiaDoZe.Domain.Entities;
using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get;  }
    
    private Cpf(string valor)
    {
        Valor = valor;
    }
    public static Result<Cpf> Criar(String valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Cpf>.Failure("Cpf", "CPF_OBRIGATORIO");

        var textoLimpo = NormalizadoService.LimparEDigitos(valor);
        if(textoLimpo.Length != 11)
            return Result<Cpf>.Failure("Cpf", "CPF_DIGITOS");

        return Result<Cpf>.Success(new Cpf(textoLimpo));
    }

    public override string ToString() => Valor;
}
