using Core.Models;
using Core.Models.TeamModel;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Utils
{
    public static class FilterService
    {
        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, Core.Models.Filter? filter)
        {
            if (filter == null)
                return query;

            query = query.ApplyDynamicFilters(filter.Filters);
            query = query.ApplySort(filter.Sort);
            query = query.ApplyPagination(filter.Pagination);

            return query;
        }

        private static IQueryable<T> ApplyDynamicFilters<T>(this IQueryable<T> query, Filters? filters)
        {
            if (filters == null)
                return query;

            var entityType = typeof(T);
            var parameter = Expression.Parameter(entityType, "x");            // x =>;     
            // x =>;                
            //x.GroupId или x.TrainerId     
            //x.GroupId           
            //[1,2,3]
            // Это рефлексия напротив каждого параметра написано определение это сделано для того
            // чтобы можно было приенять темплейт и в целом мтеод сделать универсальным для всех обьектов

            foreach (var filterProp in typeof(Filters).GetProperties())
            {
                var value = filterProp.GetValue(filters);
                if (value == null) continue;

                var entityProp = entityType.GetProperty(filterProp.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            //x.GroupId или x.TrainerId    
                if (entityProp == null) continue;

                // Фильтрация по списку (IN)
                // Если пришел список [1, 2, 3], делаем WHERE GroupId IN (1, 2, 3)
                if (value is System.Collections.IEnumerable enumerable && !(value is string))
                {
                    var list = enumerable.Cast<object>().ToList();
                    if (!list.Any()) continue; // Пустой список - пропускаем

                    // Нужно построить query.Where(x => [1,2,3].Contains(x.GroupId))
                    // Но тип T неизвестен, поэтому строим через рефлексию
                    var member = Expression.Property(parameter, entityProp);     // x.GroupId

                    // Конвертируем List<object> в List<PropType> чтобы Contains работал корректно
                    var propType = entityProp.PropertyType;
                    var typedList = typeof(Enumerable)
                        .GetMethod("Cast")!
                        .MakeGenericMethod(propType)
                        .Invoke(null, new object[] { list })!;
                    var typedListTyped = typeof(Enumerable)
                        .GetMethod("ToList")!
                        .MakeGenericMethod(propType)
                        .Invoke(null, new object[] { typedList })!;

                    var constant = Expression.Constant(typedListTyped);          // [1, 2, 3] as List<long>

                    // Находим метод Contains для нужного типа (long, string и т.д.)
                    var containsMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                        .MakeGenericMethod(propType);

                    // Собираем вызов: [1,2,3].Contains(x.GroupId)
                    var body = Expression.Call(containsMethod, constant, member);
                    var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

                    query = query.Where(lambda);
                }

                // DateRange
                if (value is DateRange range)
                {
                    var member = Expression.Property(parameter, entityProp);

                    if (range.From.HasValue)
                    {
                        var fromExpr = Expression.GreaterThanOrEqual(member, Expression.Constant(range.From.Value));
                        var lambda = Expression.Lambda<Func<T, bool>>(fromExpr, parameter);
                        query = query.Where(lambda);
                    }

                    if (range.To.HasValue)
                    {
                        var toExpr = Expression.LessThanOrEqual(member, Expression.Constant(range.To.Value));
                        var lambda = Expression.Lambda<Func<T, bool>>(toExpr, parameter);
                        query = query.Where(lambda);
                    }
                }

                // int? range (AgeFrom/To, HeightFrom/To, WeightFrom/To)
                if (value is int intValue && filterProp.Name.EndsWith("From"))
                {
                    var baseName = filterProp.Name.Substring(0, filterProp.Name.Length - 4);
                    var targetProp = entityType.GetProperty(baseName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (targetProp != null)
                    {
                        var member = Expression.Property(parameter, targetProp);
                        var fromExpr = Expression.GreaterThanOrEqual(member, Expression.Constant(intValue, targetProp.PropertyType));
                        var lambda = Expression.Lambda<Func<T, bool>>(fromExpr, parameter);
                        query = query.Where(lambda);
                    }
                }

                if (value is int intValueTo && filterProp.Name.EndsWith("To"))
                {
                    var baseName = filterProp.Name.Substring(0, filterProp.Name.Length - 2);
                    var targetProp = entityType.GetProperty(baseName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (targetProp != null)
                    {
                        var member = Expression.Property(parameter, targetProp);
                        var toExpr = Expression.LessThanOrEqual(member, Expression.Constant(intValueTo, targetProp.PropertyType));
                        var lambda = Expression.Lambda<Func<T, bool>>(toExpr, parameter);
                        query = query.Where(lambda);
                    }
                }
            }

            return query;
        }

        private static IQueryable<T> ApplySort<T>(this IQueryable<T> query, Sort? sort)
        {
            if (sort?.Fields == null || !sort.Fields.Any())
                return query;

            IOrderedQueryable<T>? orderedQuery = null;

            foreach (var field in sort.Fields.Take(3))
            {
                var property = typeof(T).GetProperty(field.Field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null) continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var lambda = Expression.Lambda(propertyAccess, parameter);

                var isDesc = field.Direction?.ToLower() == "desc";

                string methodName = orderedQuery == null
                    ? (isDesc ? "OrderByDescending" : "OrderBy")
                    : (isDesc ? "ThenByDescending" : "ThenBy");

                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), property.PropertyType);

                orderedQuery = (IOrderedQueryable<T>)method.Invoke(null, new object[] { orderedQuery ?? query, lambda })!;
            }

            return orderedQuery ?? query;
        }

        public static IEnumerable<TeamStatsResponse> ApplyStatSort(this IEnumerable<TeamStatsResponse> stats, Filters? filters)
        {
            if (filters?.StatSortField == null)
                return stats.OrderByDescending(x => x.WinRate);

            var desc = filters.StatSortDirection?.ToLower() != "asc";

            return filters.StatSortField.ToLower() switch
            {
                "wins"         => desc ? stats.OrderByDescending(x => x.Wins)         : stats.OrderBy(x => x.Wins),
                "draws"        => desc ? stats.OrderByDescending(x => x.Draws)        : stats.OrderBy(x => x.Draws),
                "losses"       => desc ? stats.OrderByDescending(x => x.Losses)       : stats.OrderBy(x => x.Losses),
                "totalmatches" => desc ? stats.OrderByDescending(x => x.TotalMatches) : stats.OrderBy(x => x.TotalMatches),
                "winrate"      => desc ? stats.OrderByDescending(x => x.WinRate)      : stats.OrderBy(x => x.WinRate),
                _              => stats.OrderByDescending(x => x.WinRate)
            };
        }

        private static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, Pagination? pagination)
        {
            if (pagination == null)
                return query;

            return query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize);
        }
    }
}
