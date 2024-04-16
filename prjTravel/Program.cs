using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();

builder.Services.AddDbContext<prjTravel.Models.TravelDbContext>
    (options => options.UseSqlServer(builder.Configuration.GetConnectionString("TravelDbContext")));

/*Cookie驗證方式*/
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {   
        options.Cookie.HttpOnly = true;                                     //經由Http協定存取
        options.LoginPath = new PathString("/Home/Login");                  //未登入時使用Home的Login方法
        options.AccessDeniedPath = new PathString("/Home/NoAuthorization"); //權限不足時呼叫Home的NoAuthorization方法
    });

var app = builder.Build();

app.UseStaticFiles();       //啟用靜態資源

app.UseCookiePolicy();      //啟用Cookie政策

app.UseAuthentication();    //啟用身分驗證

app.UseAuthorization();     //啟用身分授權

/*預設執行路徑*/
app.MapControllerRoute(name: "default",
    pattern: "{Controller=Home}/{Action=Index}/{id?}");

app.Run();
