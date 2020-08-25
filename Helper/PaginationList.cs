﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Helper
{
    public class PaginationList<T>:List<T>
    {

        public int TotalPages { get; private set; }
        public int TotalCount { get; private set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public PaginationList(int totalCount,int currentPage, int pageSize, List<T> items)
        {
            
            CurrentPage = currentPage;
            PageSize = pageSize;
            AddRange(items);//添加列表数据，需要输出的数据列表14-3 4：32
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        }

        public static async Task<PaginationList<T>> CreateAsync(// factory pattern
            int currentPage, int pageSize, IQueryable<T> result)
        {
            var totalCount = await result.CountAsync();
            // pagination   
            // skip
            var skip = (currentPage - 1) * pageSize;
            result = result.Skip(skip);
            // 以pagesize为标准显示一定量的数据
            result = result.Take(pageSize);

            // include vs join
            var items = await result.ToListAsync();

            return new PaginationList<T>(totalCount,currentPage, pageSize, items);
        }
    }
}
