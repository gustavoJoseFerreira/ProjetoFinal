using BankSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankSystemAPI.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly BankContext _context;

        public AdminController(BankContext context)
        {
            _context = context;
        }

        [HttpGet("utilizadores")]
        public async Task<ActionResult<IEnumerable<UtilizadorProfileDto>>> GetAllUtilizadores()
        {
            var utilizadores = await _context.Utilizadores
                .Select(u => new UtilizadorProfileDto
                {
                    Nome = u.Nome,
                    Email = u.Email,
                    Telemovel = u.Telemovel,
                    Perfil = u.Perfil,
                    NIF = u.NIF,
                    CartaoCidadao = u.CartaoCidadao,
                    DataNascimento = u.DataNascimento,
                    Profissao = u.Profissao,
                    Morada = u.Morada,
                    DataCriacao = u.DataCriacao
                })
                .ToListAsync();

            return Ok(utilizadores);
        }

        [HttpPost("utilizadores/{utilizadorId}/alterarperfil")]
        public async Task<ActionResult> AlterarPerfilUtilizador(int utilizadorId, [FromBody] string novoPerfil)
        {
            if (novoPerfil != "Administrador" && novoPerfil != "Cliente")
                return BadRequest("Perfil inválido. Deve ser 'Administrador' ou 'Cliente'.");

            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            utilizador.Perfil = novoPerfil;
            await _context.SaveChangesAsync();

            // Criar notificação
            var notificacao = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Alteração de Perfil",
                Mensagem = $"O seu perfil foi alterado para {novoPerfil}"
            };
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return Ok($"Perfil do utilizador {utilizador.Nome} alterado para {novoPerfil}.");
        }

        [HttpPost("utilizadores/{utilizadorId}/alternarestado")]
        public async Task<ActionResult> AlternarEstadoUtilizador(int utilizadorId)
        {
            var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
            if (utilizador == null)
                return NotFound("Utilizador não encontrado.");

            utilizador.Ativo = !utilizador.Ativo;
            await _context.SaveChangesAsync();

            
            var estado = utilizador.Ativo ? "ativada" : "desativada";
            var notificacao = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Estado da Conta",
                Mensagem = $"A sua conta foi {estado} por um administrador"
            };
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return Ok($"Conta do utilizador {utilizador.Nome} foi {estado}.");
        }

        [HttpGet("contas")]
        public async Task<ActionResult> GetAllContas()
        {
            var contas = await _context.ContasBancarias
                .Include(c => c.Utilizador)
                .Select(c => new
                {
                    c.ContaID,
                    c.IBAN,
                    c.TipoConta,
                    c.Saldo,
                    c.Estado,
                    c.DataCriacao,
                    UtilizadorNome = c.Utilizador.Nome,
                    UtilizadorEmail = c.Utilizador.Email
                })
                .ToListAsync();

            var saldoTotal = contas.Sum(c => c.Saldo);
            var contasAtivas = contas.Count(c => c.Estado == "Ativa");
            var contasEncerradas = contas.Count(c => c.Estado == "Encerrada");

            return Ok(new
            {
                TotalContas = contas.Count,
                ContasAtivas = contasAtivas,
                ContasEncerradas = contasEncerradas,
                SaldoTotal = saldoTotal,
                Contas = contas
            });
        }

        [HttpGet("transferencias")]
        public async Task<ActionResult<IEnumerable<TransferenciaResponseDto>>> GetAllTransferencias()
        {
            var transferencias = await _context.Transferencias
                .Include(t => t.ContaOrigem)
                .ThenInclude(c => c.Utilizador)
                .Include(t => t.ContaDestino)
                .ThenInclude(c => c.Utilizador)
                .OrderByDescending(t => t.DataTransferencia)
                .Select(t => new
                {
                    t.TransferenciaID,
                    IBANOrigem = t.ContaOrigem.IBAN,
                    NomeOrigem = t.ContaOrigem.Utilizador.Nome,
                    IBANDestino = t.ContaDestino.IBAN,
                    NomeDestino = t.ContaDestino.Utilizador.Nome,
                    t.Valor,
                    t.DataTransferencia,
                    t.Descricao
                })
                .ToListAsync();

            return Ok(transferencias);
        }

        [HttpGet("pagamentos")]
        public async Task<ActionResult<IEnumerable<PagamentoResponseDto>>> GetAllPagamentos()
        {
            var pagamentos = await _context.Pagamentos
                .Include(p => p.Conta)
                .ThenInclude(c => c.Utilizador)
                .OrderByDescending(p => p.DataPagamento)
                .Select(p => new
                {
                    p.PagamentoID,
                    p.Conta.IBAN,
                    UtilizadorNome = p.Conta.Utilizador.Nome,
                    p.Entidade,
                    p.Referencia,
                    p.Valor,
                    p.DataPagamento
                })
                .ToListAsync();

            return Ok(pagamentos);
        }

        [HttpGet("movimentos")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetAllMovimentos(
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            dataInicio ??= DateTime.UtcNow.AddDays(-30);
            dataFim ??= DateTime.UtcNow;

            var movimentos = await _context.Movimentos
                .Include(m => m.Conta)
                .ThenInclude(c => c.Utilizador)
                .Where(m => m.DataMovimento >= dataInicio &&
                            m.DataMovimento <= dataFim)
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new
                {
                    m.MovimentoID,
                    m.TipoMovimento,
                    m.Valor,
                    m.DataMovimento,
                    m.Descricao,
                    IBAN = m.Conta.IBAN,
                    UtilizadorNome = m.Conta.Utilizador.Nome,
                    UtilizadorEmail = m.Conta.Utilizador.Email
                })
                .ToListAsync();

            return Ok(movimentos);
        }

        [HttpGet("movimentos/estatisticas")]
        public async Task<ActionResult> GetEstatisticasMovimentos(
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            dataInicio ??= DateTime.UtcNow.AddDays(-30);
            dataFim ??= DateTime.UtcNow;

            var estatisticas = await _context.Movimentos
                .Where(m => m.DataMovimento >= dataInicio &&
                            m.DataMovimento <= dataFim)
                .GroupBy(m => m.TipoMovimento)
                .Select(g => new
                {
                    TipoMovimento = g.Key,
                    Total = g.Sum(m => m.Valor),
                    Quantidade = g.Count()
                })
                .ToListAsync();

            var totalGeral = estatisticas.Sum(e => e.Total);
            var quantidadeTotal = estatisticas.Sum(e => e.Quantidade);

            return Ok(new
            {
                PeriodoInicio = dataInicio,
                PeriodoFim = dataFim,
                TotalGeral = totalGeral,
                QuantidadeTotal = quantidadeTotal,
                Detalhes = estatisticas
            });
        }

        [HttpGet("pesquisa")]
        public async Task<ActionResult> PesquisaDetalhada(
            [FromQuery] string? termo,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            var query = _context.Movimentos
                .Include(m => m.Conta)
                .ThenInclude(c => c.Utilizador)
                .AsQueryable();

            if (!string.IsNullOrEmpty(termo))
            {
                query = query.Where(m =>
                    m.Descricao.Contains(termo) ||
                    m.Conta.IBAN.Contains(termo) ||
                    m.Conta.Utilizador.Nome.Contains(termo) ||
                    m.Conta.Utilizador.Email.Contains(termo));
            }

            if (dataInicio.HasValue)
            {
                query = query.Where(m => m.DataMovimento >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(m => m.DataMovimento <= dataFim.Value);
            }

            var resultados = await query
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new
                {
                    m.MovimentoID,
                    m.TipoMovimento,
                    m.Valor,
                    m.DataMovimento,
                    m.Descricao,
                    IBAN = m.Conta.IBAN,
                    UtilizadorNome = m.Conta.Utilizador.Nome,
                    UtilizadorEmail = m.Conta.Utilizador.Email
                })
                .ToListAsync();

            return Ok(new
            {
                TermoPesquisa = termo,
                DataInicio = dataInicio,
                DataFim = dataFim,
                TotalResultados = resultados.Count,
                Resultados = resultados
            });
        }

        [HttpGet("logsacesso")]
        public async Task<ActionResult<IEnumerable<LogAcesso>>> GetLogsAcesso()
        {
            var logs = await _context.LogsAcesso
                .Include(l => l.Utilizador)
                .OrderByDescending(l => l.DataHora)
                .Select(l => new
                {
                    l.LogID,
                    UtilizadorNome = l.Utilizador != null ? l.Utilizador.Nome : "N/A",
                    l.DataHora,
                    l.IP,
                    l.Sucesso,
                    l.Descricao
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}