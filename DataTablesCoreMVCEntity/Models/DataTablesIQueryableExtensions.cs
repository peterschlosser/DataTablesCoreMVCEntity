using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTablesCoreMVCEntity.Models
{
    /// <summary>
    /// LINQ-to-SQL extension methods for DataTables Requests
    /// </summary>
    public static class DataTablesIQueryableExtensions
    {
        /// <summary>
        /// Applies DataTables search filtering to the LINQ-to-SQL query.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Linq.IQueryable to filter.</param>
        /// <param name="request">A DataTablesRequest containing the search condition.</param>
        /// <returns>An System.Linq.IQueryable containing the filter predicate to satisfy the condition.</returns>
        /// <remarks>
        /// This method builds and applies an expression tree predicate using iQueryable.Where() to search 
        /// each of the searchable columns (TSource properties) named in the DataTablesRequest for the search value.
        /// Expression trees are used (instead of anonymous methods) allowing IQueryable LINQ-to-SQL to translate our
        /// predicate into the respective SQL statements.  Using anonymous methods, for example: IQueryable.Where(s =>
        /// s.column1.Contains(value) || s.column2.Contains(value) ...) would result in IEnumerable LINQ-to-Objects
        /// reading the entire DbSet into memory before applying the filter.  Using expression trees is more efficient
        /// since the filter may be applied prior to reading records from the DbSet.
        /// </remarks>
        public static IQueryable<TSource> WhereDataTables<TSource>(
            this IQueryable<TSource> source, DataTablesRequest request)
        {
            var result = source;

            if (string.IsNullOrWhiteSpace(request.Search?.Value) == false)
            {
                // foreach searchable column name in the DataTablesRequest, build a where-clause
                // predicate searching each property of TSource for the search value.
                // IQueryable.Where(s => s.Column1.Contains(Value) || s.Column2.Contains(Value) ...)
                // => WHERE [Column1] LIKE "%Value%" OR [Column2] LIKE "%Value%" ...
                ParameterExpression parameter = Expression.Parameter(typeof(TSource), "s");
                MethodInfo contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                Expression value = Expression.Constant(request.Search.Value, typeof(string));
                Expression predicate = null;
                foreach (var propertyName in request.Columns.Where(c => c.Searchable).Select(c => c.Name))
                {
                    try
                    {
                        Expression property = Expression.Property(parameter, propertyName);
                        Expression containsExp = Expression.Call(property, contains, value);
                        predicate = (predicate == null)
                            ? containsExp
                            : Expression.OrElse(predicate, containsExp);
                    }
                    catch (Exception ex)
                    {
                        request.Error += string.Format("{0}: {1}\n{2}",
                            ex.GetType().Name,
                            ex.Message.TrimEnd('.'),
                            ex.StackTrace);
                        return source;
                    }
                }
                result = Queryable.Where(source, Expression.Lambda<Func<TSource, bool>>(predicate, parameter));
            }
            return result;
        }

        /// <summary>
        /// Sorts the elements of a sequence using DataTables order criteria.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="request">A DataTablesRequest containing the sorting criteria.</param>
        /// <returns>An System.Linq.IOrderedQueryable whose elements are sorted according to criteria.</returns>
        /// <remarks>
        /// This method applies one or more sorting criteria using Order items from DataTablesRequest.
        /// </remarks>
        public static IOrderedQueryable<TSource> OrderByDataTables<TSource>(this IQueryable<TSource> source, DataTablesRequest request)
        {
            if (request.Order.Count > 0)
            {
                // foreach column order criteria in the DataTablesRequest, apply element sorting. 
                // IQueryable.OrderByDescending(s => s.Column1).ThenBy(s => s.Column2).ThenByDescending(s => s.Column3) ...
                // => ORDER BY [Column1] DESC, [Column2], [Column3] DESC ...
                var expression = source.Expression;
                var parameter = Expression.Parameter(typeof(TSource), string.Empty);
                var method = string.Empty;
                foreach (var order in request.Order)
                {
                    try
                    {
                        method = string.IsNullOrEmpty(method) ? "OrderBy" : "ThenBy";
                        method += (order.Descending ? "Descending" : string.Empty);
                        var property = Expression.PropertyOrField(parameter, request.Columns[order.Column].Name);
                        var keyType = new[] { typeof(TSource), property.Type };
                        var keyName = Expression.Quote(Expression.Lambda(property, parameter));
                        expression = Expression.Call(typeof(Queryable), method, keyType, expression, keyName);
                    }
                    catch (Exception ex)
                    {
                        request.Error += string.Format("{0}: {1}\n{2}",
                            ex.GetType().Name,
                            ex.Message.TrimEnd('.'),
                            ex.StackTrace);
                        return (IOrderedQueryable<TSource>)source;
                    }
                }
                return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(expression);
            }
            return (IOrderedQueryable<TSource>)source;
        }
    }
}
