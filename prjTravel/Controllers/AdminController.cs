using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using prjTravel.Models;
using System.Security.Claims;

namespace prjTravel.Controllers
{
    /*限Admin*/
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private TravelDbContext _dbContext;
        private string _Path;

        public AdminController(TravelDbContext dbContext, IWebHostEnvironment hostEnvironment)
        {
            _dbContext = dbContext;
            _Path = $@"{hostEnvironment.WebRootPath}\pictures";
        }

        //Admin主頁 如果帶值代表使用查詢功能
        public IActionResult Index(string? SearchClassify = null)
        {
            string Search = SearchClassify != null ? SearchClassify : "";    //避免Null

            var classifyItem = _dbContext.Classifies
                .Where(x => x.Cname!.Contains(Search))
                .OrderBy(m => m.Cid)
                .ToList();

            return View(classifyItem);
        }

        //刪除分類方法
        public IActionResult ClassifyDelete(int Cid)
        {
            try
            {
                var classify = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid)!;
                var folders = _dbContext.Folders.Where(m => m.Fcid == Cid)!.ToList();

                var pictureTemp = (from foldersItem in _dbContext.Folders
                                   join pictureItem in _dbContext.FolderPictures
                                        on foldersItem.Fpicture equals pictureItem.Pfid into folders_pictures
                                   from pictureItem in folders_pictures.DefaultIfEmpty()
                                   where foldersItem.Fcid == Cid
                                   select pictureItem);

                /*判斷是否存在該類別名稱資料夾如果有幫我刪掉*/
                string Path = $@"{_Path}\{classify.Cid}";
                bool IsExists = DirectoryIsExists($"{Path}");

                _dbContext.Classifies.Remove(classify);
                _dbContext.Folders.RemoveRange(folders);
                _dbContext.FolderPictures.RemoveRange(pictureTemp);
                _dbContext.SaveChanges();

                DeleteDirectory(IsExists, Path);
                TempData["Success"] = "成功刪除分類";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"刪除分類失敗, {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        //新增分類頁面
        public IActionResult ClassifyCreate()
        {
            return View();
        }

        //新增分類方法
        [HttpPost]
        public IActionResult ClassifyCreate(Classify classify)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    classify.Cstatus = 1;    //創建時狀態預設給1
                    _dbContext.Classifies.Add(classify);

                    _dbContext.SaveChanges();
                    TempData["Success"] = "新增分類成功";

                    /*建立新類別判斷是否存在該類別名稱資料夾如果沒有幫我建一個*/
                    bool IsExists = DirectoryIsExists($@"{_Path}\{classify.Cid}");
                    string Path = $@"{_Path}\{classify.Cid}";
                    AddDirectory(IsExists, Path);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"新增分類失敗, {ex}";
                }
            }
            return View();
        }

        //修改分類顯示狀態
        public IActionResult ClassifyStatus(int Cid)
        {
            var classify = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid)!;

            if (classify.Cstatus == 1)
            {
                classify.Cstatus = 0;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功關閉『{classify.Cname}』分類";
            }
            else if (classify.Cstatus == 0)
            {
                classify.Cstatus = 1;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功開啟『{classify.Cname}』分類";
            }

            return RedirectToAction("Index");
        }

        //修改類別名稱頁面
        public IActionResult ClassifyEdit(int Cid)
        {
            var classify = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid);

            if (classify == null)
            {
                TempData["Error"] = "無此相簿分類，請確認後重新送出";
                return RedirectToAction("Index");
            }

            return View(classify);
        }

        //修改類別名稱方法
        [HttpPost]
        public IActionResult ClassifyEdit(Classify classify)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int Cid = classify.Cid;
                    var tempName = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid);

                    if (!String.IsNullOrEmpty(tempName?.Cname))
                    {
                        tempName.Cname = classify.Cname;
                        
                        _dbContext.SaveChanges();
                        TempData["Success"] = "相簿分類名稱修改成功";
                    }
                    else
                    {
                        TempData["Error"] = "無此相簿分類，請確認後重新送出";
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "相簿分類名稱修改失敗";
                }
            }
            return RedirectToAction("Index");
        }

        //依相簿分類編號取得該分類的所有照片
        public IActionResult FolderClassify(int? Cid = null, int? FolderStatus = null, string? SearchFolder = null)
        {
            int? Status = FolderStatus;
            string Search = SearchFolder != null ? SearchFolder : "";
            ViewData["ClassifyKey"] = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid)?.Cid.ToString() ?? "查無資料";

            ViewBag.Classify = _dbContext.Classifies.FirstOrDefault(m => m.Cid == Cid)?.Cname ?? "查無資料";

            var Folders = _dbContext.Folders
                .Where(m => m.Fcid == Cid && m.Ftitle!.Contains(Search) && (Status == null || m.Fstatus == Status))
                .OrderByDescending(m => m.FeditTime)
                .ToList();

            return View(Folders);
        }

        //修改資料顯示狀態
        public IActionResult FolderStatus(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid)!;

            if (folder.Fstatus == 1)
            {
                folder.Fstatus = 0;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功關閉『{folder.Ftitle}』資料";
            }
            else if (folder.Fstatus == 0)
            {
                folder.Fstatus = 1;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功開啟『{folder.Ftitle}』資料";
            }

            return RedirectToAction("FolderClassify", new { Cid = folder.Fcid });
        }

        //編輯資料畫面
        public IActionResult FolderEdit(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid);

            if (folder == null)
            {
                TempData["Error"] = "查無資料，請確認資料正確性";
                return RedirectToAction("Index");
            }

            return View(folder);
        }

        //編輯資料方法
        [HttpPost]
        public async Task<IActionResult> FolderEdit(Folder folder, List<IFormFile>? TitleFormfiles)   //IFormFile一定要加上?可以接受Null不然ModelState.IsValid不會過 吃了大虧...
        {
            string Errormsg;

            if (ModelState.IsValid)
            {
                try
                {
                    string? folderId = folder.FfolderId;
                    var folderTemp = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == folderId)!;
                    int TitleNum = 0;
                    /*如果使用者更改類別才觸發*/
                    if (folderTemp.Fcid != folder.Fcid)
                    {
                        string OldMenu, OldPath, NewMenu, NewPath;

                        //找出舊資料
                        var folderPictureTemp = _dbContext.FolderPictures
                            .Where(m => m.Pfid == folderTemp.Fpicture)
                            .ToList();

                        foreach (var picture in folderPictureTemp)
                        {
                            TitleNum++;
                            OldMenu = $@"{_Path}\{folderTemp.Fcid}";
                            NewMenu = $@"{_Path}\{folder.Fcid}";

                            /*如果資料夾不存在建立，避免後面引發錯誤*/
                            if (!DirectoryIsExists(OldMenu))
                            {
                                AddDirectory(DirectoryIsExists(OldMenu), OldMenu);
                            }

                            if (!DirectoryIsExists(NewMenu))
                            {
                                AddDirectory(DirectoryIsExists(NewMenu), NewMenu);
                            }

                            /*找出舊資料*/
                            OldPath = $@"{_Path}\{folderTemp.Fcid}\{picture.Ppicture}";

                            /*確認新舊資料夾都存在後，判斷檔案在不在由搬移的Function去做處理*/
                            picture.Ppicture = $"Title_{folder.Fcid}_{folder.FfolderId}_{TitleNum}_{new Random().Next(999)}.jpg";
                            NewPath = $@"{_Path}\{folder.Fcid}\{picture.Ppicture}";
                            ChangePicturesByMenu(OldPath, NewPath);

                            _dbContext.SaveChanges();
                        }
                    }

                    folderTemp.Fcid = folder.Fcid!;
                    folderTemp.Ftitle = folder.Ftitle;
                    folderTemp.Fdescription = folder.Fdescription;
                    folderTemp.FeditTime = DateTime.Now;
                    folderTemp.FeditUser = User.FindFirst(ClaimTypes.Sid)!.Value;

                    _dbContext.SaveChanges();

                    /*如果TitleFormfiles有值代表使用者修改圖片*/
                    if (TitleFormfiles != null && TitleFormfiles.Count > 0)
                    {
                        DeletePicture(folderTemp.Fcid, folderTemp.Fpicture); //刪除舊圖檔
                        await CreatePicture(folder, TitleFormfiles);
                    }

                    TempData["Success"] = "資料修改成功";
                    return RedirectToAction("FolderClassify", new { Cid = folderTemp.Fcid });
                }
                catch (Exception)
                {
                    Errormsg = "可能是客戶編號重複";
                }
            }
            else
            {
                string Errorstr = "";
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Errorstr = $"{entry.Key}: {error.ErrorMessage}";
                    }
                }
                Errormsg = $"{Errorstr}";
            }

            TempData["Error"] = $"資料上傳失敗, {Errormsg}";
            return View();
        }

        /*幻燈片圖片順序編輯頁面*/
        public IActionResult PictureRow(string Fpicture)
        {
            var fold = _dbContext.Folders.FirstOrDefault(m => m.Fpicture == Fpicture);

            if (fold == null)
            {
                TempData["Error"] = $"查無此資料，請確認資料正確性!";
                return RedirectToAction("Index");
            }

            ViewBag.FoldTitle = fold?.Ftitle;
            ViewBag.FoldCid = fold?.Fcid;
            ViewBag.PictureFid = _dbContext.FolderPictures.FirstOrDefault(m => m.Pfid == Fpicture)!.Pfid;

            var Picture = _dbContext.FolderPictures.Where(m => m.Pfid == Fpicture).OrderBy(m => m.Prow).ToList();

            return View(Picture);
        }

        /*幻燈片圖片順序編輯方法*/
        [HttpPost]
        public IActionResult PictureRow(List<FolderPicture> folderPicture)
        {
            try
            {
                var FolderItem = _dbContext.Folders.FirstOrDefault(m => m.Fpicture == folderPicture[0].Pfid)!;

                foreach (var item in folderPicture)
                {
                    var folderpicture = _dbContext.FolderPictures.FirstOrDefault(m => m.Pid == item.Pid)!;
                    folderpicture.Prow = item.Prow;
                }

                FolderItem.FeditTime = DateTime.Now;
                FolderItem.FeditUser = User.FindFirst(ClaimTypes.Sid)!.Value;

                _dbContext.SaveChanges();
                TempData["Success"] = "圖片順序修改成功";

                return RedirectToAction("FolderClassify", new { Cid = FolderItem!.Fcid });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"圖片順序修改失敗,{ex.Message}";
            }
            return View();
        }

        //刪除資料
        public IActionResult FolderDelete(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid)!;
            var picture = _dbContext.FolderPictures.Where(m => m.Pfid == folder.Fpicture);
            DeletePicture(folder.Fcid, folder.Fpicture);

            _dbContext.Folders.Remove(folder);
            _dbContext.FolderPictures.RemoveRange(picture);
            _dbContext.SaveChanges();
            TempData["Success"] = "資料刪除成功";
            return RedirectToAction("FolderClassify", new { Cid = folder.Fcid });
        }

        //顯示會員列表
        public IActionResult MemberList(string? SearchMember = null, string? SearchClassify = null)
        {
            string member = SearchMember != null ? SearchMember : "";
            string classify = SearchClassify != null ? SearchClassify : "";

            var MemberItem = _dbContext.Members
                .Where(m => m.Muid.Contains(member) && m.Mrole!.Contains(classify))
                .OrderBy(m => m.Mrole);
            //.Where(m => m.Mrole != "Admin");

            ViewBag.Sid = User.FindFirst(ClaimTypes.Sid)!.Value;

            return View(MemberItem);
        }

        //會員列表改變帳號狀態
        [HttpPost]
        public IActionResult MemberList(Member member)
        {
            try
            {
                string uid = member.Muid;
                var memberItem = _dbContext.Members.FirstOrDefault(m => m.Muid == uid)!;

                memberItem.Mstatus = member.Mstatus;

                TempData["Success"] = $"修改{member.Muid}權限成功";
                _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = $"修改{member.Muid}權限失敗";
            }

            return View(member);
        }

        //修改會員狀態
        public IActionResult MemberStatus(string Uid)
        {
            var folder = _dbContext.Members.FirstOrDefault(m => m.Muid == Uid)!;

            if (folder.Mstatus == 1)
            {
                folder.Mstatus = 0;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功停用『{folder.Muid}』帳號";
            }
            else if (folder.Mstatus == 0)
            {
                folder.Mstatus = 1;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功啟用『{folder.Muid}』帳號";
            }

            return RedirectToAction("MemberList");
        }

        //使用者按下 按下_AdminLayout的Nav編輯個人資料時啟用
        public IActionResult GoMemberEdit()
        {
            string Sid = User.FindFirst(ClaimTypes.Sid)!.Value;

            return RedirectToAction("MemberEdit", new { Uid = Sid });
        }

        //修改會員資料畫面
        public IActionResult MemberEdit(string Uid)
        {
            var memberEdit = _dbContext.Members.FirstOrDefault(m => m.Muid == Uid);

            if (memberEdit == null)
            {
                TempData["Error"] = "會員資料編輯失敗，請確認會員資料正確性!";
                return RedirectToAction("MemberList");
            }

            return View(memberEdit);
        }

        //修改會員資料方法
        [HttpPost]
        public IActionResult MemberEdit(Member member)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uid = member.Muid;
                    var memberTemp = _dbContext.Members.FirstOrDefault(m => m.Muid == uid)!;

                    memberTemp.Mpwd = member.Mpwd;
                    memberTemp.Mname = member.Mname;
                    memberTemp.Mmail = member.Mmail;
                    memberTemp.Mrole = member.Mrole;

                    _dbContext.SaveChanges();
                    TempData["Success"] = $"會員資料修改成功,請重新登入確認資料變更。";
                    return RedirectToAction("MemberList");
                }
                catch (Exception)
                {
                    TempData["Error"] = "會員修改失敗";
                }
            }

            return View(member);
        }

        //使用者按下 按下_AdminLayout的Nav我的文章時啟用
        public IActionResult GoMemberAllFold()
        {
            string Sid = User.FindFirst(ClaimTypes.Sid)!.Value;

            return RedirectToAction("MemberAllFold", new { Uid = Sid });
        }

        //觀看某會員的所有文章
        public IActionResult MemberAllFold(string? Uid, string? SearchFolder)
        {
            string Folder = SearchFolder != null ? SearchFolder : "";

            var FoldItem = _dbContext.Folders
                .Where(m => m.FcreateUser == Uid && m.Ftitle!.Contains(Folder))
                .OrderByDescending(m => m.FcreateTime);

            var MemberItem = _dbContext.Members.FirstOrDefault(m => m.Muid == Uid);
            ViewData["Name"] = MemberItem?.Mname ?? "";
            ViewData["MUid"] = MemberItem?.Muid ?? "";

            if (MemberItem == null)
            {
                TempData["Error"] = "查無此會員";
                return RedirectToAction("MemberList");
            }

            return View(FoldItem);
        }

        //觀看某會員的所有文章_修改資料顯示狀態
        public IActionResult MemberAllFolderStatus(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid)!;

            if (folder.Fstatus == 1)
            {
                folder.Fstatus = 0;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功關閉『{folder.Ftitle}』資料";
            }
            else if (folder.Fstatus == 0)
            {
                folder.Fstatus = 1;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功開啟『{folder.Ftitle}』資料";
            }

            return RedirectToAction("MemberAllFold", new { Uid = folder.FcreateUser });
        }

        //觀看某會員的所有文章_編輯資料畫面
        public IActionResult MemberAllFolderEdit(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid);

            if (folder == null)
            {
                TempData["Error"] = "文章編輯失敗，請確認資料正確性!";
                return RedirectToAction("MemberList");
            }

            return View(folder);
        }

        //觀看某會員的所有文章_編輯資料方法
        [HttpPost]
        public async Task<IActionResult> MemberAllFolderEdit(Folder folder, List<IFormFile>? TitleFormfiles)   //IFormFile一定要加上?可以接受Null不然ModelState.IsValid不會過 吃了大虧...
        {
            string Errormsg;

            if (ModelState.IsValid)
            {
                try
                {
                    string? folderId = folder.FfolderId;
                    var folderTemp = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == folderId)!;
                    int TitleNum = 0;

                    /*如果使用者更改類別才觸發*/
                    if (folderTemp.Fcid != folder.Fcid)
                    {
                        string OldMenu, OldPath, NewMenu, NewPath;

                        //找出舊資料
                        var folderPictureTemp = _dbContext.FolderPictures
                            .Where(m => m.Pfid == folderTemp.Fpicture)
                            .ToList();

                        foreach (var picture in folderPictureTemp)
                        {
                            TitleNum++;
                            OldMenu = $@"{_Path}\{folderTemp.Fcid}";
                            NewMenu = $@"{_Path}\{folder.Fcid}";

                            /*如果資料夾不存在建立，避免後面引發錯誤*/
                            if (!DirectoryIsExists(OldMenu))
                            {
                                AddDirectory(DirectoryIsExists(OldMenu), OldMenu);
                            }

                            if (!DirectoryIsExists(NewMenu))
                            {
                                AddDirectory(DirectoryIsExists(NewMenu), NewMenu);
                            }

                            /*找出舊資料*/
                            OldPath = $@"{_Path}\{folderTemp.Fcid}\{picture.Ppicture}";

                            /*確認新舊資料夾都存在後，判斷檔案在不在由搬移的Function去做處理*/
                            picture.Ppicture = $"Title_{folder.Fcid}_{folder.FfolderId}_{TitleNum}_{new Random().Next(999)}.jpg";
                            NewPath = $@"{_Path}\{folder.Fcid}\{picture.Ppicture}";
                            ChangePicturesByMenu(OldPath, NewPath);

                            _dbContext.SaveChanges();
                        }
                    }

                    folderTemp.Fcid = folder.Fcid!;
                    folderTemp.Ftitle = folder.Ftitle;
                    folderTemp.Fdescription = folder.Fdescription;
                    folderTemp.FeditTime = DateTime.Now;
                    folderTemp.FeditUser = User.FindFirst(ClaimTypes.Sid)!.Value;

                    _dbContext.SaveChanges();

                    /*如果TitleFormfiles有值代表使用者修改圖片*/
                    if (TitleFormfiles != null && TitleFormfiles.Count > 0)
                    {
                        var pictureItem = _dbContext.FolderPictures.Where(m => m.Pfid == folderTemp.Fpicture);
                        _dbContext.RemoveRange(pictureItem);    //刪除原本的圖片資料

                        await CreatePicture(folder, TitleFormfiles);
                    }

                    TempData["Success"] = "資料修改成功";
                    return RedirectToAction("MemberAllFold", new { Uid = folderTemp.FcreateUser });
                }
                catch (Exception)
                {
                    Errormsg = "可能是客戶編號重複";
                }
            }
            else
            {
                Errormsg = "模型驗證失敗";
                //string Errorstr = "";
                //foreach (var entry in ModelState)
                //{
                //    foreach (var error in entry.Value.Errors)
                //    {
                //        Errorstr = $"{entry.Key}: {error.ErrorMessage}";
                //    }
                //}
                //Errormsg = $"{Errorstr}";
            }

            TempData["Error"] = $"資料上傳失敗, {Errormsg}";
            return View();
        }

        //觀看某會員的所有文章_幻燈片圖片順序編輯頁面
        public IActionResult MemberAllPictureRow(string Fpicture)
        {
            var fold = _dbContext.Folders.FirstOrDefault(m => m.Fpicture == Fpicture);

            if (fold == null)
            {
                TempData["Error"] = "跑馬燈順序編輯失敗，請確認資料正確性!";
                return RedirectToAction("MemberList");
            }

            ViewBag.FoldTitle = fold?.Ftitle;
            ViewBag.FoldCid = fold?.Fcid;
            ViewBag.PictureFid = _dbContext.FolderPictures.FirstOrDefault(m => m.Pfid == Fpicture)?.Pfid;

            var Picture = _dbContext.FolderPictures.Where(m => m.Pfid == Fpicture).OrderBy(m => m.Prow).ToList();

            return View(Picture);
        }

        //觀看某會員的所有文章_幻燈片圖片順序編輯方法
        [HttpPost]
        public IActionResult MemberAllPictureRow(List<FolderPicture> folderPicture)
        {
            try
            {
                var FolderItem = _dbContext.Folders.FirstOrDefault(m => m.Fpicture == folderPicture[0].Pfid)!;

                foreach (var item in folderPicture)
                {
                    var folderpicture = _dbContext.FolderPictures.FirstOrDefault(m => m.Pid == item.Pid)!;
                    folderpicture.Prow = item.Prow;
                }

                FolderItem.FeditTime = DateTime.Now;
                FolderItem.FeditUser = User.FindFirst(ClaimTypes.Sid)!.Value;

                _dbContext.SaveChanges();
                TempData["Success"] = "圖片順序修改成功";

                return RedirectToAction("MemberAllFold", new { Uid = FolderItem!.FcreateUser });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"圖片順序修改失敗,{ex.Message}";
            }
            return View();
        }

        //觀看某會員的所有文章_刪除資料
        public IActionResult MemberAllFolderDelete(string Fid)
        {
            var folder = _dbContext.Folders.FirstOrDefault(m => m.FfolderId == Fid)!;
            var picture = _dbContext.FolderPictures.Where(m => m.Pfid == folder.Fpicture);

            DeletePicture(folder.Fcid, folder.Fpicture);

            _dbContext.Folders.Remove(folder);
            _dbContext.FolderPictures.RemoveRange(picture);
            _dbContext.SaveChanges();
            TempData["Success"] = "資料刪除成功";
            return RedirectToAction("MemberAllFold", new { Uid = folder.FcreateUser });
        }

        //刪除會員方法
        public IActionResult MemberDelete(string Uid)
        {
            try
            {
                var MemberItem = _dbContext.Members.FirstOrDefault(m => m.Muid == Uid)!;

                _dbContext.Members.Remove(MemberItem);
                _dbContext.SaveChanges();

                TempData["Success"] = "會員刪除成功";
                return RedirectToAction("MemberList");
            }
            catch (Exception)
            {
                TempData["Error"] = "會員刪除失敗";
            }

            return View();
        }

        //新增會員頁面
        public IActionResult MemberCreate()
        {
            return View();
        }

        //新增會員方法
        [HttpPost]
        public IActionResult MemberCreate(Member member)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    member.Mstatus = 1;
                    _dbContext.Members.Add(member);
                    _dbContext.SaveChanges();

                    TempData["Success"] = "新用戶建立成功";
                    return RedirectToAction("MemberList");
                }
                catch (Exception)
                {
                    TempData["Success"] = "新用戶建立失敗, 也許是帳號重複";
                }
            }

            return View();
        }

        //新增資料頁面
        public IActionResult FolderUpload()
        {
            return View();
        }

        //新增資料方法
        [HttpPost]
        public async Task<IActionResult> FolderUpload(Folder folder, List<IFormFile> TitleFormfiles)
        {
            string Errormsg = "";        //錯誤訊息

            if (ModelState.IsValid)
            {
                if (TitleFormfiles != null && TitleFormfiles.Count > 0)
                {
                    try
                    {
                        /*存入檔案名稱以及建立時間*/
                        folder.Fpicture = $"{folder.FfolderId}_pid";
                        folder.FcreateUser = User.FindFirst(ClaimTypes.Sid)!.Value; // 紀錄創建者ID
                        folder.FcreateTime = DateTime.Now;
                        folder.FeditUser = User.FindFirst(ClaimTypes.Sid)!.Value;   // 紀錄創建者ID
                        folder.FeditTime = DateTime.Now;

                        _dbContext.Folders.Add(folder);
                        _dbContext.SaveChanges();

                        /*將圖檔傳至pictures裡*/
                        await CreatePicture(folder, TitleFormfiles);
                        TempData["Success"] = "資料上傳成功";
                        return RedirectToAction("FolderClassify", new { Cid = folder.Fcid });

                    }
                    catch (Exception)
                    {
                        //DeletePicture(folder.Fcid,folder.Fpicture);   //建立失敗就刪掉圖檔
                        Errormsg = "可能是客戶編號重複";
                    }
                }
                else
                {
                    Errormsg = "圖片欄位不得為空";
                }
            }
            else
            {
                Errormsg = "資料驗證失敗";
            }

            TempData["Error"] = $"資料上傳失敗, {Errormsg}";
            return View();
        }

        /*顯示廣告列表*/
        public IActionResult AdvertiseItem(int? SearchStatus, string? SearchFolder)
        {
            int? Status = SearchStatus;
            string Folder = SearchFolder != null ? SearchFolder : "";

            var advertise = (from AdvertiseItem in _dbContext.Advertises
                             join FolderItem in _dbContext.Folders on AdvertiseItem.AfolderId equals FolderItem.FfolderId into AdvertiseFolderItem
                             from AdvertiseFolder in AdvertiseFolderItem.DefaultIfEmpty()
                             where (AdvertiseItem.AfolderId!.Contains(Folder) || AdvertiseFolder.Ftitle!.Contains(Folder)) && (Status == null || AdvertiseItem.Astatus == Status)
                             orderby AdvertiseItem.Aid
                             select AdvertiseItem
                            ).ToList();

            return View(advertise);
        }

        /* 改變廣告狀態 顯示|隱藏 */
        public IActionResult AdvertiseStatus(int Aid)
        {
            var advertise = _dbContext.Advertises.FirstOrDefault(m => m.Aid == Aid)!;

            if (advertise.Astatus == 1)
            {
                advertise.Astatus = 0;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功關閉廣告";
            }
            else if (advertise.Astatus == 0)
            {
                advertise.Astatus = 1;
                _dbContext.SaveChanges();
                TempData["Success"] = $"已成功開啟廣告";
            }

            return RedirectToAction("AdvertiseItem");
        }

        /*編輯廣告頁面*/
        public IActionResult AdvertiseEdit(int Aid)
        {
            var advertise = _dbContext.Advertises.FirstOrDefault(m => m.Aid == Aid);

            /*預防使用者找不到資料後發生死當*/
            if (advertise == null)
            {
                TempData["Error"] = $"查無此廣告資料，請確認資料正確性!";
                return RedirectToAction("AdvertiseItem");
            }

            return View(advertise);
        }

        /*編輯廣告功能*/
        [HttpPost]
        public async Task<IActionResult> AdvertiseEdit(Advertise advertise, IFormFile? AdFormfilesSm, IFormFile? AdFormfilesMb)
        {
            string Errormsg = "";   //錯誤訊息

            try
            {
                var AdvertiseTemp = _dbContext.Advertises.FirstOrDefault(A => A.Aid == advertise.Aid)!;
                var FoldTemp = _dbContext.Folders.FirstOrDefault(F => F.FfolderId == advertise.AfolderId)!;

                /*取代舊資料*/
                AdvertiseTemp.Arow = advertise.Arow;
                _dbContext.SaveChanges();


                /*如果沒有修改圖片就不執行了*/
                if ((AdFormfilesSm != null && AdFormfilesSm.Length > 0) || (AdFormfilesMb != null && AdFormfilesMb.Length > 0))
                {
                    FolderPicture? pictureItem;
                    /*Sm 有值代表誰是要被更改的 找到舊資料後先刪除再重新建置新的*/
                    if (AdFormfilesSm != null && AdFormfilesSm.Length > 0)
                    {
                        pictureItem = _dbContext.FolderPictures.FirstOrDefault(FP => FP.Pfid == AdvertiseTemp.Apictures && FP.Ppicture!.Contains("_sm"));
                        if (pictureItem != null) //有可能原本就沒有
                        {
                            DeleteAdPicture(pictureItem.Pfid, "_sm");
                        }
                    }
                    /*Mb 有值代表誰是要被更改的 找到舊資料後先刪除再重新建置新的*/
                    if (AdFormfilesMb != null && AdFormfilesMb.Length > 0)
                    {
                        pictureItem = _dbContext.FolderPictures.FirstOrDefault(FP => FP.Pfid == AdvertiseTemp.Apictures && FP.Ppicture!.Contains("_mb"));
                        if (pictureItem != null) //有可能原本就沒有
                        {
                            DeleteAdPicture(pictureItem.Pfid, "_mb");
                        }
                    }

                    /*將圖檔傳至pictures裡*/
                    await CreatePictureAd(AdvertiseTemp, AdFormfilesSm, AdFormfilesMb);
                }


                TempData["Success"] = "廣告編輯成功";
                return RedirectToAction("AdvertiseItem");
            }
            catch (Exception ex)
            {
                Errormsg = $"修改資料至DB時發現例外狀況, {ex.Message}";
            }

            TempData["Error"] = $"修改廣告發生意外的情況, {Errormsg}";
            return View();
        }

        /*刪除廣告方法*/
        public IActionResult AdvertiseDelete(int Aid)
        {
            try
            {
                var Advertise = _dbContext.Advertises.FirstOrDefault(a => a.Aid == Aid);

                if (Advertise != null)
                {
                    DeleteAdPicture(Advertise.Apictures);
                    _dbContext.Advertises.Remove(Advertise);
                    _dbContext.SaveChanges();
                }

                TempData["Success"] = "廣告刪除成功";
                return RedirectToAction("AdvertiseItem");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"廣告刪除失敗, {ex.Message}";
            }
            return View();
        }

        /*新增廣告頁面*/
        public IActionResult Advertise()
        {
            return View();
        }

        /*新增廣告方法*/
        [HttpPost]
        public async Task<IActionResult> Advertise(Advertise advertise, IFormFile? AdFormfilesSm, IFormFile? AdFormfilesMb)
        {
            string Errormsg = "";        //錯誤訊息

            if (ModelState.IsValid)
            {
                if (AdFormfilesSm != null && AdFormfilesMb != null && AdFormfilesSm.Length > 0 && AdFormfilesMb.Length > 0)
                {
                    try
                    {
                        var FoldTemp = _dbContext.Folders.FirstOrDefault(F => F.FfolderId == advertise.AfolderId);

                        advertise.Acid = FoldTemp?.Fcid ?? null;
                        advertise.Apictures = $"{advertise.AfolderId}_pid_Ad_{new Random().Next(999)}";
                        advertise.Astatus = 1;

                        _dbContext.Advertises.Add(advertise);
                        _dbContext.SaveChanges();

                        /*將圖檔傳至pictures裡*/
                        await CreatePictureAd(advertise, AdFormfilesSm, AdFormfilesMb);
                        TempData["Success"] = "廣告新增成功";
                        return RedirectToAction("AdvertiseItem");
                    }
                    catch (Exception ex)
                    {
                        Errormsg = $"新增資料至DB時發現例外狀況, {ex}";
                    }
                }
                else
                {
                    Errormsg = "圖片欄位不得為空";
                }
            }
            else
            {
                Errormsg = "資料驗證失敗";
            }
            TempData["Error"] = $"新增廣告發生意外的情況, {Errormsg}";
            return View();
        }

        /*新增廣告圖檔*/
        public async Task CreatePictureAd(Advertise advertise, IFormFile? AdFormfilesSm, IFormFile? AdFormfilesMb)
        {
            try
            {
                string savePath = $@"{_Path}\advertisePictures";
                string fileNameSm, fileNameMb;
                IList<FolderPicture> folderpicture = new List<FolderPicture>();

                if (!DirectoryIsExists(savePath))
                {
                    AddDirectory(false, savePath);
                }

                /*處理Sm的圖*/
                if (AdFormfilesMb != null && AdFormfilesMb.Length > 0)
                {
                    fileNameMb = $"Advertise_{advertise.Acid}_{advertise.AfolderId}_mb_{new Random().Next(999)}.jpg";

                    using (var steam = System.IO.File.Create($@"{savePath}\{fileNameMb}"))
                    {
                        await AdFormfilesMb!.CopyToAsync(steam);
                    }

                    folderpicture.Add(new FolderPicture { Pfid = advertise.Apictures!, PcontentClassify = "Advertise", Ppicture = fileNameMb, Prow = 1 });
                }

                /*處理Mb的圖*/
                if (AdFormfilesSm != null && AdFormfilesSm.Length > 0)
                {
                    fileNameSm = $"Advertise_{advertise.Acid}_{advertise.AfolderId}_sm_{new Random().Next(999)}.jpg";

                    using (var steam = System.IO.File.Create($@"{savePath}\{fileNameSm}"))
                    {
                        await AdFormfilesSm!.CopyToAsync(steam);
                    }

                    folderpicture.Add(new FolderPicture { Pfid = advertise.Apictures!, PcontentClassify = "Advertise", Ppicture = fileNameSm, Prow = 1 });
                }

                /*存到DB*/
                foreach (var item in folderpicture)
                {
                    _dbContext.FolderPictures.Add(item);
                }

                _dbContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*主機新增圖檔*/
        public async Task CreatePicture(Folder folder, List<IFormFile> TitleFormfiles)
        {
            try
            {
                string savePath = $@"{_Path}\{folder.Fcid}";   //路徑
                int TitleNum = 0;                              //圖片流水編號用
                string fileName;                               //檔案名稱

                /*如果資料夾不存在幫我建立*/
                if (!DirectoryIsExists(savePath))
                {
                    AddDirectory(false, savePath);
                }

                /*將圖片上傳並將資料寫進DB中*/
                foreach (var files in TitleFormfiles)
                {
                    var picturesItem = new FolderPicture();
                    TitleNum++;
                    fileName = $"Title_{folder.Fcid}_{folder.FfolderId}_{TitleNum}_{new Random().Next(999)}.jpg"; /*為了避免因為圖片名稱相同修改結果被吃掉給隨機數(不要用Guid檔案名稱太長)*/

                    using (var steam = System.IO.File.Create($@"{savePath}\{fileName}"))
                    {
                        await files.CopyToAsync(steam);
                    }

                    picturesItem.Pfid = $"{folder.FfolderId}_pid";
                    picturesItem.PcontentClassify = "Title";
                    picturesItem.Ppicture = fileName;
                    picturesItem.Prow = TitleNum;
                    _dbContext.FolderPictures.Add(picturesItem);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /*主機刪除廣告圖檔*/
        public void DeleteAdPicture(string? Pid, string? type = null)
        {
            try
            {
                string savePath = $@"{_Path}\advertisePictures";   //路徑

                /*如果type是Null代表不指定Mb、Sm全部刪除*/
                if (string.IsNullOrEmpty(type))
                {
                    var DeleteItems = _dbContext.FolderPictures.Where(m => m.Pfid == Pid);

                    foreach (var item in DeleteItems)
                    {
                        System.IO.File.Delete($@"{savePath}\{item.Ppicture}");
                    }

                    _dbContext.FolderPictures.RemoveRange(DeleteItems);
                }
                else
                {
                    var DeleteItem = _dbContext.FolderPictures.FirstOrDefault(m => m.Pfid == Pid && m.Ppicture!.Contains(type));

                    if (DeleteItem != null)
                    {
                        System.IO.File.Delete($@"{savePath}\{DeleteItem.Ppicture}");
                        _dbContext.FolderPictures.Remove(DeleteItem);
                    }
                }

                _dbContext.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        //依照分類顯示用戶畫面(非表格方式呈現)
        public IActionResult FolderClassifyIndex(int? Cid = null, string? SearchFolder = null)
        {
            string Search = (SearchFolder != null ? SearchFolder : "");
            /*查詢的時候需要類別代號*/
            ViewData["ClassifyKey"] = _dbContext.Classifies
                .Where(m => m.Cstatus == 1)
                .FirstOrDefault(m => m.Cid == Cid)?
                .Cid.ToString() ?? "查無資料";

            ViewBag.Classify = _dbContext.Classifies
                .FirstOrDefault(m => m.Cid == Cid)?
                .Cname ?? "查無資料";

            var folders = _dbContext.Folders
                .Where(m => m.Fcid == Cid && m.Fstatus == 1 && m.Ftitle!.Contains(Search))
                .OrderByDescending(m => m.FeditTime)
                .ToList();

            return View(folders);
        }

        /*主機刪除圖檔*/
        public void DeletePicture(int? Cid, string? Pid)
        {
            try
            {
                var DeleteItem = _dbContext.FolderPictures.Where(m => m.Pfid == Pid);
                string savePath = $@"{_Path}\{Cid}";   //路徑

                foreach (var item in DeleteItem)
                {
                    System.IO.File.Delete($@"{savePath}\{item.Ppicture}");
                }

                _dbContext.FolderPictures.RemoveRange(DeleteItem);
                _dbContext.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        /*不同類別存放圖的資料夾是否存在*/
        public bool DirectoryIsExists(string Path)
        {
            bool fileExists = false;

            try
            {
                if (Directory.Exists(Path))
                {
                    fileExists = true;
                }
            }
            catch
            {
                throw;
            }

            return fileExists;
        }

        /*檔案是否存在*/
        public bool FileIsEsists(string Path)
        {
            bool fileExists = false;

            try
            {
                if (System.IO.File.Exists(Path))
                {
                    fileExists = true;
                }
            }
            catch
            {
                throw;
            }

            return fileExists;
        }

        /*新增資料夾*/
        public void AddDirectory(bool fileExists, string Path)
        {
            try
            {
                if (!fileExists)
                {
                    Directory.CreateDirectory(Path);
                }
            }
            catch
            {
                throw;
            }
        }

        /*刪除資料夾*/
        public void DeleteDirectory(bool fileExists, string Path)
        {
            try
            {
                if (fileExists)
                {
                    DirectoryInfo dir = new DirectoryInfo(Path);
                    FileInfo[] files = dir.GetFiles();

                    /*找出這個資料夾底下的所有資料並刪除*/
                    foreach (FileInfo filePath in files)
                    {
                        filePath.Delete();
                    }

                    dir.Delete(true);
                    //加了true後,就算資料夾裡面有東西也一樣會全刪掉。有時候會報錯誤先用迴圈刪一次巴
                    //最後發現權限錯誤問題是Google雲端備份造成的...
                }
            }
            catch (UnauthorizedAccessException)
            {
                //處理權限不足的情況。 這個問題不一定會觸發到機率型的 不一定是真的權限不夠... 同步Google雲端備份時有時候會報這個錯誤。
                if (Directory.Exists(Path))
                {
                    DeleteDirectory(fileExists, Path);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                throw;
            }
        }

        /*修改資料夾名稱*/
        //public void RenameDirectory(bool fileExists, string OldPath, string NewPath)
        //{
        //    try
        //    {
        //        if (fileExists)
        //        {
        //            Directory.Move(OldPath, NewPath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["Error"] = $"變更資料夾名稱的時候發生了什麼錯誤,{ex}";
        //    }
        //}

        /*搬移圖檔*/
        public void ChangePicturesByMenu(string OldPath, string NewPath)
        {
            try
            {
                /*確定原本的位置上有這個圖檔*/
                var OldIsExists = FileIsEsists(OldPath);

                /*如果存在幫我搬移他並使用新的名稱*/
                if (OldIsExists)
                {
                    Directory.Move(OldPath, NewPath);
                }

            }
            catch
            {
                throw;
            }
        }
    }
}
