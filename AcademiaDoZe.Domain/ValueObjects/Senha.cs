using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using System;//Giovane Melo 
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Senha
{
    public string Valor { get; set; }

    private Senha (string valor)
    {
        Valor = valor;
    }

    public static Result<Senha> Criar(string valor)
    {
        if (NormalizadoService.TextoVazioOuNulo(valor))
            return Result<Senha>.Failure("Senha", "SENHA_OBRIGATORIA");


        var TextoLimpo = NormalizadoService.LimparEspacos(valor);
        if (TextoLimpo.Length < 6)
            return Result<Senha>.Failure("Senha", "SENHA_DIGITOS");

        return Result<Senha>.Success(new Senha(TextoLimpo));

    }

    public override string ToString() => "Senha Protegida";

}
