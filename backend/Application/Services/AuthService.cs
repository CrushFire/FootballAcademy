using Application.Utils;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Models.Auth;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenGen _jwtTokenGen;
        private readonly PasswordHasher _passwordHasher;

        public AuthService(ApplicationDbContext context, JwtTokenGen jwtTokenGen, PasswordHasher passwordHasher)
        {
            _context = context;
            _jwtTokenGen = jwtTokenGen;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<AuthResponse>> AuthorizationAsync(AuthRequest req)
        {
            // Универсальный вход: ищем по логину ИЛИ email (что прислали).
            // Identifier — приоритет; если его нет — fallback на Email для старых клиентов.
            var input = (req.Identifier ?? req.Email ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(input))
                return Result<AuthResponse>.Failure("Укажите логин или email", 400);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Login == input || u.Email == input);
            if (user == null)
                return Result<AuthResponse>.Failure("Пользователь не найден", 404);

            if (!_passwordHasher.VerifyPassword(req.Password, user.Password))
                return Result<AuthResponse>.Failure("Неверный пароль", 401);

            string? personalType = null;
            string? personalId = null;
            if (user.Role == "personal")
            {
                var personal = await _context.Personal.FirstOrDefaultAsync(p => p.UserId == user.Id);
                personalType = personal?.Type.ToString();
                personalId = personal?.Id.ToString();
            }

            var token = _jwtTokenGen.TokenGen(user.Id.ToString(), user.Login, user.Email, user.Role, personalType, personalId);
            return Result<AuthResponse>.Success(new AuthResponse { Token = token, UserId = user.Id });
        }

        public async Task<Result<AuthResponse>> RegisterAdminAsync(RegistrationRequest req)
        {
            if (await _context.Users.AnyAsync(u => u.Email == req.Email))
                return Result<AuthResponse>.Failure("Данная почта уже используется", 409);

            var user = new User
            {
                Login = req.Login,
                Email = req.Email,
                Password = _passwordHasher.HashPassword(req.Password),
                Role = "admin"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenGen.TokenGen(user.Id.ToString(), user.Login, user.Email, user.Role);
            return Result<AuthResponse>.Success(new AuthResponse { Token = token, UserId = user.Id });
        }

public async Task<Result<AuthResponse>> RegisterSportsmanAsync(RegistrationRequest req)
        {
            if (await _context.Users.AnyAsync(u => u.Email == req.Email))
                return Result<AuthResponse>.Failure("Данная почта уже используется", 409);

            var user = new User
            {
                Login = req.Login,
                Email = req.Email,
                Password = _passwordHasher.HashPassword(req.Password),
                Role = "sportsman"
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenGen.TokenGen(user.Id.ToString(), user.Login, user.Email, user.Role);
            return Result<AuthResponse>.Success(new AuthResponse { Token = token, UserId = user.Id });
        }
        public async Task<Result<AuthResponse>> RegisterTrainerAsync(PersonalRegistrationRequest req)
        {
            return await RegisterPersonalWithTypeAsync(req, PersonalType.Trainer);
        }

        public async Task<Result<AuthResponse>> RegisterMedicalAsync(PersonalRegistrationRequest req)
        {
            return await RegisterPersonalWithTypeAsync(req, PersonalType.Medical);
        }

        private async Task<Result<AuthResponse>> RegisterPersonalWithTypeAsync(PersonalRegistrationRequest req, PersonalType type)
        {
            if (await _context.Users.AnyAsync(u => u.Email == req.Email))
                return Result<AuthResponse>.Failure("Данная почта уже используется", 409);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Login = req.Login,
                    Email = req.Email,
                    Password = _passwordHasher.HashPassword(req.Password),
                    Role = "personal"
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var personal = new Personal
                {
                    UserId = user.Id,
                    FIO = req.FIO,
                    Position = req.Position ?? string.Empty,
                    Description = req.Description ?? string.Empty,
                    Type = type
                };
                await _context.Personal.AddAsync(personal);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var token = _jwtTokenGen.TokenGen(user.Id.ToString(), user.Login, user.Email, user.Role, type.ToString(), personal.Id.ToString());
                return Result<AuthResponse>.Success(new AuthResponse { Token = token, UserId = user.Id });
            }
            catch
            {
                await transaction.RollbackAsync();
                return Result<AuthResponse>.Failure("Ошибка при регистрации", 500);
            }
        }
    }
}
