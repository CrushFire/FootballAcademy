using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.MetricModel;
using Core.Results;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MetricService : IMetricsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ParserMetrics _parser;
        private readonly IRagService _rag;

        public MetricService(ApplicationDbContext context, IMapper mapper, ParserMetrics parser, IRagService rag)
        {
            _context = context;
            _mapper = mapper;
            _parser = parser;
            _rag = rag;
        }

        public async Task<Result<MetricResponse>> GetMetricAsync(long metricId)
        {
            var metric = await _context.TrainingMetrics.FirstOrDefaultAsync(x => x.Id == metricId);
            if (metric == null)
            {
                return Result<MetricResponse>.Failure("Метрика не найдена", 404);
            }

            var metricRes = _mapper.Map<MetricResponse>(metric);
            return Result<MetricResponse>.Success(metricRes);
        }

        public async Task<Result<List<MetricResponse>>> GetMetricsAsync(Filter? filter)
        {
            var query = _context.TrainingMetrics.AsQueryable();

            var sportsmanIds = filter?.Filters?.SportsmanId;
            if (sportsmanIds != null && sportsmanIds.Any())
                query = query.Where(m => sportsmanIds.Contains(m.SportsmanId));

            var dateRange = filter?.Filters?.Date;
            if (dateRange?.From.HasValue == true)
            {
                var from = DateTime.SpecifyKind(dateRange.From.Value, DateTimeKind.Unspecified);
                query = query.Where(m => m.CreatedAt >= from);
            }
            if (dateRange?.To.HasValue == true)
            {
                var to = DateTime.SpecifyKind(dateRange.To.Value, DateTimeKind.Unspecified);
                query = query.Where(m => m.CreatedAt <= to);
            }

            var pagination = filter?.Pagination;
            if (pagination != null)
                query = query.OrderByDescending(m => m.CreatedAt)
                             .Skip((pagination.Page - 1) * pagination.PageSize)
                             .Take(pagination.PageSize);

            var metrics = await query.ToListAsync();
            var metricsRes = _mapper.Map<List<MetricResponse>>(metrics);
            return Result<List<MetricResponse>>.Success(metricsRes);
        }

        public async Task<Result<MetricResponse>> CreateMetricAsync(MetricCreateRequest req)
        {
            var newMetric = _mapper.Map<TrainingMetrics>(req);
            await _context.TrainingMetrics.AddAsync(newMetric);
            await _context.SaveChangesAsync();

            _ = _rag.UpsertSportsmanEmbeddingAsync(newMetric.SportsmanId);

            var metricRes = _mapper.Map<MetricResponse>(newMetric);
            return Result<MetricResponse>.Success(metricRes);
        }

        public async Task<Result<MetricResponse>> UpdateMetricAsync(MetricUpdateRequest req, long metricId)
        {
            var metricExist = await _context.TrainingMetrics.FirstOrDefaultAsync(x => x.Id == metricId);
            if (metricExist == null)
            {
                return Result<MetricResponse>.Failure("Метрика не найдена", 404);
            }

            var newMetric = _mapper.Map(req, metricExist);
            _context.TrainingMetrics.Update(newMetric);
            await _context.SaveChangesAsync();

            _ = _rag.UpsertSportsmanEmbeddingAsync(newMetric.SportsmanId);

            var metricRes = _mapper.Map<MetricResponse>(newMetric);
            return Result<MetricResponse>.Success(metricRes);
        }

        public async Task<Result<bool>> DeleteMetricAsync(long metricId)
        {
            var metricExist = await _context.TrainingMetrics.FirstOrDefaultAsync(x => x.Id == metricId);
            if (metricExist == null)
            {
                return Result<bool>.Failure("Метрика не найдена", 404);
            }

            _context.TrainingMetrics.Remove(metricExist);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }



        public async Task<Result<MetricResponse>> GetMetricByTrainingSportsmanAsync(long trainingId, long sportsmanId)
        {
            var metric = await _context.TrainingMetrics
                .FirstOrDefaultAsync(m => m.TrainingId == trainingId && m.SportsmanId == sportsmanId);

            if (metric == null)
            {
                return Result<MetricResponse>.Failure("Метрика не найдена", 404);
            }

            var metricRes = _mapper.Map<MetricResponse>(metric);
            return Result<MetricResponse>.Success(metricRes);
        }

        public async Task<Result<string>> ImportMetricsFromFilesAsync(List<IFormFile> files)
        {
            var results = new List<string>();

            foreach (var file in files)
            {
                var parseResult = await _parser.ParseExcelFileAsync(file);
                if (!parseResult.IsSuccess)
                {
                    results.Add($"{file.FileName}: {parseResult.ErrorMessage}");
                    continue;
                }

                var metrics = parseResult.Data.Select(req => _mapper.Map<TrainingMetrics>(req)).ToList();
                await _context.TrainingMetrics.AddRangeAsync(metrics);
                await _context.SaveChangesAsync();
                results.Add($"{file.FileName}: Успешно импортировано {parseResult.Data.Count} метрик");

                var sportsmanIds = metrics.Select(m => m.SportsmanId).Distinct();
                foreach (var id in sportsmanIds)
                    _ = _rag.UpsertSportsmanEmbeddingAsync(id);
            }

            return Result<string>.Success(string.Join("\n", results));
        }

        public async Task<Result<string>> ImportMetricsFromFileAsync(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var formFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath));

            var parseResult = await _parser.ParseExcelFileAsync(formFile);
            if (!parseResult.IsSuccess)
            {
                return Result<string>.Failure(parseResult.ErrorMessage, parseResult.StatusCode);
            }

            var metrics = parseResult.Data.Select(req => _mapper.Map<TrainingMetrics>(req)).ToList();
            await _context.TrainingMetrics.AddRangeAsync(metrics);
            await _context.SaveChangesAsync();
            return Result<string>.Success($"Импортировано {parseResult.Data.Count} метрик");
        }
    }
}
