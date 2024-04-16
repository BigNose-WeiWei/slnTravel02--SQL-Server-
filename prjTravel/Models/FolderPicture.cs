using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjTravel.Models;

public partial class FolderPicture
{
    public int Pid { get; set; }

    public string Pfid { get; set; } = null!;

    public string? PcontentClassify { get; set; }
    [Display(Name = "圖片")]
    public string? Ppicture { get; set; }
    [Display(Name = "排序")]
    public int? Prow { get; set; }
}
