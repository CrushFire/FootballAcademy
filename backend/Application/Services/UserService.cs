using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.User;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPersonalService _personalService;
        private readonly ISportsmanService _sportsmanService;
        private readonly PasswordHasher _passwordHasher;

        public UserService(ApplicationDbContext context, IMapper mapper, IPersonalService personalService, ISportsmanService sportsmanService, PasswordHasher passwordHasher)
        {
            _context = context;
            _mapper = mapper;
            _personalService = personalService;
            _sportsmanService = sportsmanService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<bool>> CreateUserAsync(UserCreateRequest req)
        {
            if (req == null)
                return Result<bool>.Failure("Данные пользователя не могут быть пустыми", 400);

            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == req.Email);
            if (existingUser != null)
                return Result<bool>.Failure("Пользователь с таким email уже существует", 409);

            var newUser = _mapper.Map<User>(req);
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                newUser.Password = _passwordHasher.HashPassword(req.Password);
            }
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        public async Task<Result<UserResponse>> UpdateUserAsync(UserUpdateRequest req, long Id)
        {
            if (req == null)
                return Result<UserResponse>.Failure("Данные пользователя не могут быть пустыми", 400);

            var userExist = await _context.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (userExist == null)
                return Result<UserResponse>.Failure("Пользователь не найден", 404);

            var emailExist = await _context.Users.AnyAsync(x => x.Email == req.Email && x.Id != Id);
            if (emailExist)
            {
                return Result<UserResponse>.Failure("Пользователь с таким email уже существует", 409);
            }

            userExist.Login = req.Login;
            userExist.Email = req.Email;
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                userExist.Password = _passwordHasher.HashPassword(req.Password);
            }
            _context.Users.Update(userExist);
            await _context.SaveChangesAsync();

            var userRes = _mapper.Map<UserResponse>(userExist);
            return Result<UserResponse>.Success(userRes);
        }

        public async Task<Result<UserResponse>> GetUserAsync(long id)
        {
            if (id <= 0)
            {
                return Result<UserResponse>.Failure("Некорректный ID пользователя", 400);
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return Result<UserResponse>.Failure("Данный пользователь не найден", 404);
            }

            var userRes = _mapper.Map<UserResponse>(user);
            return Result<UserResponse>.Success(userRes);
        }

        public async Task<Result<List<UserResponse>>> GetUsersAsync(Filter? filter)
        {
            var users = await _context.Users
                .ApplyFilter(filter)
                .ToListAsync();

            var usersRes = _mapper.Map<List<UserResponse>>(users);
            return Result<List<UserResponse>>.Success(usersRes);
        }

        public async Task<Result<List<UserAdminResponse>>> GetUsersAdminAsync(Filter? filter)
        {
            var users = await _context.Users
                .ApplyFilter(filter)
                .ToListAsync();

            var usersRes = _mapper.Map<List<UserAdminResponse>>(users);
            return Result<List<UserAdminResponse>>.Success(usersRes);
        }

        public async Task<Result<bool>> DeleteUserAsync(long id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return Result<bool>.Failure("Данного пользователя не существует", 404);

            // Удалить связанный Personal или Sportsman через соответствующие сервисы
            var personal = await _context.Personal.FirstOrDefaultAsync(x => x.UserId == id);
            if (personal != null)
            {
                var res = await _personalService.DeletePersonalAsync(personal.Id);
                if (!res.IsSuccess)
                    return Result<bool>.Failure(res.ErrorMessage!, res.StatusCode);
            }

            var sportsman = await _context.Sportsmen.FirstOrDefaultAsync(x => x.UserId == id);
            if (sportsman != null)
            {
                var res = await _sportsmanService.DeleteSportsmanAsync(sportsman.Id);
                if (!res.IsSuccess)
                    return Result<bool>.Failure(res.ErrorMessage!, res.StatusCode);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
    }
}
