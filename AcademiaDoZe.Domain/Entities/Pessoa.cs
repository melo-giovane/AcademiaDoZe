using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public abstract class Pessoa : Entity
    {
        public string Nome { get; protected set; }
        public string Cpf { get; protected set; }
        protected Pessoa(int id, string nome, string cpf) : base(id)
        {
            Nome = nome;
            Cpf = cpf;
        }
    }

}
