using AcademiaDoZe.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

private class Aluno : Pessoa
{
    private Aluno(string nome, 
                Cpf cpf, 
                DateOnly dataNascimento, 
                Telefone telefone, 
                Email email, 
                Senha senha, 
                Arquivo foto, 
                Endereco endereco) 
    : base(nome, cpf, dataNascimento, telefone, email, senha, foto, endereco)
    {
    }

}
