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
    public class TransferenciaController : ControllerBase
    {
        private readonly BankContext _context;

        public TransferenciaController(BankContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TransferenciaResponseDto>> RealizarTransferencia(TransferenciaDto transferenciaDto)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validar conta de origem
            var contaOrigem = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == transferenciaDto.IBANOrigem && c.UtilizadorID == utilizadorId);

            if (contaOrigem == null)
                return BadRequest("Conta de origem não encontrada ou não pertence ao utilizador.");

            if (contaOrigem.Estado != "Ativa")
                return BadRequest("Conta de origem não está ativa.");

            // Validar conta de destino
            var contaDestino = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == transferenciaDto.IBANDestino);

            if (contaDestino == null)
                return BadRequest("Conta de destino não encontrada.");

            if (contaDestino.Estado != "Ativa")
                return BadRequest("Conta de destino não está ativa.");

            if (contaOrigem.Saldo < transferenciaDto.Valor)
                return BadRequest("Saldo insuficiente para realizar a transferência.");

            // Realizar transferência
            contaOrigem.Saldo -= transferenciaDto.Valor;
            contaDestino.Saldo += transferenciaDto.Valor;

            // Registrar movimentos
            var movimentoDebito = new Movimento
            {
                ContaID = contaOrigem.ContaID,
                TipoMovimento = "Debito",
                Valor = transferenciaDto.Valor,
                Descricao = transferenciaDto.Descricao ?? $"Transferência para {transferenciaDto.IBANDestino}"
            };

            var movimentoCredito = new Movimento
            {
                ContaID = contaDestino.ContaID,
                TipoMovimento = "Credito",
                Valor = transferenciaDto.Valor,
                Descricao = transferenciaDto.Descricao ?? $"Transferência de {transferenciaDto.IBANOrigem}"
            };

            _context.Movimentos.Add(movimentoDebito);
            _context.Movimentos.Add(movimentoCredito);

            // Registrar transferência
            var transferencia = new Transferencia
            {
                ContaOrigemID = contaOrigem.ContaID,
                ContaDestinoID = contaDestino.ContaID,
                Valor = transferenciaDto.Valor,
                Descricao = transferenciaDto.Descricao
            };
            _context.Transferencias.Add(transferencia);

            // Criar notificações
            var notificacaoOrigem = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Transferência Realizada",
                Mensagem = $"Transferiu {transferenciaDto.Valor:C} para a conta {transferenciaDto.IBANDestino}"
            };

            var notificacaoDestino = new Notificacao
            {
                UtilizadorID = contaDestino.UtilizadorID,
                Titulo = "Transferência Recebida",
                Mensagem = $"Recebeu {transferenciaDto.Valor:C} da conta {transferenciaDto.IBANOrigem}"
            };

            _context.Notificacoes.Add(notificacaoOrigem);
            _context.Notificacoes.Add(notificacaoDestino);

            await _context.SaveChangesAsync();

            return Ok(new TransferenciaResponseDto
            {
                TransferenciaID = transferencia.TransferenciaID,
                IBANOrigem = transferenciaDto.IBANOrigem,
                IBANDestino = transferenciaDto.IBANDestino,
                Valor = transferenciaDto.Valor,
                DataTransferencia = transferencia.DataTransferencia,
                Descricao = transferenciaDto.Descricao
            });
        }

        [HttpGet("minhastransferencias")]
        public async Task<ActionResult<IEnumerable<TransferenciaResponseDto>>> GetTransferenciasUtilizador()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var transferencias = await _context.Transferencias
                .Include(t => t.ContaOrigem)
                .Include(t => t.ContaDestino)
                .Where(t => t.ContaOrigem.UtilizadorID == utilizadorId || t.ContaDestino.UtilizadorID == utilizadorId)
                .OrderByDescending(t => t.DataTransferencia)
                .Select(t => new TransferenciaResponseDto
                {
                    TransferenciaID = t.TransferenciaID,
                    IBANOrigem = t.ContaOrigem.IBAN,
                    IBANDestino = t.ContaDestino.IBAN,
                    Valor = t.Valor,
                    DataTransferencia = t.DataTransferencia,
                    Descricao = t.Descricao
                })
                .ToListAsync();

            return Ok(transferencias);
        }
    }
}