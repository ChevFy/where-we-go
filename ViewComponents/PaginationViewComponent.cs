using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using where_we_go.DTO;

namespace where_we_go.ViewComponents
{
    public class PaginationViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(
            PaginatedMetaDto meta,
            string action,
            string controller,
            object? routeValues = null,
            int radius = 2)
        {
            var vm = new PaginationViewModel
            {
                Meta = meta,
                Action = action,
                Controller = controller,
                RouteValues = new RouteValueDictionary(routeValues ?? new { }),
                Items = BuildPageItems(meta.CurrentPage, meta.LastPage, radius)
            };

            return Task.FromResult<IViewComponentResult>(View(vm));
        }

        private static List<PaginationItemViewModel> BuildPageItems(int currentPage, int lastPage, int radius)
        {
            var result = new List<PaginationItemViewModel>();
            if (lastPage <= 0) return result;

            var pages = new SortedSet<int> { 1, lastPage };
            var start = Math.Max(1, currentPage - radius);
            var end = Math.Min(lastPage, currentPage + radius);

            for (var p = start; p <= end; p++) pages.Add(p);

            int? prev = null;
            foreach (var p in pages)
            {
                if (prev.HasValue && p - prev.Value > 1)
                {
                    result.Add(new PaginationItemViewModel
                    {
                        Text = "...",
                        IsEllipsis = true
                    });
                }

                result.Add(new PaginationItemViewModel
                {
                    Page = p,
                    Text = p.ToString(),
                    IsActive = p == currentPage
                });

                prev = p;
            }

            return result;
        }
    }

    public class PaginationViewModel
    {
        public required PaginatedMetaDto Meta { get; init; }
        public required string Action { get; init; }
        public required string Controller { get; init; }
        public required RouteValueDictionary RouteValues { get; init; }
        public required List<PaginationItemViewModel> Items { get; init; }
    }

    public class PaginationItemViewModel
    {
        public int? Page { get; init; }
        public required string Text { get; init; }
        public bool IsActive { get; init; }
        public bool IsEllipsis { get; init; }
    }
}
