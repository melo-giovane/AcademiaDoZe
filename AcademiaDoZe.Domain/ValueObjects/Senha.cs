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

}
