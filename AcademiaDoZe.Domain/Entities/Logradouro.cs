using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

internal class Logradouro : Entity
{
    public Cep Cep { get; private set; }
    public String Nome { get; private set; }

    public Logradouro(int id, Cep cep, String nome) : base(id)
    {
        Cep = cep;
        Nome = nome;
    }
}
