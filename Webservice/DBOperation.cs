using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Webservice.Model;

namespace Webservice
{
    public class DBOperation : IDisposable
    {
        private static string Split = "｜";
        private static SqlConnection sqlCon;
        private static Timer myTimer;
        private String ConServerStr = @"Data Source=FENGR;Initial Catalog=Cooperation;Integrated Security=True;MultipleActiveResultSets=True";
        //构造函数
        public DBOperation()
        {
            if (sqlCon == null)
            {
                sqlCon = new SqlConnection();
                sqlCon.ConnectionString = ConServerStr;
                sqlCon.Open();
            }
            //定时执行,搜索到期众筹项目,
            myTimer = new Timer(3600000);
            myTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            myTimer.Enabled = true;
            GC.KeepAlive(myTimer);
        }

        //关闭/销毁函数
        public void Dispose()
        {
            if (sqlCon != null)
            {
                sqlCon.Close();
                sqlCon = null;
            }
        }

        //周期执行
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //执行众筹到期提醒
            try
            {
                string date = DateTime.Now.ToString("yyyy-MM-dd");
                string sql_searchPID = "select 众筹.项目编号,项目名称 from 众筹 left join 项目 on 众筹.项目编号=项目.项目编号 where 预设时间<='" + date + "' and 目标金额<=已筹金额";//到期项目编号列表
                SqlCommand cmd_pid = new SqlCommand(sql_searchPID, sqlCon);
                SqlDataReader reader = cmd_pid.ExecuteReader();
                while (reader.Read())
                {
                    string PID = reader[0].ToString();//项目编号
                    string PName = reader[1].ToString();//项目名称
                    string sql_searchUID = "select distinct 用户账号 from 众筹支持 where 项目编号=" + PID;
                    SqlCommand cmd_uid = new SqlCommand(sql_searchUID, sqlCon);
                    SqlDataReader reader_uid = cmd_uid.ExecuteReader();
                    while (reader_uid.Read())
                    {
                        string UID = reader_uid[0].ToString();//用户账号
                        //挂起通知,等待用户打开客户端接收
                        string notifiStr = "您支持的众筹项目:[" + PName + "]已到期，并且达到了目标金额!请您在一周内进行现金支持";
                        string sql_notifi = "insert into 通知(用户账号,通知信息,通知指向) values('" + UID + "','" + notifiStr + "','众筹')";
                        SqlCommand cmd_notifi = new SqlCommand(sql_notifi, sqlCon);
                        cmd_notifi.ExecuteNonQuery();
                        cmd_notifi.Dispose();
                    }
                    reader_uid.Close();
                    cmd_uid.Dispose();
                }
                reader.Close();
                cmd_pid.Dispose();
            }
            catch { }

        }

        public string Calculator(string rmb)
        {
            string result = "";
            Regex reg = new Regex("^[0-9]+$");
            Match ma = reg.Match(rmb);
            if (ma.Success)
            {
                result = "是数字";
            }
            else
            {
                result = "不是数字";
            }
            return result;
        }

