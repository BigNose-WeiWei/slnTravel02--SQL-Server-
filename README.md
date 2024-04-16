使用SQL Server作為資料庫
Program.cs與appsettings.json的寫法跟SQL Express不太一樣，因為要使用IIS，所以不可以用Windows認證，要用SQL Server驗證方式。


# 網頁功能介紹
https://drive.google.com/file/d/10PK5DchI5r23K-sk3c1Nq2Hs9Ewvnpig/view?usp=drive_link  

# 後端  
*  ASP.NET Core MVC框架
*  帳密授權驗證
*  使用者可建立、閱讀、修改、刪除資料庫的資料
*  限制使用者上傳檔案格式，以及檔案數量

# 前端
* Bootstrap框架
* 畫面採RWD設計

## Node.js
因為有使用SCSS覆寫Bootstrap的優先權需要安裝Node.js  
https://nodejs.org/en

## 資料庫  
在prjTravel底下新增App_Data資料夾，放入dbProject.mdf即可  
20240123 - https://drive.google.com/drive/folders/1vJyFrqc4kO1RbMRqAMxVkzpFAV8OQuVT?usp=drive_link

Member 使用者帳戶
|Key| 名稱      | 資料類型            | 允許Null    | 預設  |    備註                |
|:-:|:----------|:-------------------|:----------- |:----:|:----------------------|
|*  |	MUid	|    nvarchar(50)    |    False    |      |    帳號                |
|   |	MPwd	|    nvarchar(50)    |    True     |      |    密碼                |
|   |	MName	|    nvarchar(50)    |    True     |      |    名稱                |
|   |	MMail	|    nvarchar(50)    |    True     |	  |    信箱                |
|   |	MRole	|    nvarchar(50)    |    True     |      |    角色 (串Role的Rid)  |
|   |	MStatus	|    int             |    True     | (1)  |    是否啟用 1、0       | 

Role 角色
|Key| 名稱      | 資料類型            | 允許Null    | 預設  |    備註                |
|:-:|:----------|:-------------------|:----------- |:----:|:----------------------|
|*  |	RId	    |    nvarchar(50)    |    False    |      |    編號                |
|   |	RName	|    nvarchar(50)    |    True     |      |    名稱                |
|   |	RStatus	|    int             |    True     | (1)  |    是否啟用 1、0       | 

Classify 分類
|Key| 名稱      | 資料類型            | 允許Null    | 預設  |    備註               |
|:-:|:----------|:-------------------|:----------- |:----:|:----------------------|
|*  |	Cid	    |    int             |    False    |      |    編號                |
|   |	CName	|    nvarchar(50)    |    True     |      |    分類別稱            |
|   |	CStatus	|    int             |    True     | (1)  |    是否啟用 1、0       | 

Folder 文章資料
|Key| 名稱          | 資料類型            | 允許Null    | 預設  |    備註                          |
|:-:|:--------------|:-------------------|:----------- |:----:|:---------------------------------|
|*  |	FFolderId	|    nvarchar(50)    |    False    |      |    產品ID                        |
|   |	FCid	    |    int             |    True     |      |    產品分類 (串Classify的Cid)       |
|   |	FTitle	    |    nvarchar(50)    |    True     |      |    標題                          |
|   |	FDescription|    nvarchar(MAX)   |    True     |	  |    內容                          |
|   |	FPicture	|    nvarchar(50)    |    True     |      |    圖片Id (串FolderPicture的PFid)  |
|   |	FCreateUser	|    nvarchar(50)    |    True     |      |    創建帳號 (串Member的Muid       | 
|   |	FCreateTime	|    datetime        |    True     |      |    創建時間                      | 
|   |	FEditUser	|    nvarchar(50)    |    True     |      |    最後修改帳號 串Member的Muid    | 
|   |	FEditTime	|    datetime        |    True     |      |    最後修改時間                   | 
|   |	FStatus	    |    int             |    True     | (1)  |    是否啟用 1、0                  | 
    
Advertise 廣告資料
|Key| 名稱          | 資料類型            | 允許Null    | 預設  |    備註                            |
|:-:|:--------------|:-------------------|:----------- |:----:|:-----------------------------------|
|*  |	AId	        |    int             |    False    |      |    文章編號                        |
|   |	ACid	    |    int             |    True     |      |    對應分類                        |
|   |	AFolderId	|    nvarchar(50)    |    True     |      |    對應的文章                      |
|   |	APicture	|    nvarchar(50)    |    True     |	  |    圖片Id (串FolderPicture的PFid)  |
|   |	AStatus	    |    int             |    True     | (1)  |    是否啟用 1、0                   |
|   |	ARow	    |    int             |    True     |	  |    廣告順序                        | 

FolderPicture 圖片
|Key| 名稱                  | 資料類型            | 允許Null    | 預設  |    備註                                |
|:-:|:----------------------|:-------------------|:----------- |:----:|:----------------------                |
|*  |	Pid	                |    int             |    False    |      |    編號                               |
|   |	PFid	            |    nvarchar(50)    |    True     |      |    圖片Id                             |
|   |	PContentClassify	|    nvarchar(50)    |    True     |      |    Title文章跑馬燈、Advertise廣告      |
|   |	PPicture	        |    nvarchar(50)    |    True     |	  |    圖片在主機的檔案名稱                |
|   |	PRow	            |    int             |    True     |      |    排序                               |

## 角色權限
Admin、Manager、User、訪客。   

Admin

    最高權限。  
    
Manager  

    除了不可修改Admin的基本資料外其餘跟Admin相同。

User  

    只可上傳檔案，並且只能編輯自己創建的文章。  

訪客

    只可觀看。
