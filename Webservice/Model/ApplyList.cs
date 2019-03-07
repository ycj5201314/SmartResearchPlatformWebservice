using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Webservice.Model
{
    public class ApplyList
    {
        private string duty;//职责
        private string name;//用户名
        private string face;//头像
        private string userid;//用户账号
        private int pid;//项目编号
        public string Duty
        {
            get
            {
                return duty;
            }

            set
            {
                duty = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Face
        {
            get
            {
                return face;
            }

            set
            {
                face = value;
            }
        }

        public string Userid
        {
            get
            {
                return userid;
            }

            set
            {
                userid = value;
            }
        }

        public int Pid
        {
            get
            {
                return pid;
            }

            set
            {
                pid = value;
            }
        }
    }
}