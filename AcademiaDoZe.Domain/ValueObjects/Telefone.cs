using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Telefone
{
    public string Valor { get; }

    public Telefone(string valor)
    {
        Valor = valor;
    }
}
