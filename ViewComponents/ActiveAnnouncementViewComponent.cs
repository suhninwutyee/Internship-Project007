//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ProjectManagementSystem.Data;

//namespace ProjectManagementSystem.ViewComponents
//{
//    public class ActiveAnnouncementViewComponent : ViewComponent
//    {
//        private readonly ApplicationDbContext _context;

//        public ActiveAnnouncementViewComponent(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IViewComponentResult> InvokeAsync()
//        {
//            var now = DateTime.Now;
//            var announcement = await _context.Announcements
//                .Where(a => a.ShowAsPublicNotice &&
//                       a.StartDate <= now &&
//                       (a.ExpiryDate == null || a.ExpiryDate >= now))
//                .OrderByDescending(a => a.IsUrgent)
//                .ThenByDescending(a => a.CreatedDate)
//                .FirstOrDefaultAsync();

//            return View("_AnnouncementAlert", announcement);
//        }
//    }
//}
