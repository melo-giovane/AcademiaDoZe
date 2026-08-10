using AcademiaDoZe.Domain.Common;
using AcademiaDoZe.Domain.Services;
using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }
    private Email(string valor)
    {
        Valor = valor;
    }
    public static Result<Email> Criar(string valor)
    {
        var TextoLimpo = NormalizadoService.LimparEspacos(valor);
        if (string.IsNullOrWhiteSpace(TextoLimpo) || !ValidarFormato(TextoLimpo))
            return Result<Email>.Failure("Email", "EMAIL_FORMATO");
        return Result<Email>.Success(new Email(TextoLimpo));
    }

    private static bool ValidarFormato(string email)
    {
        var partes = email.Split('@');
        if (partes.Length != 2) return false;
        if (string.IsNullOrWhiteSpace(partes[0])) return false;

        var dominio = partes[1];
        if (string.IsNullOrEmpty(dominio)) return false;
        if (dominio.StartsWith('.') || dominio.EndsWith('.')) return false;

        var labels = dominio.Split('.');
        if (labels.Length < 2) return false;
        if (labels.Any(l => string.IsNullOrWhiteSpace(l))) return false;

        return true;
    }
    public override string ToString() => Valor;




}
