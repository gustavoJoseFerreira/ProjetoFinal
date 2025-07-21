using BankSystemAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity; 

namespace BankSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BankContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<Utilizador> _hasher; 

        public AuthController(BankContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _hasher = new PasswordHasher<Utilizador>(); 
        }

        [HttpPost("register")]
        public async Task<ActionResult<Utilizador>> Register(UtilizadorRegisterDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Utilizadores.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email já está em uso.");

            if (await _context.Utilizadores.AnyAsync(u => u.NIF == request.NIF))
                return BadRequest("NIF já está registado.");

            if (await _context.Utilizadores.AnyAsync(u => u.CartaoCidadao == request.CartaoCidadao))
                return BadRequest("Cartão de Cidadão já está registado.");

            var utilizador = new Utilizador
            {
                Nome = request.Nome,
                Email = request.Email,
                Telemovel = request.Telemovel,
                Perfil = "Cliente",
                Ativo = true,
                NIF = request.NIF,
                CartaoCidadao = request.CartaoCidadao,
                DataNascimento = request.DataNascimento,
                Profissao = request.Profissao,
                Morada = request.Morada
            };

            // gerar o hash com PasswordHasher
            utilizador.PasswordHash = _hasher.HashPassword(utilizador, request.Password);

            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();

            var logAcesso = new LogAcesso
            {
                UtilizadorID = utilizador.UtilizadorID,
                IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Sucesso = true,
                Descricao = "Registo de novo utilizador"
            };
            _context.LogsAcesso.Add(logAcesso);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Utilizador registado com sucesso." });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UtilizadorLoginDto request)
        {
            var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (utilizador == null)
            {
                var logAcesso = new LogAcesso
                {
                    IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Sucesso = false,
                    Descricao = "Tentativa de login com email não registado"
                };
                _context.LogsAcesso.Add(logAcesso);
                await _context.SaveChangesAsync();

                return BadRequest("Utilizador não encontrado.");
            }

            //verificar password com PasswordHasher
            var result = _hasher.VerifyHashedPassword(utilizador, utilizador.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                var logAcesso = new LogAcesso
                {
                    UtilizadorID = utilizador.UtilizadorID,
                    IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Sucesso = false,
                    Descricao = "Tentativa de login com password incorreta"
                };
                _context.LogsAcesso.Add(logAcesso);
                await _context.SaveChangesAsync();

                return BadRequest("Password incorreta.");
            }

            if (!utilizador.Ativo)
            {
                var logAcesso = new LogAcesso
                {
                    UtilizadorID = utilizador.UtilizadorID,
                    IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Sucesso = false,
                    Descricao = "Tentativa de login com conta inativa"
                };
                _context.LogsAcesso.Add(logAcesso);
                await _context.SaveChangesAsync();

                return BadRequest("Conta de utilizador inativa.");
            }

            var logSucesso = new LogAcesso
            {
                UtilizadorID = utilizador.UtilizadorID,
                IP = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Sucesso = true,
                Descricao = "Login bem-sucedido"
            };
            _context.LogsAcesso.Add(logSucesso);
            await _context.SaveChangesAsync();

            string token = CreateToken(utilizador);

            return Ok(new { Token = token, Perfil = utilizador.Perfil, Nome = utilizador.Nome });
        }

        private string CreateToken(Utilizador utilizador)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, utilizador.Email),
                new Claim(ClaimTypes.Role, utilizador.Perfil),
                new Claim(ClaimTypes.NameIdentifier, utilizador.UtilizadorID.ToString()),
                new Claim(ClaimTypes.Name, utilizador.Nome)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("JwtSettings:SecretKey").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JwtSettings:Issuer").Value,
                audience: _configuration.GetSection("JwtSettings:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
