using System.Web.Services;

namespace Webservice
{
    /// <summary>
    /// Service 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // [System.Web.Script.Services.ScriptService]
    public class Service : WebService
    {
        DBOperation dbOperation = new DBOperation();

        [WebMethod(Description = "计算(List<string>)")]
        public string Funny(string rmb)
        {
            return dbOperation.Calculator(rmb);
        }
        [WebMethod(Description = "检查用户账户及密码(bool)")]
        public bool CheckOne(string UserName, string PassWord)
        {
            return dbOperation.SelectOne(UserName, PassWord);
        }
        [WebMethod(Description = "增加一个账户(bool)")]
        public string InsertUser(string UserName, string PassWord, string NickName, string UserType)
        {
            return dbOperation.AddUser(UserName, PassWord, NickName, UserType);
        }
        [WebMethod(Description = "删除一个账户(bool)")]
        public bool DeleteUser(string UserName)
        {
            return dbOperation.DeleteUser(UserName);
        }
        [WebMethod(Description = "获取用户信息(string)")]
        public string UserInfo(string UserID)
        {
            return dbOperation.GetInfo(UserID);
        }
        [WebMethod(Description = "增加一个项目(bool)")]
        public string InsertProject(string UserName, string ProjectName, string ProjectContent, string ProjectTeacher, string NeedPeople, string ProjectType)
        {
            return dbOperation.AddProject(UserName, ProjectName, ProjectContent, ProjectTeacher, NeedPeople, ProjectType);
        }
        [WebMethod(Description = "项目列表(string)")]
        public string ReturnProjectList(string page, string type)
        {
            return dbOperation.ProjectList(page, type);
        }
        [WebMethod(Description = "项目申请详情(string)")]
        public string ReturnProjectDetail(string ProjectID, string UserID)
        {
            return dbOperation.ProjectDetail(ProjectID, UserID);
        }
        [WebMethod(Description = "项目当前详情(string)")]
        public string ReturnNowProjectDetail(string ProjectID, string UserID)
        {
            return dbOperation.NowProjectDetail(ProjectID, UserID);
        }
        [WebMethod(Description = "申请加入(bool)")]
        public bool UpdateApplyJoin(string UserID, string ProjectID, string Duty)
        {
            return dbOperation.ApplyJoinProject(UserID, ProjectID, Duty);
        }
        [WebMethod(Description = "我的项目列表(string)")]
        public string ReturnMeProjectList(string UserID, string Who)
        {
            return dbOperation.MeProjectList(UserID, Who);
        }
        [WebMethod(Description = "收藏的项目列表(string)")]
        public string ReturnMeLikeProjectList(string UserID, string State)
        {
            return dbOperation.MeLikeProjectList(UserID, State);
        }
        [WebMethod(Description = "改变收藏状态(string)")]
        public string ChangeProjectFavorite(string UserID, string ProjectID, string Checked)
        {
            return dbOperation.ChangeProjectFavorite(UserID, ProjectID, Checked);
        }
        [WebMethod(Description = "检查收藏状态(bool)")]
        public bool CheckProjectFavorite(string UserID, string ProjectID)
        {
            return dbOperation.ProjectState(UserID, ProjectID);
        }
        [WebMethod(Description = "返回待处理的项目申请(string)")]
        public string ReturnProjectMessageList(string UserID)
        {
            return dbOperation.ProjectMessageList(UserID);
        }
        [WebMethod(Description = "处理项目申请(string)")]
        public string UpdateProjectApply(string UserID, string ProjectID, string Intent, string Duty, string ProjectName)
        {
            return dbOperation.ChangeProjectApply(UserID, ProjectID, Intent, Duty, ProjectName);
        }
        [WebMethod(Description = "上传头像(bool)")]
        public string UpFaceImage(string UserID, string face)
        {
            return dbOperation.UpLoadImage(UserID, face);
        }
        [WebMethod(Description = "下载头像(Base64)")]
        public string DownFaceImage(string UserID)
        {
            return dbOperation.DownloadFaceImage(UserID);
        }
        [WebMethod(Description = "批量下载头像(Base64List)")]
        public string DownFaceImageList(string UserIDList)
        {
            return dbOperation.DownloadFaceImageList(UserIDList);
        }
        [WebMethod(Description = "上传资料(bool)")]
        public string DataUpload(string UserID, string DataName, string DownloadUrl, string DataBrief, string DataType, string DocName, string DocBase64)
        {
            return dbOperation.DataUpload(UserID, DataName, DownloadUrl, DataBrief, DataType, DocName, DocBase64);
        }
        [WebMethod(Description = "返回资料列表(string)")]
        public string ReturnDataList(string pageNum)
        {
            return dbOperation.DataList(pageNum);
        }
        [WebMethod(Description = "返回包含关键字的资料列表(string)")]
        public string ReturnDataSearch(string Word)
        {
            return dbOperation.DataSearch(Word);
        }
        [WebMethod(Description = "返回资料详情(string)")]
        public string ReturnDataDetail(string DataID, string UserID)
        {
            return dbOperation.DataDetail(DataID, UserID);
        }
        [WebMethod(Description = "下载文件(Base64)")]
        public string ReturnFile(string FileName)
        {
            return dbOperation.DownloadFile(FileName);
        }
        [WebMethod(Description = "资料有用指数(bool)")]
        public string UpdateGoodOrBad(string DataID, string UserID, string Type)
        {
            return dbOperation.DataGoodOrBad(DataID, UserID, Type);
        }
        [WebMethod(Description = "评价资料(bool)")]
        public string UpdateComment(string UserID, string DataID, string Comment)
        {
            return dbOperation.CommentData(UserID, DataID, Comment);
        }
        [WebMethod(Description = "返回资料贡献排名列表(string)")]
        public string ReturnDevoteList()
        {
            return dbOperation.DevoteList();
        }
        [WebMethod(Description = "返回包含关键字的资料贡献排名(string)")]
        public string ReturnWordDevoteList(string Word)
        {
            return dbOperation.WordDevoteList(Word);
        }
        [WebMethod(Description = "存储一个项目的众筹信息(bool)")]
        public string SendRaised(string ProjectID, string Target, string Calendar, string Brief, string Img, string RepayList)
        {
            return dbOperation.SendRaised(ProjectID, Target, Calendar, Brief, Img, RepayList);
        }
        [WebMethod(Description = "读取一个项目的众筹信息(string)")]
        public string ReturnRaised(string ProjectID)
        {
            return dbOperation.LoadRaised(ProjectID);
        }
        [WebMethod(Description = "支持一个众筹项目(bool)")]
        public string UpdateHadMoney(string ProjectID, string UserID, string Number)
        {
            return dbOperation.SupportRaised(ProjectID, UserID, Number);
        }
        [WebMethod(Description = "获取通知列表(string)")]
        public string GetNotification(string UserID)
        {
            return dbOperation.GetNotification(UserID);
        }
        [WebMethod(Description = "设置一个通知不再提示(bool)")]
        public string SetNofication(string ID)
        {
            return dbOperation.SetNofication(ID);
        }
        [WebMethod(Description = "搜索用户(string)")]
        public string SearchUser(string Word)
        {
            return dbOperation.SearchUser(Word);
        }
        [WebMethod(Description = "返回分组(string)")]
        public string GetGroup(string UserID)
        {
            return dbOperation.GetGroup(UserID);
        }
        [WebMethod(Description = "添加为好友(bool)")]
        public string AddFriend(string MyID, string YouID, string Group)
        {
            return dbOperation.AddFriend(MyID, YouID, Group);
        }
        [WebMethod(Description = "好友列表(string)")]
        public string GetFriendList(string UserID)
        {
            return dbOperation.GetFriendList(UserID);
        }
        [WebMethod(Description = "消息列(string)")]
        public string GetMessage(string MyID, string YouID)
        {
            return dbOperation.GetMessage(MyID, YouID);
        }
        [WebMethod(Description = "发送消息(bool)")]
        public string SendMessage(string MyID, string YouID, string Message)
        {
            return dbOperation.SendMessage(MyID, YouID, Message);
        }
        [WebMethod(Description = "更新个人简介(bool)")]
        public string UpdateInfo(string UserID, string Name, string Age, string Sex, string School, string Like, string Brief, string Question, string Answer)
        {
            return dbOperation.UpdateInfo(UserID, Name, Age, Sex, School, Like, Brief, Question, Answer);
        }
        [WebMethod(Description = "返回个人上传的资料(string)")]
        public string ReturnMyDataList(string UserID, string Page)
        {
            return dbOperation.ReturnMyDataList(UserID, Page);
        }
        [WebMethod(Description = "删除一条资料(bool)")]
        public string DeleteData(string DataID)
        {
            return dbOperation.DeleteData(DataID);
        }
        [WebMethod(Description = "搜索自己的资料(string)")]
        public string ReturnMyDataSearch(string UserID,string Word)
        {
            return dbOperation.ReturnMyDataSearch(UserID, Word);
        }
    }
}
