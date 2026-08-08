using System;//Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

public record Arquivo
{
    public byte[] Conteudo { get;  }

    private Arquivo(byte[] conteudo)
    {
        Conteudo = conteudo;
    }
}
