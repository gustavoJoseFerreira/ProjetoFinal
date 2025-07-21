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
    public class ContaBancariaController : ControllerBase
    {
        private readonly BankContext _context;

        public ContaBancariaController(BankContext context)
        {
            _context = context;
        }

        [HttpGet("minhascontas")]
        public async Task<ActionResult<IEnumerable<ContaBancariaDto>>> GetContasUtilizador()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var contas = await _context.ContasBancarias
                .Where(c => c.UtilizadorID == utilizadorId && c.Estado == "Ativa")
                .Select(c => new ContaBancariaDto
                {
                    ContaID = c.ContaID,
                    IBAN = c.IBAN,
                    TipoConta = c.TipoConta,
                    Saldo = c.Saldo,
                    Estado = c.Estado,
                    DataCriacao = c.DataCriacao
                })
                .ToListAsync();

            return Ok(contas);
        }

        [HttpPost("criar")]
        public async Task<ActionResult<ContaBancariaDto>> CriarConta(ContaBancariaCreateDto contaDto)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Gerar IBAN 
            var iban = GerarIBAN();

            var conta = new ContaBancaria
            {
                UtilizadorID = utilizadorId,
                IBAN = iban,
                TipoConta = contaDto.TipoConta,
                Saldo = 0,
                Estado = "Ativa"
            };

            _context.ContasBancarias.Add(conta);
            await _context.SaveChangesAsync();

            // Criar notificação
            var notificacao = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Nova Conta Bancária",
                Mensagem = $"Foi criada uma nova conta {contaDto.TipoConta} com IBAN {iban}"
            };
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return Ok(new ContaBancariaDto
            {
                ContaID = conta.ContaID,
                IBAN = conta.IBAN,
                TipoConta = conta.TipoConta,
                Saldo = conta.Saldo,
                Estado = conta.Estado,
                DataCriacao = conta.DataCriacao
            });
        }

        [HttpGet("{iban}/saldo")]
        public async Task<ActionResult<decimal>> GetSaldoConta(string iban)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada.");

            return Ok(conta.Saldo);
        }

        [HttpGet("{iban}/movimentos")]
        public async Task<ActionResult<IEnumerable<MovimentoDto>>> GetMovimentosConta(string iban)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada.");

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

        [HttpPut("{iban}/encerrar")]
        public async Task<ActionResult> EncerrarConta(string iban)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var conta = await _context.ContasBancarias
                .FirstOrDefaultAsync(c => c.IBAN == iban && c.UtilizadorID == utilizadorId);

            if (conta == null)
                return NotFound("Conta não encontrada.");

            if (conta.Saldo != 0)
                return BadRequest("O saldo da conta deve ser zero para encerrar.");

            conta.Estado = "Encerrada";
            conta.DataEncerramento = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Criar notificação
            var notificacao = new Notificacao
            {
                UtilizadorID = utilizadorId,
                Titulo = "Conta Encerrada",
                Mensagem = $"A conta com IBAN {iban} foi encerrada com sucesso."
            };
            _context.Notificacoes.Add(notificacao);
            await _context.SaveChangesAsync();

            return Ok("Conta encerrada com sucesso.");
        }

        private string GerarIBAN()
        {
            var random = new Random();
            return $"PT50{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(1000, 9999)}{random.Next(100, 999)}";
        }
    }
}