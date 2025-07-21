using BankSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankSystemAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PagamentoController : ControllerBase
    {
        private readonly BankContext _context;

        public PagamentoController(BankContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<PagamentoResponseDto>> RealizarPagamento(PagamentoDto pagamentoDto)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validar conta
            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == pagamentoDto.IBAN && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return BadRequest("Conta não encontrada ou não pertence ao utilizador.");

            if (conta.Estado != "Ativa")
                return BadRequest("Conta não está ativa.");

            if (conta.Saldo < pagamentoDto.Valor)
                return BadRequest("Saldo insuficiente para realizar o pagamento.");

            // Realizar pagamento
            conta.Saldo -= pagamentoDto.Valor;

            // Registrar movimento
            var movimento = new Movimento
            {
                ContaID = conta.ContaID,
                TipoMovimento = "Debito",
                Valor = pagamentoDto.Valor,
                Descricao = $"Pagamento a {pagamentoDto.Entidade} - Ref: {pagamentoDto.Referencia}"
            };
            _context.Movimentos.Add(movimento);

            // Registrar pagamento
            var pagamento = new Pagamento
            {
                ContaID = conta.ContaID,
                Entidade = pagamentoDto.Entidade,
                Referencia = pagamentoDto.Referencia,
                Valor = pagamentoDto.Valor
            };
            _context.Pagamentos.Add(pagamento);

            // Criar notificação
            var notificacao = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Pagamento Realizado",
                Mensagem = $"Pagamento de {pagamentoDto.Valor:C} a {pagamentoDto.Entidade} (Ref: {pagamentoDto.Referencia})"
            };
            _context.Notificacoes.Add(notificacao);

            await _context.SaveChangesAsync();

            return Ok(new PagamentoResponseDto
            {
                PagamentoID = pagamento.PagamentoID,
                IBAN = pagamentoDto.IBAN,
                Entidade = pagamentoDto.Entidade,
                Referencia = pagamentoDto.Referencia,
                Valor = pagamentoDto.Valor,
                DataPagamento = pagamento.DataPagamento
            });
        }

        [HttpGet("meuspagamentos")]
        public async Task<ActionResult<IEnumerable<PagamentoResponseDto>>> GetPagamentosUtilizador()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var pagamentos = await _context.Pagamentos
                .Include(p => p.Conta)
                .Where(p => p.Conta.UtilizadorID == utilizadorId)
                .OrderByDescending(p => p.DataPagamento)
                .Select(p => new PagamentoResponseDto
                {
                    PagamentoID = p.PagamentoID,
                    IBAN = p.Conta.IBAN,
                    Entidade = p.Entidade,
                    Referencia = p.Referencia,
                    Valor = p.Valor,
                    DataPagamento = p.DataPagamento
                })
                .ToListAsync();

            return Ok(pagamentos);
        }
    }
}