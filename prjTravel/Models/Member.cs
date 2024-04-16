using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjTravel.Models;

public partial class Member
{
    [Display(Name = "帳號")]
    [Required(ErrorMessage = "必填")]
    [Remote(action: "CheckMuid", controller: "Home", ErrorMessage = "帳號已存在不可重複")]
    public string Muid { get; set; } = null!;
    [Display(Name = "密碼")]
    [Required(ErrorMessage = "密碼為必填")]
    public string? Mpwd { get; set; }
    [Display(Name = "姓名")]
    [Required(ErrorMessage = "姓名為必填")]
    public string? Mname { get; set; }
    [Display(Name = "信箱")]
    [Required(ErrorMessage = "信箱為必填")]
    [EmailAddress(ErrorMessage = "必須符合信箱格式")]
    public string? Mmail { get; set; }
    [Display(Name = "角色")]
    public string? Mrole { get; set; }
    [Display(Name = "成員狀態")]
    public int? Mstatus { get; set; }
}
