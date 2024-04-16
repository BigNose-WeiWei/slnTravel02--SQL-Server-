using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace prjTravel.Models;

public partial class Advertise
{
    public int Aid { get; set; }

    public int? Acid { get; set; }
    [Display(Name = "客戶編號")]
    public string? AfolderId { get; set; }
    [Display(Name = "廣告圖示")]
    public string? Apictures { get; set; }
    [Display(Name = "狀態")]
    public int? Astatus { get; set; }
    [Display(Name = "廣告順序")]
    [Range(0, 99, ErrorMessage = "數值必須介於0至99")]
    [RegularExpression("^[0-9]+$", ErrorMessage = "請輸入有效的數值")]
    public int? Arow { get; set; }
}
