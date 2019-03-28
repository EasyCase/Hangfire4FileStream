using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Hangfire4FileStream
{
    class Program
    {
        public static string DbConnectStr;
        static void Main(string[] args)
        {

            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configurationRoot = builder.Build();
            string MyDatabaseStr = configurationRoot["MyDatabase"].ToString();
            DbConnectStr = MyDatabaseStr;
            Console.WriteLine(MyDatabaseStr);

            GlobalConfiguration.Configuration.UseColouredConsoleLogProvider()
                .UseSqlServerStorage(MyDatabaseStr);
            var manager = new RecurringJobManager();
            //Expression<Action> a = myMethod;
            using (var server = new BackgroundJobServer())
            {
                //BackgroundJob.Enqueue(() => Console.WriteLine("Simple111"));
                RecurringJob.AddOrUpdate(() => FileWrite(), Cron.Minutely);//注意最小单位是分钟
                //RecurringJob.AddOrUpdate(()=> insertTripOrderSync(),Cron.Minutely);
                Console.WriteLine("Hangfire Server started. Press any key to exit...");
                Console.ReadKey();

            }

            #region MyRegion
            //IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            //IConfigurationRoot configuration = builder.Build();
            //Console.WriteLine($"ServerCode:{configuration["ServerCode"]}");
            //UserInfo user1 = new UserInfo();
            //UserInfo user2 = new UserInfo();
            //configuration.GetSection("section0").Bind(user1);
            //Console.WriteLine(user1.ToString());

            //configuration.GetSection("section1").Bind(user2);
            //Console.WriteLine(user2.ToString());

            //Console.WriteLine($"section0:UserId:{configuration["section0:UserId"]},,UserName:{configuration["section1:UserName"]}");

            Console.ReadKey();
            #endregion
        }

        public static void FileWrite()
        {
            Encoding encoder = Encoding.UTF8;
            byte[] bytes = encoder.GetBytes("Hello World! \n\r");
            FileStream fs = new FileStream(@"D:\datisTeleSender\datisdata\log.txt", FileMode.OpenOrCreate);
            
            try
            {
                string[] arratisFileStr = File.ReadAllLines(@"D:\datisTeleSender\datisdata\arratis.ini");
                string[] depatisFileStr = File.ReadAllLines(@"D:\datisTeleSender\datisdata\depatis.ini");
                DAtisMsg arratis = new DAtisMsg();
                arratis.LoadDAtisMsg(@"D:\datisTeleSender\datisdata\arratis.ini");
                DAtisMsg depatis = new DAtisMsg();
                depatis.LoadDAtisMsg(@"D:\datisTeleSender\datisdata\depatis.ini");


                for (int i = 0; i < arratisFileStr.Length; i++)
                {
                    if (arratisFileStr[i].IndexOf("Time") == 0)
                    {
                        string[] timeArr = arratisFileStr[i].Split("=");
                        //timeArr[1] = DateTime.Now.ToString();
                        string newStr = arratisFileStr[i].Replace(timeArr[1], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        arratisFileStr[i] = newStr;
                        File.WriteAllLines(@"D:\datisTeleSender\datisdata\arratis.ini", arratisFileStr);
                        break;
                    }
                }
                for (int i = 0; i < depatisFileStr.Length; i++)
                {
                    if (depatisFileStr[i].IndexOf("Time") == 0)
                    {
                        string[] timeArr = depatisFileStr[i].Split("=");
                        string newStr = depatisFileStr[i].Replace(timeArr[1], DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        depatisFileStr[i] = newStr;
                        File.WriteAllLines(@"D:\datisTeleSender\datisdata\depatis.ini", depatisFileStr);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                string jj = "fail:" + DateTime.Now.ToString();
                fs.Write(encoder.GetBytes(jj), 0, jj.Length);
                throw ex;
            }

            string str = "success:" + DateTime.Now.ToString();
            fs.Write(encoder.GetBytes(str), 0, str.Length);
            Console.WriteLine("OK");
        }

        public static void myMethod()
        {
            DateTime dt = DateTime.Now;
            string str = dt.ToString();
            Console.WriteLine($"Transparent!:{str}");
        }

        public static void insertTripOrderSync()
        {
            FileStream fs = new FileStream(@"E:\Picture\2.jpg", FileMode.Open);
            int len = (int)fs.Length;
            byte[] fileData = new byte[len];
            fs.Read(fileData, 0, len);
            fs.Close();
            using (SqlConnection conn = new SqlConnection(DbConnectStr))
            {
                string sqlText = "INSERT INTO [dbo].[TripOrderSync] ([OutOrderId],[OrderPayId],[CheckInDateTime],[CheckNum],[LastCheckDateTime],[Status],[InsertTime]) VALUES";
                sqlText += "(@OutOrderId,@OrderPayId,@CheckInDateTime,@CheckNum,@LastCheckDateTime,@Status,@InsertTime)";
                List<SqlParameter> paraList = new List<SqlParameter>() {
                    new SqlParameter("@OutOrderId",7),
                     new SqlParameter("@OrderPayId",7),
                      new SqlParameter("@CheckInDateTime",DateTime.Now.ToString()),
                       new SqlParameter("@CheckNum","7"),
                        new SqlParameter("@LastCheckDateTime",DateTime.Now),
                         new SqlParameter("@Status",new Random().Next(1,10)),
                         new SqlParameter("@InsertTime",DateTime.Now.ToString())
                };

                //SqlParameterCollection paColla = new SqlParameterCollection() { new SqlParameter()};

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sqlText;
                cmd.Parameters.AddRange(paraList.ToArray());


                string sqlText2 = "INSERT INTO [dbo].[Categories]([CategoryName],[Description],[Picture])VALUES";
                sqlText2 += "(@CategoryName,@Description,@Picture)";

                List<SqlParameter> paraList2 = new List<SqlParameter>()
                {
                    new SqlParameter("@CategoryName","test"),
                    new SqlParameter("@Description","test"),
                    new SqlParameter("@Picture", fileData)
                };
                SqlCommand sqlCommand2 = conn.CreateCommand();
                sqlCommand2.CommandText = sqlText2;
                sqlCommand2.Parameters.AddRange(paraList2.ToArray());

                conn.Open();
                SqlTransaction trans = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "Test");
                cmd.Transaction = trans;
                sqlCommand2.Transaction = trans;
                try
                {
                    cmd.ExecuteNonQuery();
                    sqlCommand2.ExecuteNonQuery();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
            }
        }
    }
}
