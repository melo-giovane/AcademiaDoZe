using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    internal class Aluno : Pessoa
    {
        public Aluno(string nome, Cpf cpf, DateOnly dataNascimento, Telefone telefone, Email email, Senha senha, Arquivo foto, Endereco endereco) : base(nome, cpf, dataNascimento, telefone, email, senha, foto, endereco)
        {

        }

    }
}
