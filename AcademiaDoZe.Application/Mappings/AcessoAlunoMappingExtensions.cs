using AcademiaDoZe.Application.DTOs;//Giovane Melo
using AcademiaDoZe.Domain.Entities;
namespace AcademiaDoZe.Application.Mappings;

public static class AcessoAlunoMappingExtensions
{
    public static AcessoAlunoDto ToDto(this AcessoAluno acesso, AlunoDto alunoDto)
    {
        ArgumentNullException.ThrowIfNull(acesso);
        ArgumentNullException.ThrowIfNull(alunoDto);
        return new AcessoAlunoDto
        {
            Id = acesso.Id,
            AlunoAcesso = alunoDto,
            DataHora = acesso.DataHora
        };
    }
    public static AcessoAluno ToEntity(this AcessoAlunoDto acessoDto, Aluno aluno)
    {
        ArgumentNullException.ThrowIfNull(acessoDto);
        ArgumentNullException.ThrowIfNull(aluno);
        var result = AcessoAluno.Criar(acessoDto.Id, aluno, acessoDto.DataHora);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Erro de validação ao converter AcessoAluno: {string.Join(", ", result.Notifications.Select(n => n.Mensagem))}");
        }
        return result.Value!;
    }
}
