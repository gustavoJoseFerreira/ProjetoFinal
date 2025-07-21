using BankSystemAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BankSystemAPI.Controllers
{
    [Authorize] // Exige autenticação (qualquer perfil)
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly BankContext _context;

        public UserController(BankContext context)
        {
            _context = context;
        }

        // Obter perfil do próprio usuário
        [HttpGet("me")]
        public async Task<ActionResult<UtilizadorProfileDto>> GetMyProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = await _context.Utilizadores
                .Where(u => u.UtilizadorID == userId)
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
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Ok(user);
        }

        // Atualizar dados pessoais (exceto senha/perfil)
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto profileDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Utilizadores.FindAsync(userId);

            if (user == null) return NotFound();

            // Atualiza apenas campos permitidos
            user.Telemovel = profileDto.Telemovel ?? user.Telemovel;
            user.Profissao = profileDto.Profissao ?? user.Profissao;
            user.Morada = profileDto.Morada ?? user.Morada;
            user.DataNascimento = profileDto.DataNascimento ?? user.DataNascimento;

            await _context.SaveChangesAsync();

            return Ok("Perfil atualizado com sucesso.");
        }

        //Listar minhas notificações
        [HttpGet("notificacoes")]
        public async Task<ActionResult<IEnumerable<NotificacaoDto>>> GetMyNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var notifications = await _context.Notificacoes
                .Where(n => n.UtilizadorID == userId)
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

            return Ok(notifications);
        }

        // Marcar notificação como lida
        [HttpPost("notificacoes/{id}/ler")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var notification = await _context.Notificacoes
                .FirstOrDefaultAsync(n => n.NotificacaoID == id && n.UtilizadorID == userId);

            if (notification == null) return NotFound();

            notification.Lida = true;
            await _context.SaveChangesAsync();

            return Ok("Notificação marcada como lida.");
        }
    }

    // DTOs (Data Transfer Objects)
    public class UpdateProfileDto
    {
        public string? Telemovel { get; set; }
        public string? Profissao { get; set; }
        public string? Morada { get; set; }
        public DateTime? DataNascimento { get; set; }
    }
}       
