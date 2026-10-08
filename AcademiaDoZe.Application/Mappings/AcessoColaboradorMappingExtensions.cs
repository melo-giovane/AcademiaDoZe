using AcademiaDoZe.Application.DTOs;//Giovane Melo
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Application.Mappings;

public static class AcessoColaboradorMappingExtensions
{
    public static AcessoColaboradorDto ToDto(this AcessoColaborador acesso, ColaboradorDto colaboradorDto)
    {
        ArgumentNullException.ThrowIfNull(acesso);
        ArgumentNullException.ThrowIfNull(colaboradorDto);
        return new AcessoColaboradorDto
        {
            Id = acesso.Id,
            ColaboradorAcesso = colaboradorDto,
            DataHora = acesso.DataHora
        };
    }
    public static AcessoColaborador ToEntity(this AcessoColaboradorDto acessoDto, Colaborador colaborador)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        ArgumentNullException.ThrowIfNull(colaborador);
        var result = AcessoColaborador.Criar(acessoDto.Id, colaborador, acessoDto.DataHora);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter AcessoColaborador: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
}
