using AcademiaDoZe.Domain.ValueObjects; //Giovane Melo
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

public sealed class Logradouro : Entity
{
    public Cep Cep { get;  }
    public string Nome { get;  }
    public string Bairro { get; }
    public string Cidade { get; }
    public string Estado { get; }
    public string Pais { get; }




    private Logradouro(int id, Cep cep, String nome, string bairro, string cidade, string estado, string pais) : base(id)
    {
        Cep = cep;
        Nome = nome;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Pais = pais;
    }
}
