using Microsoft.AspNetCore.Mvc;

namespace ApiwebMVC.Controllers;

public class NosotrosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
