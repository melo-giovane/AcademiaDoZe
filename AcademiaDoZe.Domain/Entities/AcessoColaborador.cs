using System; //Giovane Melo
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities;

public class AcessoColaborador : Entity
{
    public Colaborador Colaborador { get; private set; }
    public DateOnly DataHora { get; private set; }

    private AcessoColaborador(int id, Colaborador colaborador, DateOnly dataHora) : base(id)
    {
        Colaborador = colaborador;
        DataHora = dataHora;
    }
}
