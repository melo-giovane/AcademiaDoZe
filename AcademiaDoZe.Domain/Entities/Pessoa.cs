using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public abstract class Pessoa : Entity
    {
        public string Nome { get; set; }
        public Cpf Cpf { get; }
        public DateOnly DataNascimento { get; set; }
        public Telefone Telefone { get; set; }
        public Email Email { get; set; }
        public Senha Senha { get; set; }
        public Arquivo Foto { get; set; }
        public Endereco Endereco { get; set; }

        public Pessoa(string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email, Senha senha, Arquivo foto, Endereco endereco) : base(id)
        {
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
            Telefone = telefone;
            Email = email;
            Senha = senha;
            Foto = foto;
            Endereco = endereco;
        }
    }

}
