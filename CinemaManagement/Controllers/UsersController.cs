using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using CinemaManagement.Data;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager,
                           SignInManager<ApplicationUser> signInManager,
                           RoleManager<IdentityRole> roleManager,
                           AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }

    private void SetDropdownData()
    {
        ViewBag.Theaters = _context.Theaters.ToList();
        ViewBag.Genres = _context.Genres.ToList();
    }

    public IActionResult Login()
    {
        SetDropdownData();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        SetDropdownData();

        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
                return RedirectToAction("Index", "Movies");
            }
            ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu!");
        }
        return View(model);
    }

    public IActionResult Register()
    {
        SetDropdownData();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        SetDropdownData();

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            TempData["RegisterErrors"] = string.Join("; ", errors);
            return View(model);
        }

        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    public async Task<IActionResult> Index()
    {
        SetDropdownData();

        var users = await _userManager.Users.ToListAsync();
        var userRoles = new Dictionary<string, IList<string>>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles;
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Người dùng không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Admin"))
        {
            TempData["Error"] = "Không thể xóa tài khoản có vai trò Admin.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Đã xóa người dùng thành công.";
        }
        else
        {
            TempData["Error"] = "Xóa người dùng thất bại.";
        }

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> EditRoles(string id)
    {
        SetDropdownData();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var allRoles = await _roleManager.Roles.ToListAsync();
        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            Roles = allRoles.Select(r => new RoleSelection
            {
                RoleName = r.Name,
                IsSelected = userRoles.Contains(r.Name)
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> EditRoles(EditRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            TempData["Error"] = "Người dùng không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        // Không cho phép tự thay đổi vai trò của chính mình
        var currentUserId = _userManager.GetUserId(User);
        if (user.Id == currentUserId)
        {
            TempData["Error"] = "Bạn không thể thay đổi vai trò của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, existingRoles);
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
            TempData["Success"] = $"Cập nhật vai trò của người dùng {user.UserName} thành công.";
        }

        return RedirectToAction(nameof(Index));
    }

}
