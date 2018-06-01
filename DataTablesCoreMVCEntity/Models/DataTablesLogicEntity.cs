// Copyright (c) Peter Schlosser. All rights reserved.  Licensed under the MIT license. See LICENSE.txt in the project root for license information.
using DataTablesCoreMVCEntity.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataTablesCoreMVCEntity.Models
{
    public class DataTablesLogicEntity
    {
        /// <summary>
        /// Handles generic DataTablesRequest for the TSource Database using Entity Framework and LINQ-to-SQL
        /// </summary>
        /// <param name="request">DataTables Ajax Request</param>
        /// <returns>DataTablesResponse for TSource data</returns>
        public static async Task<DataTablesResponse<TSource>> DataTablesRequestAsync<TSource>(
            DataTablesRequest request, DbSet<TSource> dbSet) where TSource : class
        {
            // prepare the data and count queries
            var totalQuery = dbSet;
            var filterQuery = totalQuery.WhereDataTables(request);
            var dataQuery = filterQuery.OrderByDataTables(request).Skip(request.Start).Take(request.Length);

            // run the queries and return the response
            return new DataTablesResponse<TSource>()
            {
                Draw = request.Draw,
                RecordsTotal = totalQuery.Count(),
                RecordsFiltered = filterQuery.Count(),
                Data = await dataQuery.ToListAsync(),
                Error = request.Error,
            };
        }

        /// <summary>
        /// Handles the DataTablesRequest for the Customer Database
        /// </summary>
        /// <param name="request">DataTables Ajax Request</param>
        /// <param name="dbContext">CustomerContext for Customer data</param>
        /// <returns>DataTablesResponse for Customer</returns>
        public static async Task<DataTablesResponse<Customer>> DataTablesCustomerRequestAsync(DataTablesRequest request, CustomerContext dbContext)
        {
            var dbset = dbContext.Customers;
            return await DataTablesRequestAsync(request, dbset);
        }
    }
}
