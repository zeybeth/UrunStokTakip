using Data.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UrunStokTakip.ViewComponents;

public class CartCountViewComponent : ViewComponent
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<IdentityUser> _userManager;

    public CartCountViewComponent(IUnitOfWork uow, UserManager<IdentityUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        //  Kullanıcı giriş yapmış mı?
        if (User.Identity.IsAuthenticated)
        {
            //  Kullanıcının ID'sini al
            var userId = ((ClaimsPrincipal)User).FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                // Bu kullanıcıya ait aktif sepet öğelerinin sayısını bul
                var count = await _uow.Carts.GetAll()
                    .Where(x => x.UserId == userId)
                    .CountAsync();

                return View(count); // Sayıyı View'a gönder
            }
        }
        return View(0); // Giriş yapmamışsa 0 gönder
    }
}