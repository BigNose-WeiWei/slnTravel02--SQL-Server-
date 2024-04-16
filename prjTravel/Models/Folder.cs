using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjTravel.Models;

public partial class Folder
{
    [Display(Name = "編號")]
    [Required(ErrorMessage = "編號為必填")]
    [Remote(action: "CheckFfolderId", controller: "Home", ErrorMessage = "編號已存在不可重複")]
    public string? FfolderId { get; set; }
    [Display(Name = "類別名稱")]
    [Required(ErrorMessage = "類別為必填")]
    public int? Fcid { get; set; }
    [Display(Name = "主題名稱")]
    [Required(ErrorMessage = "主題名稱為必填")]
    public string? Ftitle { get; set; }
    [Display(Name = "描述說明")]
    [Required(ErrorMessage = "描述說明為必填")]
    public string? Fdescription { get; set; }
    [Display(Name = "圖檔")]
    public string? Fpicture { get; set; }
    [Display(Name = "發佈帳號")]
    //[Required(ErrorMessage = "必填")]
    public string? FcreateUser { get; set; }
    [Display(Name = "發佈日期")]
    //[Required(ErrorMessage = "必填")]
    public DateTime? FcreateTime { get; set; }
    [Display(Name = "編輯帳號")]
    public string? FeditUser { get; set; }
    [Display(Name = "編輯日期")]
    public DateTime? FeditTime { get; set; }
    [Display(Name = "資料狀態")]
    public int? Fstatus { get; set; }
}
