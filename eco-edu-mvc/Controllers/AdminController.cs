using eco_edu_mvc.Models.AccountsViewModel;
using eco_edu_mvc.Models.AdminViewModel;
using eco_edu_mvc.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eco_edu_mvc.Controllers;
public class AdminController(EcoEduContext context) : Controller
{
    private readonly EcoEduContext _context = context;

    public async Task<ActionResult> Index()
    {
        if (HttpContext.Session.GetString("Role") == "Admin")
        {
            var surveys = await _context.Surveys.Where(s => s.Active == true).CountAsync();
            var competitions = await _context.Competitions.Where(c => c.Active == true).CountAsync();
            var users = await _context.Users.Where(u => u.Role == "Staff" && u.Role == "Student").CountAsync();
            var seminars = await _context.Seminars.Where(m => m.Active == true).CountAsync();

            AdminModel model = new()
            {
                Surveys = surveys,
                Competitions = competitions,
                Users = users,
                Seminars = seminars
            };
            return View(model);
        }
        TempData["PermissionDenied"] = true;
        return RedirectToAction("index", "home");
    }

    public async Task<IActionResult> RequestShow()
    {
        if (HttpContext.Session.GetString("Role") == "Admin")
        {
            return View(await _context.Users.ToListAsync());
        }
        TempData["PermissionDenied"] = true;
        return RedirectToAction("index", "home");
    }

    [HttpPost]
    public async Task<IActionResult> Accept(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        user.IsAccept = true;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("RequestShow");
    }

    [HttpPost]
    public async Task<IActionResult> Decline(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("RequestShow");
    }

    public IActionResult Update()
    {
        if (HttpContext.Session.GetString("Role") == "Admin")
        {
            return View();
        }
        TempData["PermissionDenied"] = true;
        return RedirectToAction("index", "home");
    }

    [HttpPost]
    public async Task<IActionResult> Update(EditUserModel model, int id)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();
            if (model.EntryDate >= DateTime.Now)
            {
                user.EntryDate = model.EntryDate;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("RequestShow");
            }
            ModelState.AddModelError("EntryDate", "Entrydate must not be a future day!");
            return View(model);
        }
        ModelState.AddModelError("EntryDate", "Entrydate is invalid!");
        return View(model);
    }

    public async Task<IActionResult> UserList()
    {
        if (HttpContext.Session.GetString("Role") == "Admin")
        {
            return View(await _context.Users.ToListAsync());
        }
        TempData["PermissionDenied"] = true;
        return RedirectToAction("index", "home");
    }

    [HttpPost]
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsBan = true;

        _context.Update(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("UserList");
    }

    public IActionResult ChangePasswordAdmin()
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["PermissionDenied"] = true;
            return RedirectToAction("index", "home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePasswordAdmin(ChangePasswordModel model)
    {
        try
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["PermissionDenied"] = true;
                return RedirectToAction("index", "home");
            }
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var user = await context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return RedirectToAction("login", "account");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("Error", ex.Message);
            return View();
        }
    }
}
