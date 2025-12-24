using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRISM.DataAccess;
using PRISM.Dto;
using PRISM.Services.IServices;
using System.Drawing;
using System.Security.Claims;

namespace PRISM.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private IReportService _reportService;
        public ReportController(AppDbContext context , IReportService reportService)
        {
            _reportService = reportService;
            _context = context;
        }

        // GET: Report/Index
        public async Task<IActionResult> Index(int? businessId, DateTime? startDate, DateTime? endDate, string reportType = "summary")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Get user's businesses
            var userBusinesses = await _context.Businesses
                .Where(b => !b.IsDeleted && b.UserId == userId)
                .ToListAsync();

            if (!userBusinesses.Any())
            {
                ViewBag.NoBusiness = true;
                ViewBag.Businesses = new List<Business>();
                return View(new ReportViewModel());
            }
            // Set date range defaults
            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            var viewModel = await _reportService.GetIndexAsync(userId, businessId, startDate, endDate, reportType);

            ViewBag.Businesses = userBusinesses;
            ViewBag.SelectedBusinessId = businessId;


            return View(viewModel);
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel(int? businessId, DateTime? startDate, DateTime? endDate, string? reportType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!startDate.HasValue)
                startDate = DateTime.Now.AddMonths(-1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;
            var ExcelFile = await _reportService.ExportToExcelAsync(userId, businessId, startDate, endDate, reportType);

            return File(ExcelFile.Content, ExcelFile.ContentType, ExcelFile.FileName);
        }

    }
}