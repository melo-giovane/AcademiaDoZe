using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Email
{
    public string Valor { get; }
    public Email(string valor)
    {
        Valor = valor;
    }
}
