using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.NormativeModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class NormativeService : INormativeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public NormativeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Normative
        public async Task<Result<NormativeResponse>> GetNormativeAsync(long id)
        {
            var normative = await _context.Normatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<NormativeResponse>.Failure("Норматив не найден", 404);

            return Result<NormativeResponse>.Success(_mapper.Map<NormativeResponse>(normative));
        }

        public async Task<Result<List<NormativeResponse>>> GetNormativesAsync(Filter? filter)
        {
            var normatives = await _context.Normatives
                .ApplyFilter(filter)
                .ToListAsync();
            return Result<List<NormativeResponse>>.Success(_mapper.Map<List<NormativeResponse>>(normatives));
        }

        public async Task<Result<NormativeResponse>> CreateNormativeAsync(NormativeCreateRequest req)
        {
            var normative = _mapper.Map<Normative>(req);
            normative.CreatedAt = DateTime.UtcNow;
            await _context.Normatives.AddAsync(normative);
            await _context.SaveChangesAsync();

            return Result<NormativeResponse>.Success(_mapper.Map<NormativeResponse>(normative));
        }

        public async Task<Result<NormativeResponse>> UpdateNormativeAsync(NormativeUpdateRequest req, long id)
        {
            var normative = await _context.Normatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<NormativeResponse>.Failure("Норматив не найден", 404);

            var newNormative = _mapper.Map(req, normative);
            _context.Normatives.Update(newNormative);
            await _context.SaveChangesAsync();

            return Result<NormativeResponse>.Success(_mapper.Map<NormativeResponse>(newNormative));
        }

        public async Task<Result<bool>> DeleteNormativeAsync(long id)
        {
            var normative = await _context.Normatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<bool>.Failure("Норматив не найден", 404);

            _context.NormativeSportsmen.RemoveRange(_context.NormativeSportsmen.Where(x => x.NormativeId == id));
            _context.Normatives.Remove(normative);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        // LocalNormative
        
        public async Task<Result<LocalNormativeResponse>> GetLocalNormativeAsync(long id)
        {
            var normative = await _context.LocalNormatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<LocalNormativeResponse>.Failure("Локальный норматив не найден", 404);

            return Result<LocalNormativeResponse>.Success(_mapper.Map<LocalNormativeResponse>(normative));
        }

        public async Task<Result<List<LocalNormativeResponse>>> GetLocalNormativesAsync(Filter? filter)
        {
            var normatives = await _context.LocalNormatives
                .ApplyFilter(filter)
                .ToListAsync();
            return Result<List<LocalNormativeResponse>>.Success(_mapper.Map<List<LocalNormativeResponse>>(normatives));
        }

        public async Task<Result<LocalNormativeResponse>> CreateLocalNormativeAsync(LocalNormativeCreateRequest req)
        {
            var normative = _mapper.Map<LocalNormative>(req);
            normative.CreatedAt = DateTime.UtcNow;
            await _context.LocalNormatives.AddAsync(normative);
            await _context.SaveChangesAsync();

            return Result<LocalNormativeResponse>.Success(_mapper.Map<LocalNormativeResponse>(normative));
        }

        public async Task<Result<LocalNormativeResponse>> UpdateLocalNormativeAsync(LocalNormativeUpdateRequest req, long id)
        {
            var normative = await _context.LocalNormatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<LocalNormativeResponse>.Failure("Локальный норматив не найден", 404);

            var newNormative = _mapper.Map(req, normative);
            _context.LocalNormatives.Update(newNormative);
            await _context.SaveChangesAsync();

            return Result<LocalNormativeResponse>.Success(_mapper.Map<LocalNormativeResponse>(newNormative));
        }

        public async Task<Result<bool>> DeleteLocalNormativeAsync(long id)
        {
            var normative = await _context.LocalNormatives.FirstOrDefaultAsync(x => x.Id == id);
            if (normative == null)
                return Result<bool>.Failure("Локальный норматив не найден", 404);

            _context.LocalNormativeSportsmen.RemoveRange(_context.LocalNormativeSportsmen.Where(x => x.LocalNormativeId == id));
            _context.LocalNormatives.Remove(normative);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        //ResultsNormative

        public async Task<Result<NormativeSportsmanResponse>> AddNormativeResultAsync(NormativeSportsmanCreateRequest req)
        {
            var entry = _mapper.Map<NormativeSportsman>(req);
            entry.CreatedAt = DateTime.UtcNow;
            await _context.NormativeSportsmen.AddAsync(entry);
            await _context.SaveChangesAsync();

            return Result<NormativeSportsmanResponse>.Success(_mapper.Map<NormativeSportsmanResponse>(entry));
        }

        public async Task<Result<List<NormativeSportsmanResponse>>> GetNormativeResultsAsync(long sportsmanId)
        {
            var results = await _context.NormativeSportsmen
                .Where(x => x.SportsmanId == sportsmanId)
                .ToListAsync();
            return Result<List<NormativeSportsmanResponse>>.Success(_mapper.Map<List<NormativeSportsmanResponse>>(results));
        }

        public async Task<Result<LocalNormativeSportsmanResponse>> AddLocalNormativeResultAsync(LocalNormativeSportsmanCreateRequest req)
        {
            var entry = _mapper.Map<LocalNormativeSportsman>(req);
            entry.CreatedAt = DateTime.UtcNow;
            await _context.LocalNormativeSportsmen.AddAsync(entry);
            await _context.SaveChangesAsync();

            return Result<LocalNormativeSportsmanResponse>.Success(_mapper.Map<LocalNormativeSportsmanResponse>(entry));
        }

        public async Task<Result<List<LocalNormativeSportsmanResponse>>> GetLocalNormativeResultsAsync(long sportsmanId)
        {
            var results = await _context.LocalNormativeSportsmen
                .Where(x => x.SportsmanId == sportsmanId)
                .ToListAsync();
            return Result<List<LocalNormativeSportsmanResponse>>.Success(_mapper.Map<List<LocalNormativeSportsmanResponse>>(results));
        }
    }
}
