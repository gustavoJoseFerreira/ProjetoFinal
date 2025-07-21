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
    public class NotificacaoController : ControllerBase
    {
        private readonly BankContext _context;

        public NotificacaoController(BankContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificacaoDto>>> GetNotificacoes()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UtilizadorID == utilizadorId)
                .OrderByDescending(n => n.DataEnvio)
                .Select(n => new NotificacaoDto
                {
                    NotificacaoID = n.NotificacaoID,
                    Titulo = n.Titulo,
                    Mensagem = n.Mensagem,
                    DataEnvio = n.DataEnvio,
                    Lida = n.Lida
                })
                .ToListAsync();

            return Ok(notificacoes);
        }

        [HttpGet("naolidas")]
        public async Task<ActionResult<IEnumerable<NotificacaoDto>>> GetNotificacoesNaoLidas()
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notificacoes = await _context.Notificacoes
                .Where(n => n.UtilizadorID == utilizadorId && !n.Lida)
                .OrderByDescending(n => n.DataEnvio)
                .Select(n => new NotificacaoDto
                {
                    NotificacaoID = n.NotificacaoID,
                    Titulo = n.Titulo,
                    Mensagem = n.Mensagem,
                    DataEnvio = n.DataEnvio,
                    Lida = n.Lida
                })
                .ToListAsync();

            return Ok(notificacoes);
        }

        [HttpPost("{notificacaoId}/marcarcomolida")]
        public async Task<ActionResult> MarcarComoLida(int notificacaoId)
        {
            var utilizadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notificacao = await _context.Notificacoes
                .FirstOrDefaultAsync(n => n.NotificacaoID == notificacaoId && n.UtilizadorID == utilizadorId);

            if (notificacao == null)
                return NotFound("Notificação não encontrada.");

            notificacao.Lida = true;
            await _context.SaveChangesAsync();

            return Ok("Notificação marcada como lida.");
        }
    }
}