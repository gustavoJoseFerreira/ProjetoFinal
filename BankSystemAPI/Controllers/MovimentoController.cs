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
    public class MovimentoController : ControllerBase
    {
        private readonly BankContext _context;

        public MovimentoController(BankContext context)
        {
            _context = context;
        }

        [HttpGet("conta/{iban}")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetMovimentosPorConta(string iban)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verificar se a conta pertence ao utilizador
            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada ou não pertence ao utilizador.");

            var movimentos = await _context.Movimentos
                .Where(m => m.ContaID == conta.ContaID)
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = iban
                })
                .ToListAsync();

            return Ok(movimentos);
        }

        [HttpGet("periodo")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetMovimentosPorPeriodo(
            [FromQuery] DateTime dataInicio,
            [FromQuery] DateTime dataFim)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Obter todas as contas do utilizador
            var contasIds = await _context.ContasBancarias
                .Where(c => c.UtilizadorID == utilizadorId)
                .Select(c => c.ContaID)
                .ToListAsync();

            var movimentos = await _context.Movimentos
                .Where(m => contasIds.Contains(m.ContaID) &&
                            m.DataMovimento >= dataInicio &&
                            m.DataMovimento <= dataFim)
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = _context.ContasBancarias
                        .Where(c => c.ContaID == m.ContaID)
                        .Select(c => c.IBAN)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(movimentos);
        }

        [HttpGet("tipo/{tipoMovimento}")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetMovimentosPorTipo(string tipoMovimento)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Validar tipo de movimento
            if (tipoMovimento != "Credito" && tipoMovimento != "Debito")
                return BadRequest("Tipo de movimento inválido. Use 'Credito' ou 'Debito'.");

            // Obter todas as contas do utilizador
            var contasIds = await _context.ContasBancarias
                .Where(c => c.UtilizadorID == utilizadorId)
                .Select(c => c.ContaID)
                .ToListAsync();

            var movimentos = await _context.Movimentos
                .Where(m => contasIds.Contains(m.ContaID) &&
                            m.TipoMovimento == tipoMovimento)
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = _context.ContasBancarias
                        .Where(c => c.ContaID == m.ContaID)
                        .Select(c => c.IBAN)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(movimentos);
        }

        [HttpGet("detalhe/{movimentoId}")]
        public async Task<ActionResult<MovimentoDto>> GetMovimentoDetalhado(int movimentoId)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var movimento = await _context.Movimentos
                .Include(m => m.Conta)
                .Where(m => m.MovimentoID == movimentoId &&
                            m.Conta.UtilizadorID == utilizadorId)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = m.Conta.IBAN
                })
                .FirstOrDefaultAsync();

            if (movimento == null)
                return NotFound("Movimento não encontrado ou não pertence ao utilizador.");

            return Ok(movimento);
        }

        [HttpGet("saldo/{iban}")]
        public async Task<ActionResult<decimal>> GetSaldoAtual(string iban)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada ou não pertence ao utilizador.");

            return Ok(conta.Saldo);
        }

        [HttpGet("extrato/{iban}")]
        public async Task<ActionResult<ExtratoDto>> GetExtratoBancario(
            string iban,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada ou não pertence ao utilizador.");

            // Definir datas padrão se não forem fornecidas
            dataInicio ??= DateTime.UtcNow.AddDays(-30);
            dataFim ??= DateTime.UtcNow;

            var movimentos = await _context.Movimentos
                .Where(m => m.ContaID == conta.ContaID &&
                            m.DataMovimento >= dataInicio &&
                            m.DataMovimento <= dataFim)
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = iban
                })
                .ToListAsync();

            // Calcular totais
            var totalCreditos = movimentos
                .Where(m => m.TipoMovimento == "Credito")
                .Sum(m => m.Valor);

            var totalDebitos = movimentos
                .Where(m => m.TipoMovimento == "Debito")
                .Sum(m => m.Valor);

            var saldoFinal = conta.Saldo;
            var saldoInicial = saldoFinal - totalCreditos + totalDebitos;

            var extrato = new ExtratoDto
            {
                IBAN = iban,
                DataInicio = dataInicio.Value,
                DataFim = dataFim.Value,
                SaldoInicial = saldoInicial,
                SaldoFinal = saldoFinal,
                TotalCreditos = totalCreditos,
                TotalDebitos = totalDebitos,
                Movimentos = movimentos
            };

            return Ok(extrato);
        }

        [HttpGet("meus")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetTodosMovimentosDoUtilizador()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contasIds = await _context.ContasBancarias
                .Where(c => c.UtilizadorID == utilizadorId)
                .Select(c => c.ContaID)
                .ToListAsync();

            var movimentos = await _context.Movimentos
                .Where(m => contasIds.Contains(m.ContaID))
                .OrderByDescending(m => m.DataMovimento)
                .Select(m => new MovimentoDto
                {
                    MovimentoID = m.MovimentoID,
                    TipoMovimento = m.TipoMovimento,
                    Valor = m.Valor,
                    DataMovimento = m.DataMovimento,
                    Descricao = m.Descricao,
                    IBAN = _context.ContasBancarias
                        .Where(c => c.ContaID == m.ContaID)
                        .Select(c => c.IBAN)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(movimentos);
        }
    }

    public class ExtratoDto
    {
        public string IBAN { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal SaldoFinal { get; set; }
        public decimal TotalCreditos { get; set; }
        public decimal TotalDebitos { get; set; }
        public List<MovimentoDto> Movimentos { get; set; }
    }
}