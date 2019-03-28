using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Hangfire4FileStream
{
    //通报报文消息
   public class DAtisMsg
    {
        //类型,进场ARR，离场DEP
        private string _type;
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        //时间
        private string _msgTime;

        public string MsgTime
        {
            get { return _msgTime; }
            set { 
                _msgTime = value;               
            }
        }

        //简洁格式
        private string _msgTimeShort;

        public string MsgTimeShort
        {
            get { return _msgTimeShort; }
            set { _msgTimeShort = value; }
        }

        //版本
        private string _order;

        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }

        //场压
        private string _qNH;

        public string QNH
        {
            get { return _qNH; }
            set { _qNH = value; }
        }

        //加载DAtisMsg
       public void LoadDAtisMsg(string filePath) 
        {
            try
            {
                string[] msgContents = File.ReadAllLines(filePath);
                //标准格式为5行
               /* [ATIS ARR]
                    Time=2011-06-21 05:13:09
                    Order=F
                    Temperature=40
                    QNH=1000
                * */

                /*[ATIS DEP]
                    Time=2011-06-21 05:08:53
                    Order=E
                    Temperature=40
                    QNH=1000*/
                if (msgContents.Length > 0)
                {
                    int valueCount = 0;
                    for (int i = 0; i < msgContents.Length; i++)
                    {
                        if (msgContents[i] != null && msgContents[i].Length>0)
                        {
                            //解析类型
                            if (msgContents[i].Contains("ATIS"))
                            {
                                if (msgContents[i].Contains("ARR"))
                                {
                                    this.Type = "ARR";
                                    valueCount++;
                                }
                                else
                                {
                                    if (msgContents[i].Contains("DEP"))
                                    {
                                        this.Type = "DEP";
                                        valueCount++;
                                    }
                                    else
                                    {
                                        throw new Exception("DAtis数据文件msg类型异常，请检查数据文件！");
                                    }
                                }

                            }
                            //解析时间
                            if (msgContents[i].Contains("Time"))
                            {
                                string[] timeValueStrs = msgContents[i].Split('=');
                                if (timeValueStrs.Length > 1 && timeValueStrs[1] != null)
                                {
                                    this.MsgTime = timeValueStrs[1];

                                    if (MsgTime != null)
                                    {
                                        string[] timestrs = MsgTime.Split(':');
                                        if (timestrs.Length > 2)
                                        {
                                            this.MsgTimeShort = timestrs[0].Substring(timestrs[0].Length - 2, 2) + timestrs[1];
                                        }
                                        else 
                                        {
                                            throw new Exception("DAtis数据文件msg 时间 异常，请检查数据文件！");
                                        }
                                    }

                                    valueCount++;
                                }
                                else 
                                {
                                    throw new Exception("DAtis数据文件msg 时间 异常，请检查数据文件！");
                                }
                            }

                            //解析版本
                            if (msgContents[i].Contains("Order"))
                            {
                                string[] orderValueStrs = msgContents[i].Split('=');
                                if (orderValueStrs.Length > 1 && orderValueStrs[1] != null)
                                {
                                    this.Order = orderValueStrs[1];
                                    valueCount++;
                                }
                                else
                                {
                                    throw new Exception("DAtis数据文件msg版本 异常，请检查数据文件！");
                                }
                            }
                            //解析场压
                            if (msgContents[i].Contains("QNH"))
                            {
                                string[] QNHValueStrs = msgContents[i].Split('=');
                                if (QNHValueStrs.Length > 1 && QNHValueStrs[1] != null)
                                {
                                    this.QNH = QNHValueStrs[1];
                                    valueCount++;
                                }
                                else
                                {
                                    throw new Exception("DAtis数据文件msg QNH 异常，请检查数据文件！");
                                }
                            }
                        }
                    }

                    if ( !(valueCount == 4))                    
                    {
                        throw new Exception("DAtis数据文件属性不足4项，请检查数据文件！");
                    }
                }
                else
                {
                    throw new Exception("DAtis数据文件为空，请检查数据文件！");
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        //得到报文格式字符串
        public override string ToString()
        {
            string teleTemplate = "TYPE:{0}/ORDER:{1}/TIME:{2}UTC/QNH:Q{3}";
            string aftnMsg= String.Format(teleTemplate,this.Type,this.Order,this.MsgTimeShort,this.QNH);

            return aftnMsg;
        }

        
    }
}
