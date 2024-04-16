using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using prjTravel.Models;
using System.Security.Claims;
using static NuGet.Packaging.PackagingConstants;

namespace prjTravel.Controllers
{
    public class HomeController : Controller
    {
        private TravelDbContext _dbContext;
        private string _path;

        public HomeController(TravelDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _path = $"{webHostEnvironment.WebRootPath}\\pictures";
        }

        //首頁
        public IActionResult Index(string? SearchFolder = null)
        {

            string Search = (SearchFolder != null ? SearchFolder : "");

            //Folder串Clasify，status狀態不是1的不顯示
            var HotFolder = (from folders in _dbContext.Folders
                            join classify in _dbContext.Classifies on folders.Fcid equals classify.Cid into folders_classify
                            from classify in folders_classify.DefaultIfEmpty()
                            where classify.Cstatus == 1 && folders.Fstatus == 1 && folders.Ftitle!.Contains(Search)
                            orderby folders.FcreateTime descending
                            select folders)
                            .Take(10) //前10筆
                            .ToList();

            return View(HotFolder);
        }

        //照片分類查詢
        public IActionResult FolderClassify(int? Cid = null,string? SearchFolder = null)
        {
            string Search = SearchFolder != null ? SearchFolder : "";

            /*查詢的時候需要類別代號*/
            ViewData["ClassifyKey"] = _dbContext.Classifies
                .Where(m => m.Cstatus == 1)
                .FirstOrDefault(m => m.Cid == Cid)?
                .Cid.ToString() ?? "查無資料";

            ViewBag.ClassifyName = _dbContext.Classifies
                .Where(m => m.Cstatus == 1)
                .FirstOrDefault(m => m.Cid == Cid)?
                .Cname ?? "查無資料";

            var folderClassify = _dbContext.Folders
                .Where(m => m.Fcid == Cid && m.Fstatus == 1 && m.Ftitle!.Contains(Search))
                .OrderByDescending(m => m.FeditTime)
                .ToList();

            return View(folderClassify);
        }

        //新增資料頁面
        //[Authorize(Roles = "Admin,Manager,User")]
        //public IActionResult FolderUpload()
        //{
        //    return View();
        //}

        //新增資料方法
        //[HttpPost]
        //[Authorize(Roles = "Admin,Manager,User")]
        //public async Task<IActionResult> FolderUpload(Folder folder, IFormFile formFile)
        //{
        //    string Errormsg;
        //    string fileName;
        //    string savePath = "";

        //    if (ModelState.IsValid)
        //    {
        //        if (formFile != null && formFile.Length > 0)
        //        {
        //            try
        //            {
        //                /*圖片檔案名稱給予隨機值、設定路徑，並使用非同步方式存檔。*/
        //                //fileName = $"{Guid.NewGuid()}.jpg"; 我要直接從DB洗資料進去，如果圖檔變成隨機碼我認不出來先取消。
        //                fileName = $"{folder.Fcid}_{folder.FfolderId}.jpg";
        //                savePath = $"{_path}\\{fileName}";
        //                using (var steam = new FileStream(savePath, FileMode.Create))
        //                {
        //                    await formFile.CopyToAsync(steam);
        //                }

        //                /*存入檔案名稱以及建立時間*/
        //                folder.Fpicture = fileName;
        //                folder.FcreateTime = DateTime.Now;
        //                folder.FeditTime = DateTime.Now;

        //                _dbContext.Folders.Add(folder);
        //                _dbContext.SaveChanges();
        //                TempData["Success"] = "資料上傳";
        //                return RedirectToAction("FolderClassify", new { Cid = folder.Fcid });
        //            }
        //            catch (Exception)
        //            {
        //                Errormsg = "可能是編號重複";
        //                System.IO.File.Delete($"{savePath}");   //建立失敗就刪掉圖檔
        //            }
        //        }
        //        else 
        //        {
        //            Errormsg = "上傳圖片格式有誤";
        //        }
        //    }
        //    else
        //    {
        //        Errormsg = "資料驗證失敗";
        //    }

        //    TempData["Error"] = $"資料上傳失敗, {Errormsg}";
        //    return View();
        //}

        //登入頁面
        public IActionResult Login()
        {
            return View();
        }

        //登入方法
        [HttpPost]
        public IActionResult Login(string uid, string pwd)
        {
            var member = _dbContext.Members
                .FirstOrDefault(m => m.Muid == uid && m.Mpwd == pwd && m.Mstatus == 1);

            //判斷帳密是否存在
            if (member != null) 
            {
                //宣告身分驗證陣列
                IList<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Sid, member.Muid),
                    new Claim(ClaimTypes.Name, member.Mname!),
                    new Claim(ClaimTypes.Role, member.Mrole!)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var properties = new AuthenticationProperties { IsPersistent = true };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    properties);

                TempData["Success"] = "登入成功";
                return RedirectToAction("Index", member.Mrole);
            }

            TempData["Error"] = "帳密錯誤或是關閉請聯絡管理員!";
            return View();
        }

        //登出
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }


        [HttpPost]
        [HttpGet]
        /*新增資料時判斷客戶編號是否重複 當作共用方法放在Home控制器裡*/
        public IActionResult CheckFfolderId(string FfolderId)
        {
            string FolderTemp = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == FfolderId)?.FfolderId ?? "";
            
            /*如果找到重複的回傳false代表不能再新增了*/
            if (FolderTemp == FfolderId)
            {
                return Json(false);
            }

            return Json(true);
        }

        [HttpPost]
        [HttpGet]
        /*新增成員的時候檢查是否有重複的帳號*/
        public IActionResult CheckMuid(string Muid)
        {
            string MemberItem = _dbContext.Members.FirstOrDefault(m => m.Muid == Muid)?.Muid ?? "";

            if (MemberItem == Muid)
            {
                return Json(false);
            }

            return Json(true);
        }

    }
}
