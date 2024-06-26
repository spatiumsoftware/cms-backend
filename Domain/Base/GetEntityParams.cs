﻿namespace Domain.Base
{
    public class GetEntitiyParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchColumn { get; set; }
        public string SearchValue { get; set; }
        public string FilterColumn { get; set; }
        public string FilterValue { get; set; }
        public string SortColumn { get; set; }
        public bool IsDescending { get; set; } = false;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