        /// <summary>
        /// 获取所有账户的信息
        /// </summary>
        public List<string> SelectAllUser()
        {
            List<string> list = new List<string>();

            try
            {
                string sql = "select * from 用户";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    //将结果集信息添加到返回向量中
                    list.Add(reader[0].ToString());
                    list.Add(reader[1].ToString());
                    list.Add(reader[2].ToString());
                    list.Add(reader[3].ToString());
                    list.Add(reader[4].ToString());
                    list.Add(reader[5].ToString());
                    list.Add(reader[6].ToString());

                }

                reader.Close();
                cmd.Dispose();

            }
            catch (Exception)
            {

            }
            return list;
        }

        /// <summary>
        /// 增加一个账户
        /// </summary>
        public string AddUser(string UserName, string PassWord, string NickName, string UserType)
        {
            try
            {
                string sql = "insert into 用户 (用户账号,用户密码,用户名称,用户类型) values ('" + UserName.Trim() + "','" + PassWord.Trim() + "','" + NickName.Trim() + "','" + UserType.Trim() + "')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return "true";
            }
            catch
            {
                return "false";
            }
        }

        /// <summary>
        /// 删除一个账户
        /// </summary>
        public bool DeleteUser(string UserName)
        {
            try
            {
                string sql = "delete from 用户 where 用户账号='" + UserName.Trim() + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查用户账户及密码
        /// </summary>
        public bool SelectOne(string UserName, string PassWord)
        {
            try
            {
                string sql = "select * from 用户 where 用户账号='" + UserName.Trim() + "' and 用户密码='" + PassWord.Trim() + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                cmd.Dispose();
                if (reader.Read())
                {
                    reader.Close();
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public string GetInfo(string UserID)
        {
            string list = "";
            try
            {
                string sql = "select 用户名称,性别,学校,年龄,擅长,简介,密保问题,密保答案 from 用户 where 用户账号='" + UserID + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//用户名称
                    list += (reader[1].ToString() == "" ? "未填写" : reader[1].ToString()) + Split;//性别
                    list += (reader[2].ToString() == "" ? "未填写" : reader[2].ToString()) + Split;//学校
                    list += (reader[3].ToString() == "" ? "未填写" : reader[3].ToString()) + Split;//年龄
                    list += (reader[4].ToString() == "" ? "未填写" : reader[4].ToString()) + Split;//擅长
                    list += (reader[5].ToString() == "" ? "未填写" : reader[5].ToString()) + Split;//简介
                    list += (reader[6].ToString() == "" ? "未填写" : reader[6].ToString()) + Split;//密保问题
                    list += (reader[7].ToString() == "" ? "未填写" : reader[7].ToString()) + Split;//密保答案
                }
                cmd.Dispose();
                reader.Close();
            }
            catch
            {
            }
            return list;
        }

        /// <summary>
        /// 增加一个项目
        /// </summary>
        public string AddProject(string UserName, string ProjectName, string ProjectContent, string ProjectTeacher, string NeedPeople, string ProjectType)
        {
            try
            {
                string State = "筹备";
                string Date = DateTime.Now.ToLongDateString();

                string sql = "insert into 项目 (用户账号,项目名称,项目内容,项目日期,指导老师,负责人,需求人员,项目状态,参与人员,申请人员,项目类型) values ('"
                    + UserName.Trim() + "','" + ProjectName.Trim() + "','" + ProjectContent.Trim() + "','" + Date + "','"
                    + ProjectTeacher.Trim() + "','" + UserName.Trim() + "','" + NeedPeople.Trim() + "','" + State + "','','','" + ProjectType + "')";

                string sql1 = "insert into 项目 (用户账号,项目名称,项目内容,项目日期,负责人,需求人员,项目状态,参与人员,申请人员,项目类型) values ('"
                    + UserName.Trim() + "','" + ProjectName.Trim() + "','" + ProjectContent.Trim() + "','" + Date +
                    "','" + UserName.Trim() + "','" + NeedPeople.Trim() + "','" + State + "','','','" + ProjectType + "')";

                SqlCommand cmd;
                if (ProjectTeacher == "")
                {
                    cmd = new SqlCommand(sql1, sqlCon);
                }
                else
                {
                    cmd = new SqlCommand(sql, sqlCon);

                }
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                return "true";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 根据当前 页码 返回15个项目
        /// </summary>
        public string ProjectList(string page, string type)
        {
            string list = "";
            int ipage = Convert.ToInt16(page);
            int OnePage = 15;   //每页显示条目
            string sql;
            try
            {
                sql = "select top " + OnePage + " * from (select row_number() over(order by 项目.项目编号 DESC) as rownumber,用户名称,项目.用户账号,项目.项目编号,项目内容,头像,项目名称,预设时间 from 项目 left join 用户 on 项目.用户账号=用户.用户账号 left join 众筹 on 项目.项目编号=众筹.项目编号) A where rownumber >" + ipage * OnePage;
                if (type == "众筹")
                {
                    sql = "select top " + OnePage + " * from (select row_number() over(order by 项目.项目编号 DESC) as rownumber,用户名称,项目.用户账号,项目.项目编号,项目内容,头像,项目名称,预设时间 from 项目 left join 用户 on 项目.用户账号=用户.用户账号 left join 众筹 on 项目.项目编号=众筹.项目编号　where 预设时间 is not null) A where rownumber > " + ipage * OnePage;
                }
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list += reader[1].ToString() + Split;//用户名称
                    list += reader[2].ToString() + Split;//用户账号
                    list += reader[3].ToString() + Split;//项目编号
                    list += reader[4].ToString() + Split;//项目内容
                    list += (reader[5].ToString() == "" ? "null" : reader[5].ToString()) + Split;//头像
                    list += reader[6].ToString() + Split;//项目名称
                    list += (reader[7].ToString() == "" ? "null" : reader[7].ToString().Substring(0, reader[7].ToString().IndexOf(' '))) + Split;//众筹时间
                }
                reader.Close();
                cmd.Dispose();

            }
            catch
            {
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 返回一个项目的详细信息
        /// </summary>
        public string ProjectDetail(string ProjectID, string UserID)
        {
            string list = "";
            int id = Convert.ToInt32(ProjectID);
            try
            {
                string WhetherJoin_sql = "select 用户账号,指导老师,负责人,参与人员,申请人员 from 项目 where 项目编号='" + id + "'";//用于判断该用户是否参与此项目
                string teacher_sql = "(select 用户名称 from 用户 where 用户账号 =(select 指导老师 from 项目 left join 用户 on 项目.负责人 = 用户.用户账号 where 项目编号 = " + id + "))";//指导老师名称
                string sql = "select 项目名称,用户名称," + teacher_sql + ",项目内容,需求人员,项目类型,项目状态,预设时间 from 项目 left join 用户 on 项目.用户账号=用户.用户账号 left join 众筹 on 项目.项目编号=众筹.项目编号 where 项目.项目编号=" + id;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                SqlCommand cmd_WJ = new SqlCommand(WhetherJoin_sql, sqlCon);
                SqlDataReader reader_WJ = cmd_WJ.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//项目名称
                    list += reader[1].ToString() + Split;//负责人名称
                    list += (reader[2].ToString() == "" ? "暂无" : reader[2].ToString()) + Split;//指导老师
                    list += reader[3].ToString() + Split;//项目内容
                    list += reader[4].ToString() + Split;//需求人员
                    list += reader[5].ToString() + Split;//项目类型
                    list += reader[6].ToString() + Split;//项目状态
                    //是否参加
                    while (reader_WJ.Read())
                    {
                        if (reader_WJ[0].ToString() == UserID ||
                            reader_WJ[1].ToString() == UserID ||
                            reader_WJ[2].ToString() == UserID ||
                            (reader_WJ[3].ToString().IndexOf(":" + UserID + "|") >= 0) ||
                            (reader_WJ[4].ToString().IndexOf(":" + UserID + "|") >= 0))
                        {
                            list += "已参加" + Split;
                        }
                        else
                        {
                            list += "未参加" + Split;
                        }
                    }
                    list += reader[7].ToString() == "" ? "null" : reader[7].ToString();//众筹时间
                }
                reader_WJ.Close();
                reader.Close();
                cmd_WJ.Dispose();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 返回一个项目的当前信息
        /// </summary>
        public string NowProjectDetail(string ProjectID, string UserID)
        {
            string list = "";
            int id = Convert.ToInt32(ProjectID);
            try
            {
                string searchStr = "select 用户名称 from 用户 where 用户账号 in (";//参与人员名称select前半部分
                string tempJoin_sql = "select 参与人员 from 项目 where 项目编号=" + id;
                SqlCommand cmd_tempJoin = new SqlCommand(tempJoin_sql, sqlCon);
                SqlDataReader reader_tempJoin = cmd_tempJoin.ExecuteReader();

                reader_tempJoin.Read();
                string[] duty_name = null;
                string[] Duty = null;
                string[] Name = null;
                //有参与人员时,解析字符串
                if (reader_tempJoin[0].ToString() != "")
                {
                    duty_name = reader_tempJoin[0].ToString().Split('|');
                    Duty = new string[duty_name.Length];//职责列表
                    Name = new string[duty_name.Length];//用户名称列表
                    string[] tempArr;
                    for (int i = 0; i < duty_name.Length - 1; i++)
                    {
                        tempArr = duty_name[i].Split(':');
                        Duty[i] = tempArr[0];
                        if (i < duty_name.Length - 2)
                            searchStr += "'" + tempArr[1] + "',";
                        else
                            searchStr += "'" + tempArr[1] + "')";//拼接完成
                    }
                    SqlCommand cmd_Join = new SqlCommand(searchStr, sqlCon);
                    SqlDataReader reader_Join = cmd_Join.ExecuteReader();
                    int index = 0;
                    while (reader_Join.Read())
                    {
                        Name[index++] = reader_Join[0].ToString();
                    }
                    cmd_Join.Dispose();
                    reader_Join.Close();
                }
                else
                {
                    Duty = new string[0];
                    Name = new string[0];
                }


                string teacher_sql = "(select 用户名称 from 用户 where 用户账号 =(select 指导老师 from 项目 left join 用户 on 项目.用户账号 = 用户.用户账号 where 项目编号 = " + id + "))";//指导老师名称
                string sql = "select 项目名称,用户名称," + teacher_sql + ",项目内容,项目状态,项目类型,负责人 from 项目 left join 用户 on 项目.负责人=用户.用户账号 where 项目编号=" + id;//最终结果集
                string WhetherJoin_sql = "select 用户账号,指导老师,负责人,参与人员,申请人员 from 项目 where 项目编号='" + id + "'";//用于判断该用户是否参与此项目
                string raised_sql = "select count(*) from 众筹 where 项目编号=" + ProjectID;

                SqlCommand cmd_All = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd_All.ExecuteReader();
                SqlCommand cmd_WJ = new SqlCommand(WhetherJoin_sql, sqlCon);
                SqlDataReader reader_WJ = cmd_WJ.ExecuteReader();
                SqlCommand cmd_raised = new SqlCommand(raised_sql, sqlCon);
                string raised = "";
                if ((int)cmd_raised.ExecuteScalar() > 0)
                    raised = "已众筹";

                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//项目名称
                    list += reader[1].ToString() + Split;//负责人名称
                    list += (reader[2].ToString() == "" ? "暂无" : reader[2].ToString()) + Split;//指导老师
                    list += reader[3].ToString() + Split;//项目内容
                    list += reader[4].ToString() + Split;//项目状态
                    list += reader[5].ToString() + Split;//项目类型
                    //能否众筹
                    if (raised == "已众筹")
                    {
                        list += "已众筹" + Split;
                    }
                    else if (reader[6].ToString() == UserID)
                    {
                        list += "可众筹" + Split;
                    }
                    else
                    {
                        list += "不可众筹" + Split;
                    }
                    //参与人员
                    int x = 0;
                    do
                    {
                        list += (Duty.Length == 0 ? "暂无" : (Duty[x] + ":" + Name[x] + "|"));
                    } while (++x < Duty.Length - 1);
                    //是否参加
                    while (reader_WJ.Read())
                    {
                        if (reader_WJ[0].ToString() == UserID ||
                            reader_WJ[1].ToString() == UserID ||
                            reader_WJ[2].ToString() == UserID ||
                            (reader_WJ[3].ToString().IndexOf(":" + UserID + "|") >= 0) ||
                            (reader_WJ[4].ToString().IndexOf(":" + UserID + "|") >= 0))
                        {
                            list += Split + "已参加";
                        }
                        else
                        {
                            list += Split + "未参加";
                        }
                    }
                }
                reader_WJ.Close();
                reader_tempJoin.Close();
                reader.Close();
                cmd_WJ.Dispose();
                cmd_tempJoin.Dispose();
                cmd_All.Dispose();
            }
            catch (Exception ex)
            {
                list += ex.Source + ":" + ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 根据 项目编号 提交申请
        /// </summary>
        public bool ApplyJoinProject(string UserID, string ProjectID, string Duty)
        {
            string applyStr = Duty + ":" + UserID + "|";//要插入的字符串
            try
            {
                string sql = "update 项目 set 申请人员+='" + applyStr + "' where 项目编号=" + Convert.ToInt32(ProjectID);
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() == 0)
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 根据不同发布人返回我的项目列表
        /// </summary>
        public string MeProjectList(string UserID, string Who)
        {
            string list = "";
            try
            {
                string sql = "";
                if (Who == "自己")
                {
                    sql = "select 项目编号,项目名称,项目内容 from 项目 where 用户账号='" + UserID + "'";
                }
                else if (Who == "参与")
                {
                    sql = "select 项目编号,项目名称,项目内容 from 项目 where 指导老师='" + UserID + "' or 参与人员 like '%:" + UserID + "|%'";
                }
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//项目编号
                    list += reader[1].ToString() + Split;//项目名称
                    list += reader[2].ToString() + Split;//项目内容
                }
                reader.Close();
                cmd.Dispose();
            }
            catch { }
            if (list.Length == 0)
                list = " " + Split + "暂无" + Split + "去申请一个项目吧!" + Split;
            return list;
        }

        /// <summary>
        /// 根据需要的状态返回收藏的项目列表
        /// </summary>
        public string MeLikeProjectList(string UserID, string State)
        {
            string list = "";
            try
            {
                string sql = "select 收藏.项目编号,项目名称,项目内容 from 收藏 left join 项目 on 收藏.项目编号=项目.项目编号 where 收藏.用户账号='" + UserID + "' and ";
                if (State == "进行")
                    sql += "(项目状态='进行' or 项目状态='筹备')";
                else
                    sql += "项目状态='完成'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//项目编号
                    list += reader[1].ToString() + Split;//项目名称
                    list += reader[2].ToString() + Split;//项目内容
                }
                if (list == "")
                {
                    list = " " + Split + " " + Split + " " + Split + "";
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 增加或取消收藏
        /// </summary>
        public string ChangeProjectFavorite(string UserID, string ProjectID, string Checked)
        {
            string result = "";
            int pid = Convert.ToInt32(ProjectID);
            bool check = Convert.ToBoolean(Checked);
            try
            {
                if (check)
                {
                    string sql = "insert into 收藏 values('" + UserID + "'," + pid + ")";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        result = "favorite";
                    }
                    else
                    {
                        result = "收藏失败";
                    }
                    cmd.Dispose();
                }
                else
                {
                    string sql = "delete from 收藏 where 用户账号='" + UserID + "' and 项目编号=" + pid + "";
                    SqlCommand cmd = new SqlCommand(sql, sqlCon);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        result = "unfavorite";
                    }
                    else
                    {
                        result = "取消收藏失败";
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 检查是否收藏
        /// </summary>
        public bool ProjectState(string UserID, string ProjectID)
        {
            bool result = false;
            int pid = Convert.ToInt32(ProjectID);
            try
            {
                string sql = "select * from 收藏 where 用户账号='" + UserID + "' and 项目编号=" + pid;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                cmd.Dispose();
            }
            catch
            {
            }
            return result;
        }

        /// <summary>
        /// 返回该用户需要处理的项目申请
        /// </summary>
        public string ProjectMessageList(string UserID)
        {
            string list = "";
            try
            {
                Dictionary<string, ArrayList> allApplyList = new Dictionary<string, ArrayList>();//所有申请的键值对(项目名:申请list)

                string duty_name_pid_sql = "select 项目名称,申请人员,项目编号 from 项目 where 用户账号='" + UserID + "' and 申请人员 <>''";//查询该用户有申请人员的项目
                SqlCommand cmd_Apply = new SqlCommand(duty_name_pid_sql, sqlCon);
                SqlDataReader reader_Apply = cmd_Apply.ExecuteReader();

                while (reader_Apply.Read())
                {
                    //查询每个项目申请人员
                    string name_face_sql = "select 用户名称,头像,用户账号 from 用户 where 用户账号 in (";//用户名,头像,用户账号 前半部
                    string[] tempArr = reader_Apply[1].ToString().Split('|');//(职责:用户账号)
                    string[] duty = new string[tempArr.Length - 1];//职责
                    int pid = Convert.ToInt32(reader_Apply[2]);//项目编号
                    string valuesStr = "";
                    for (int i = 0; i < tempArr.Length - 1; i++)
                    {
                        string[] dutyname = tempArr[i].Split(':');
                        duty[i] = dutyname[0];//职责
                        valuesStr += dutyname[1] + ",";
                        if (i == tempArr.Length - 2)
                            name_face_sql += "'" + dutyname[1] + "') Order By charindex( 用户账号 , '" + valuesStr + "')"; //拼接完成
                        else
                            name_face_sql += "'" + dutyname[1] + "',"; //拼接
                    }

                    SqlCommand cmd_User = new SqlCommand(name_face_sql, sqlCon);
                    SqlDataReader reader_User = cmd_User.ExecuteReader();

                    ArrayList applyList = new ArrayList();//某一个项目的申请列表
                    int x = 0;
                    Array.Sort(duty);
                    while (reader_User.Read())
                    {
                        ApplyList al = new ApplyList();//某一个人的申请
                        al.Duty = duty[x++];//职责
                        al.Name = reader_User[0].ToString();//用户名称
                        al.Face = reader_User[1].ToString();//头像
                        al.Userid = reader_User[2].ToString();//用户账号
                        al.Pid = pid;//项目编号
                        applyList.Add(al);
                    }
                    allApplyList.Add(reader_Apply[0].ToString(), applyList);
                    cmd_User.Dispose();
                    reader_User.Close();
                }

                cmd_Apply.Dispose();
                reader_Apply.Close();
                //遍历
                foreach (KeyValuePair<string, ArrayList> a in allApplyList)
                {
                    string pname = a.Key;//项目名
                    foreach (ApplyList s in a.Value)
                    {
                        string duty = s.Duty;//职责
                        string name = s.Name;//用户名
                        string face = s.Face;//头像
                        string userid = s.Userid;//用户账号
                        int pid = s.Pid;//项目编号
                        list += pname + Split + duty + Split + name + Split + face + Split + userid + Split + pid + Split;
                    }
                }
            }
            catch (Exception ex)
            {
                list += ex.Message + ":" + ex.Source;
            }
            return list;
        }

        /// <summary>
        ///处理项目申请
        /// <summary>
        public string ChangeProjectApply(string UserID, string ProjectID, string Intent, string Duty, string ProjectName)
        {
            string result = "";
            try
            {
                string sql_ApplyStr = "select 申请人员 from 项目 where 项目编号=" + ProjectID + "";
                SqlCommand cmd_applystr = new SqlCommand(sql_ApplyStr, sqlCon);
                SqlDataReader reader_apply = cmd_applystr.ExecuteReader();

                if (reader_apply.Read())
                {
                    string applyStr = reader_apply[0].ToString();

                    string notifiStr = notifiStr = "您申请的项目:[" + ProjectName + "]的" + Duty + "职责";
                    string newApplyStr = applyStr.Replace(Duty + ":" + UserID + "|", "");
                    string sql_ChangeApply = "update 项目 set 申请人员='" + newApplyStr + "' where 项目编号=" + ProjectID + "";

                    if (Intent == "Agree")
                    {
                        //将用户添加到参与人员中
                        string agreeStr = Duty + ":" + UserID + "|";//参与人员字符段
                        string sql_Agree = "update 项目 set 参与人员+='" + agreeStr + "' where 项目编号=" + ProjectID + "";
                        SqlCommand cmd_Agree = new SqlCommand(sql_Agree, sqlCon);
                        cmd_Agree.ExecuteNonQuery();
                        cmd_Agree.Dispose();

                        //通知字符串
                        notifiStr += "已通过，可以进一步了解";
                    }
                    else
                    {
                        //通知字符串
                        notifiStr += "未通过，请尝试更多交流";
                    }

                    //通知用户
                    string sql_Notification = "insert into 通知(用户账号,通知信息,通知指向) values('" + UserID + "','" + notifiStr + "','项目')";
                    SqlCommand cmd_Notifi = new SqlCommand(sql_Notification, sqlCon);
                    cmd_Notifi.ExecuteNonQuery();
                    cmd_Notifi.Dispose();

                    //删除该申请人员字符段
                    SqlCommand cmd_change = new SqlCommand(sql_ChangeApply, sqlCon);
                    cmd_change.ExecuteNonQuery();
                    cmd_change.Dispose();
                }
                cmd_applystr.Dispose();
                reader_apply.Close();
                result = "true";
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 接收用户上传的头像
        /// </summary>
        public string UpLoadImage(string UserID, string face)
        {
            string result = "true";
            string RelativePath = @"/Work/FaceImg/" + UserID + ".jpg";
            string PathHead = "D:";
            try
            {
                MemoryStream stream = new MemoryStream(Convert.FromBase64String(face));//Base64转流
                Bitmap img = new Bitmap(stream);//流转位图
                img.Save(PathHead + RelativePath);//存储位图
                stream.Close();
            }
            catch
            {
                result = "false";
            }
            return result;
        }

        /// <summary>
        /// 返回用户头像的Base64编码
        /// </summary>
        public string DownloadFaceImage(string UserID)
        {
            string Base64 = "";
            string RelativePath = @"/Work/FaceImg/" + UserID + ".jpg";
            string PathHead = "D:";
            try
            {
                Image fromImage = Image.FromFile(PathHead + RelativePath);//打开图片
                MemoryStream stream = new MemoryStream();
                fromImage.Save(stream, ImageFormat.Jpeg);//图片转换到流
                Base64 = Convert.ToBase64String(stream.GetBuffer());//流转换为Base64
                stream.Close();
            }
            catch
            {
                Base64 = "NoExists";
            }
            return Base64;
        }

        /// <summary>
        /// 批量返回头像的Base64编码集合
        /// </summary>
        public string DownloadFaceImageList(string UserIDList)
        {
            string Base64List = "";
            string[] IDlist = UserIDList.Split(',');//提取用户ID
            for (int i = 0; i < IDlist.Length - 1; i++)
            {
                string RelativePath = @"/Work/FaceImg/" + IDlist[i] + ".jpg";
                string PathHead = "D:";
                Base64List += IDlist[i] + ":";
                try
                {
                    Image fromImage = Image.FromFile(PathHead + RelativePath);//打开图片
                    MemoryStream stream = new MemoryStream();
                    fromImage.Save(stream, ImageFormat.Jpeg);//图片转换到流
                    Base64List += Convert.ToBase64String(stream.GetBuffer());//UserID:Base64
                    stream.Close();
                }
                catch
                {
                    Base64List += "null";
                }
                Base64List += Split;
            }
            return Base64List;
        }

        /// <summary>
        /// 将一条资料存储在数据库,有附件则存储在硬盘
        /// </summary>
        public string DataUpload(string UserID, string DataName, string DownloadUrl, string DataBrief, string DataType, string DocName, string DocBase64)
        {
            string result = "";
            string Name = "";
            try
            {
                if (DocName != "null" && DocBase64 != "null")
                {
                    string DataTime = DateTime.Now.ToString().Replace(' ', '-').Replace('/', '．').Replace(':', '：');
                    Name = DataTime + "－" + DocName.Replace(',', '，');
                    string DocPath = "";
                    DocPath = @"D:/Work/DocFile/" + Name;
                    FileStream fs = new FileStream(DocPath, FileMode.Create);
                    BinaryWriter write = new BinaryWriter(fs);
                    write.Write(Convert.FromBase64String(DocBase64));
                    fs.Close();
                }
                string sql = "insert into 资料(用户账号,资料名称,下载地址,资料简介,资料类型,文档路径,评价内容) values('" + UserID + "','" + DataName + "','" + DownloadUrl + "','" + DataBrief + "','" + DataType + "','" + Name + "','')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                else
                    result = "false";
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }


            return result;
        }

        /// <summary>
        /// 根据页码返回资料列表
        /// </summary>
        public string DataList(string pageNum)
        {
            string list = "";
            int ipage = Convert.ToInt32(pageNum);
            int OnePage = 15;   //每页显示条目
            string sql = "select top " + OnePage + " * from (select row_number() over(order by 资料编号 DESC) as rownumber,资料编号,资料名称,用户名称,资料简介,浏览次数 from 资料 left join 用户 on 资料.用户账号=用户.用户账号) A where rownumber > " + ipage * OnePage;

            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[1].ToString() + Split;//资料编号
                    list += reader[2].ToString() + Split;//资料名称
                    list += reader[3].ToString() + Split;//用户名称
                    list += reader[4].ToString() + Split;//资料简介
                    list += reader[5].ToString() + Split;//浏览次数
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 根据关键字返回资料列表,用户名称优先匹配
        /// </summary>
        public string DataSearch(string Word)
        {
            string list = "";
            string sql_username = "select 资料编号,资料名称,用户名称,资料简介,浏览次数 from 资料 left join 用户 on 资料.用户账号=用户.用户账号 where 用户名称 like '%" + Word + "%'";
            string sql_brief_comment = "select 资料编号,资料名称,用户名称,资料简介,浏览次数 from 资料 left join 用户 on 资料.用户账号=用户.用户账号 where 资料名称 like '%" + Word + "%' or 资料简介 like '%" + Word + "%' or 评价内容 like '%" + Word + "%'";
            try
            {
                SqlCommand cmd = new SqlCommand(sql_username + " union " + sql_brief_comment, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//资料编号
                    list += reader[1].ToString() + Split;//资料名称
                    list += reader[2].ToString() + Split;//用户名称
                    list += reader[3].ToString() + Split;//资料简介
                    list += reader[4].ToString() + Split;//浏览次数
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 返回一个资料的详情
        /// </summary>
        public string DataDetail(string DataID, string UserID)
        {
            string list = "";
            string sql = "select 资料编号,资料名称,资料简介,评价内容,有用指数,无用指数,下载地址,文档路径 from 资料 where 资料编号=" + DataID;
            string sql_seeNum = "update 资料 set 浏览次数+=1 where 资料编号=" + DataID;
            string sql_check = "select count(*) from 资料评级 where 用户账号='" + UserID + "' and 资料编号=" + DataID;
            try
            {
                SqlCommand cmd = new SqlCommand(sql_seeNum, sqlCon);
                cmd.ExecuteNonQuery();



                cmd.Dispose();
            }
            catch { }

            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();

                SqlCommand cmd_check = new SqlCommand(sql_check, sqlCon);
                int check = Convert.ToInt16(cmd_check.ExecuteScalar());

                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//资料编号
                    list += reader[1].ToString() + Split;//资料名称
                    list += reader[2].ToString() + Split;//资料简介
                    list += reader[3].ToString() == "" ? "暂无" + Split : reader[3].ToString() + Split;//评论内容
                    list += reader[4].ToString() + Split;//有用指数
                    list += reader[5].ToString() + Split;//无用指数
                    list += reader[6].ToString() == "" ? "暂无" + Split : reader[6].ToString() + Split;//下载地址
                    list += reader[7].ToString() == "" ? "暂无" + Split : reader[7].ToString() + Split;//文档路径
                    list += check;//是否对该资料评级过
                }
                cmd.Dispose();
                cmd_check.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 根据资料名称(唯一)返回一个文件的Base64字符串
        /// </summary>
        public string DownloadFile(string FileName)
        {
            string Base64 = "";
            string RelativePath = @"/Work/DocFile/" + FileName;
            string PathHead = "D:";
            try
            {
                FileInfo fi = new FileInfo(PathHead + RelativePath);
                byte[] buff = new byte[fi.Length];

                FileStream fs = fi.OpenRead();
                fs.Read(buff, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                Base64 = Convert.ToBase64String(buff);//流转换为Base64
            }
            catch
            {
                Base64 = "NoExists";
            }
            return Base64;
        }

        /// <summary>
        /// 对资料的有用或无用指数进行+1  Type(0:有用,1:无用)
        /// </summary>
        public string DataGoodOrBad(string DataID, string UserID, string Type)
        {
            string result = "";
            string sql = "";
            string sql_rank = "";
            try
            {
                if (Type == "0")
                {
                    sql = "update 资料 set 有用指数+=1 where 资料编号=" + DataID;
                    sql_rank = "insert into 资料评级 values('" + UserID + "','" + DataID + "','0')";
                }
                else
                {
                    sql = "update 资料 set 无用指数+=1 where 资料编号=" + DataID;
                    sql_rank = "insert into 资料评级 values('" + UserID + "','" + DataID + "','1')";
                }
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                SqlCommand cmd_rank = new SqlCommand(sql_rank, sqlCon);
                cmd_rank.ExecuteNonQuery();
                cmd.Dispose();
                cmd_rank.Dispose();
                result = "true";
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 对资料进行评价
        /// </summary>
        public string CommentData(string UserID, string DataID, string Comment)
        {
            string result = "";
            string childSql = "(select 用户名称 from 用户 where 用户账号='" + UserID + "')";//用户名称
            string content = "+'：'+'" + Comment + "[" + DateTime.Now.ToString("yyyy-MM-dd") + "]\n";
            string sql = "update 资料 set 评价内容=" + childSql + content + "'+评价内容 where 资料编号=" + DataID;
            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                result = "true";
            }
            catch
            {
                result = "false" + sql;
            }

            return result;
        }

        /// <summary>
        /// 返回资料贡献 Top10 的用户列表
        /// </summary>
        public string DevoteList()
        {
            string list = "";
            string sql = "select top 10 资料.用户账号,用户名称,count(*) from 资料 left join 用户 on 资料.用户账号=用户.用户账号 group by 资料.用户账号,用户名称 order by count(*) desc";
            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//用户账号
                    list += reader[1].ToString() + Split;//用户名称
                    list += reader[2].ToString() + Split;//资料数量
                }
                cmd.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 根据关键字返回资料贡献用户列表
        /// </summary>
        public string WordDevoteList(string Word)
        {
            string list = "";
            string sql = "select *from (select 资料.用户账号,用户.用户名称,count(*) as 贡献量,row_number() over(order by count(*) desc) as 排名  from 资料 left join 用户 on 资料.用户账号=用户.用户账号 group by 资料.用户账号,用户.用户名称) as T where ";
            string sql_ranking = "排名 =" + Word;
            string sql_name = "用户名称 like '%" + Word + "%'";

            //判断是否纯数字
            Regex reg = new Regex("^[0-9]+$");
            Match ma = reg.Match(Word);
            if (ma.Success)
            {
                sql += sql_ranking;//进行排名搜索
            }
            else
            {
                sql += sql_name;//进行用户名称搜索
            }

            try
            {
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//用户账号
                    list += reader[1].ToString() + Split;//用户名称
                    list += reader[2].ToString() + Split;//贡献数量
                    list += reader[3].ToString() + Split;//排名
                }
                cmd.Dispose();
                reader.Close();
            }
            catch { }
            if (list.Length == 0)
                list = "null";
            return list;
        }

        /// <summary>
        /// 保存一条众筹信息,图片保存到硬盘
        /// </summary>
        public string SendRaised(string ProjectID, string Target, string Calendar, string Brief, string Img, string RepayList)
        {
            string result = "false";
            try
            {
                string sql = "insert into 众筹(项目编号,目标金额,预设时间,简介,支持与回报) values('" + ProjectID + "'," + Target + ",'" + Calendar + "','" + Brief + "','" + RepayList + "')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                if (Img != "null")
                {
                    string RelativePath = @"/Work/RaisedImg/" + ProjectID + ".jpg";
                    string PathHead = "D:";
                    MemoryStream stream = new MemoryStream(Convert.FromBase64String(Img));//Base64转流
                    Bitmap img = new Bitmap(stream);//流转位图
                    img.Save(PathHead + RelativePath);//存储位图
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 读取一条众筹信息
        /// </summary>
        public string LoadRaised(string ProjectID)
        {
            string list = "";
            try
            {
                //读取图片
                string Base64 = "";
                string RelativePath = @"/Work/RaisedImg/" + ProjectID + ".jpg";
                string PathHead = "D:";
                try
                {
                    Image fromImage = Image.FromFile(PathHead + RelativePath);//打开图片
                    MemoryStream stream = new MemoryStream();
                    fromImage.Save(stream, ImageFormat.Jpeg);//图片转换到流
                    Base64 = Convert.ToBase64String(stream.GetBuffer());//流转换为Base64
                    stream.Close();
                }
                catch
                {
                    Base64 = "NoExists";
                }

                string sql = "select * from 众筹 where 项目编号=" + ProjectID;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[1].ToString() + Split;//目标金额
                    list += reader[2].ToString() + Split;//预设时间
                    list += reader[3].ToString() + Split;//简介
                    list += reader[4].ToString() + Split;//支持与回报
                    list += reader[5].ToString() + Split;//已筹金额
                    list += Base64;//图片介绍
                }
            }
            catch (Exception ex)
            {
                list += ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 支持一个众筹项目
        /// </summary>
        public string SupportRaised(string ProjectID, string UserID, string Number)
        {
            string result = "false";
            try
            {
                string sql = "update 众筹 set 已筹金额+=" + Number + " where 项目编号=" + ProjectID;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                string sql_support = "insert into 众筹支持 values(" + ProjectID + ",'" + UserID + "'," + Number + ")";
                SqlCommand cmd_support = new SqlCommand(sql_support, sqlCon);
                if (cmd.ExecuteNonQuery() > 0 && cmd_support.ExecuteNonQuery() > 0)
                    result = "true";
            }
            catch { }
            return result;
        }

        /// <summary>
        /// 通知列表
        /// </summary>
        public string GetNotification(string UserID)
        {
            string list = "";
            try
            {
                string sql = "select 通知信息,通知指向,时间,通知编号 from 通知 where 不再通知 is null and 用户账号='" + UserID + "' order by 通知编号 desc";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//通知信息
                    list += reader[1].ToString() + Split;//通知指向
                    list += reader[2].ToString() + Split;//时间
                    list += reader[3].ToString() + Split;//通知编号
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = " " + Split + "无通知" + Split + " " + Split + " " + Split;
            return list;
        }

        /// <summary>
        /// 取消一个通知
        /// </summary>
        public string SetNofication(string ID)
        {
            string result = "false";
            try
            {
                string sql = "update 通知 set 不再通知=1 where 通知编号=" + ID;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
            }
            catch { }

            return result;
        }

        /// <summary>
        /// 搜索用户以便添加好友
        /// </summary>
        public string SearchUser(string Word)
        {
            string list = "";
            try
            {
                string sql = "select top 10 用户名称,学校,用户账号 from 用户 where 用户账号='" + Word + "' or 用户名称 like '%" + Word + "%'";

                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//用户名称
                    list += (reader[1].ToString() == "" ? "未填学校" : reader[1].ToString()) + Split;//学校
                    list += reader[2].ToString() + Split;//用户账号
                    //头像base64
                    try
                    {
                        string RelativePath = @"/Work/FaceImg/" + reader[2].ToString() + ".jpg";
                        string PathHead = "D:";
                        Image fromImage = Image.FromFile(PathHead + RelativePath);//打开图片
                        MemoryStream stream = new MemoryStream();
                        fromImage.Save(stream, ImageFormat.Jpeg);//图片转换到流
                        list += Convert.ToBase64String(stream.GetBuffer()) + Split;//UserID:Base64
                        stream.Close();
                    }
                    catch
                    {
                        list += "null" + Split;
                    }

                }
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 返回一个用户的所有分组,没有则创建一个默认分组
        /// </summary>
        public string GetGroup(string UserID)
        {
            string list = "";
            try
            {
                string sql = "select 分组名 from 好友分组 where 用户账号='" + UserID + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//分组名
                }
                //如果为空,则创建一个默认分组
                if (list == "")
                {
                    string sql_create = "insert into 好友分组 values('" + UserID + "','我的好友')";
                    SqlCommand cmd_create = new SqlCommand(sql_create, sqlCon);
                    if (cmd_create.ExecuteNonQuery() > 0)
                        list = "我的好友";
                    cmd_create.Dispose();
                }
                cmd.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            return list;
        }

        /// <summary>
        /// 添加用户为好友
        /// </summary>
        public string AddFriend(string MyID, string YouID, string Group)
        {
            string result = "false";
            try
            {
                string sql = "insert into 好友 values('" + MyID + "','" + YouID + "','" + Group + "')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 返回用户的所有好友列表
        /// </summary>
        public string GetFriendList(string UserID)
        {
            string list = "";
            try
            {
                string sql = "select 好友账号,用户名称,分组名 from 好友 left join 用户 on 好友账号=用户.用户账号 where 好友.用户账号='" + UserID + "' order by 分组名";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//好友账号
                    list += reader[1].ToString() + Split;//用户名称
                    list += reader[2].ToString() + Split;//分组名
                }
                cmd.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 返回两个用户之间的消息
        /// </summary>
        public string GetMessage(string MyID, string YouID)
        {
            string list = "";
            try
            {
                string sql = "select 发送账号,消息内容,发送时间 from 消息 where (发送账号='" + MyID + "' and 接受账号='" + YouID + "') or (发送账号='" + YouID + " ' and 接受账号='" + MyID + "')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//发送账号
                    list += reader[1].ToString() + Split;//消息内容
                    list += reader[2].ToString() + Split;//发送时间
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 插入一条消息
        /// </summary>
        public string SendMessage(string MyID, string YouID, string Message)
        {
            string result = "false";
            try
            {
                string sql = "insert into 消息(发送账号,接受账号,消息内容) values('" + MyID + "','" + YouID + "','" + Message + "')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                cmd.Dispose();
            }
            catch { }
            return result;
        }

        /// <summary>
        /// 更新个人简介
        /// </summary>
        public string UpdateInfo(string UserID, string Name, string Age, string Sex, string School, string Like, string Brief, string Question, string Answer)
        {
            string result = "false";
            try
            {
                string sql = "update 用户 set 用户名称='" + Name + "',性别='" + Sex + "',年龄='" + Age + "',学校='" + School + "',擅长='" + Like + "',简介='" + Brief + "',密保问题='" + Question + "',密保答案='" + Answer + "' where 用户账号='" + UserID + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                result += ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 根据页码返回个人上传的资料
        /// </summary>
        public string ReturnMyDataList(string UserID, string Page)
        {
            int OnePage = 15;
            int pageNum = Convert.ToInt32(Page);
            string list = "";
            try
            {
                string sql = "select top " + OnePage + " * from(select row_number() over(order by 资料编号 DESC) as row,资料编号,资料名称,资料简介,浏览次数 from 资料 where 用户账号='" + UserID + "') A where row>" + pageNum * OnePage;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[1].ToString() + Split;//资料编号
                    list += reader[2].ToString() + Split;//资料名称
                    list += reader[3].ToString() + Split;//资料简介
                    list += reader[4].ToString() + Split;//浏览次数
                }
                cmd.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }

        /// <summary>
        /// 根据资料编号删除一条资料
        /// </summary>
        public string DeleteData(string DataID)
        {
            string result = "false";
            try
            {
                //删除文件
                string sql_file = "select 文档路径 from 资料 where 资料编号=" + DataID;
                SqlCommand cmd_file = new SqlCommand(sql_file, sqlCon);
                SqlDataReader reader = cmd_file.ExecuteReader();
                while (reader.Read())
                {
                    string fileName = reader[0].ToString();
                    string Path = @"D:/Work/DocFile/" + fileName;
                    if (File.Exists(Path))
                    {
                        File.Delete(Path);
                    }
                }
                //删除相关资料评级
                string sql_deleteOther = "delete from 资料评级 where 资料编号=" + DataID;
                SqlCommand cmd_deleteOther = new SqlCommand(sql_deleteOther, sqlCon);
                cmd_deleteOther.ExecuteNonQuery();
                //删除资料条目
                string sql = "delete from 资料 where 资料编号=" + DataID;
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                if (cmd.ExecuteNonQuery() > 0)
                    result = "true";
                cmd.Dispose();
                cmd_deleteOther.Dispose();
                cmd_file.Dispose();
                reader.Close();
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 根据关键词返回该用户的资料
        /// </summary>
        public string ReturnMyDataSearch(string UserID, string Word)
        {
            string list = "";
            try
            {
                string sql = "select 资料编号,资料名称,资料简介,浏览次数 from 资料 where 用户账号='" + UserID + "' and (资料名称 like'%" + Word + "%' or 资料简介 like '%" + Word + "%')";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list += reader[0].ToString() + Split;//资料编号
                    list += reader[1].ToString() + Split;//资料名称
                    list += reader[2].ToString() + Split;//资料简介
                    list += reader[3].ToString() + Split;//浏览次数
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                list = ex.Message;
            }
            if (list == "")
                list = "null";
            return list;
        }
    }
}