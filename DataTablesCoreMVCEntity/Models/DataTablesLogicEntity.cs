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
        /// Handles the DataTablesRequest for the Customer Database using Entity Framework and LINQ-to-SQL
        /// </summary>
        /// <param name="request">DataTables Ajax Request</param>
        /// <returns>DataTablesResponse</returns>
        public static async Task<DataTablesResponse<Customer>> DataTablesRequestAsync(DataTablesRequest request, CustomerContext dbContext)
        {
            // prepare the data and count queries
            var totalQuery = dbContext.Customers;
            var filterQuery = totalQuery
                .WhereDataTables(request);
            var dataQuery = filterQuery
                .OrderByDataTables(request)
                .Skip(request.Start)
                .Take(request.Length);

            // run the queries and gather the data
            var total = totalQuery.Count();
            var filtered = filterQuery.Count();
            var data = await dataQuery.ToListAsync();
            var error = request.Error;

            // prepare and return the repsonse
            return new DataTablesResponse<Customer>()
            {
                Draw = request.Draw,
                RecordsTotal = total,
                RecordsFiltered = filtered,
                Data = data,
                Error = error,
            };
        }
    }
}
