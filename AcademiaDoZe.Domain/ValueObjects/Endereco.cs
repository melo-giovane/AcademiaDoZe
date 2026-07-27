using AcademiaDoZe.Domain.Entities;//Giovane Melo
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.ValueObjects;

internal class Endereco
{
    public required string Cidade { get; set; }
    public required string Estado { get; set; }
    public required Logradouro Logradouro { get; set; }
    public int Numero { get; set; }
    public string Bairro { get; set; }

    public Endereco(Logradouro logradouro, int numero, string bairro)
    {
        Logradouro = logradouro;
        Numero = numero;
        Bairro = bairro;
    }

}
