using System; //Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

internal class AcessoColaborador : Entity
{
    Colaborador Colaborador { get; set; }
    DateOnly Chegada { get; set; }
    DateOnly Saida { get; set; }

    public AcessoColaborador(int id, Colaborador colaborador, DateOnly chegada, DateOnly saida) : base(id)
    {
        Colaborador = colaborador;
        Chegada = chegada;
        Saida = saida;
    }
}
