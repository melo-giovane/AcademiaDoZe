using AcademiaDoZe.Domain.ValueObjects;//Giovane Melo   
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

public class Aluno : Pessoa
{
    private Aluno(string nome,
                int id,
                Cpf cpf,
                DateOnly dataNascimento,
                Telefone telefone,
                Email email,
                Endereco endereco,
                Senha senha,
                Arquivo foto)
           : base(id, nome, cpf, dataNascimento, telefone, email, endereco, senha, foto)
    {
    }

}
