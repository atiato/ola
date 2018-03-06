using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace LinuxPerfomanceTest.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            try
            {
                FileWriter.WriteToFile("Before get data");
                GetData();
                FileWriter.WriteToFile("After get data");
            }
            catch (Exception ex)
            {
                FileWriter.LogException(ex);
            }
            return new string[] { "value1", "value2" };
        }

        string connectionString = "Password=sasql;Persist Security Info=True;User ID=sa;Initial Catalog=mea;Data Source=tcp:192.168.1.10";

        public void GetData()
        {
            TransactionOptions transOption = new TransactionOptions();
            transOption.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, transOption))
            {
                try
                {
                    ReadData();
                    InsertData();
                    ReadData();
                }
                catch (Exception ex)
                {
                    FileWriter.LogException(ex);
                    scope.Dispose();
                }
                scope.Complete();
            }
        }

        public void ReadData()
        {
            try
            {
                List<EmployeeWorkHour> lst = new List<EmployeeWorkHour>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 50 * FROM dbo.EmployeeWorkHour WHERE PersonID = '81d8e09f-ce98-45fa-a311-ca7d7deccb7d'", conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                EmployeeWorkHour employeeWorkHour = new EmployeeWorkHour();
                                employeeWorkHour.EmployeeWorkHourID = reader["EmployeeWorkHourID"].ToString();
                                employeeWorkHour.PersonID = reader["PersonID"].ToString();
                                employeeWorkHour.AttendanceTypeID = reader["AttendanceTypeID"].ToString();

                                lst.Add(employeeWorkHour);
                            }

                            FileWriter.WriteToFile("Success Read...");
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                FileWriter.LogException(ex);
            }
        }


        public void InsertData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("insert into AccessGroup values(NEWID(), 'TestAccessGroup')", conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();

                        FileWriter.WriteToFile("Read success...");
                    }
                }
            }
            catch (Exception ex)
            {
                FileWriter.LogException(ex);
            }
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    internal class FileWriter
    {
        static string lockWriteToFile = "0";
        internal static void WriteToFile(string message)
        {
            lock (lockWriteToFile)
            {
                try
                {
                    Directory.CreateDirectory(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "/var/nfs-data/"));

                    string loggingPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "/var/nfs-data/", DateTime.Now.ToShortDateString().Replace("/", "-"), ".txt");

                    StreamWriter writer = new StreamWriter(loggingPath, true);
                    writer.WriteLine(string.Concat("Time: ", DateTime.Now.TimeOfDay, "------ Message: ", message));
                    writer.Close();
                }
                catch { }
            }
        }

        static string lockLogException = "0";
        internal static void LogException(Exception ex)
        {
            lock (lockLogException)
            {
                try
                {
                    Directory.CreateDirectory(string.Concat(AppDomain.CurrentDomain.BaseDirectory, "/var/nfs-data/"));

                    string loggingPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "/var/nfs-data/", DateTime.Now.ToShortDateString().Replace("/", "-"), ".txt");

                    string message = "Time: {0} ------- Message: {1} ------- Inner Exception: {2} ---------- Stack Trace: {3}";

                    StreamWriter writer = new StreamWriter(loggingPath, true);
                    writer.WriteLine(string.Format(message, DateTime.Now.TimeOfDay, ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty, ex.StackTrace != null ? ex.StackTrace : string.Empty));
                    writer.Close();

                    if (ex is System.Reflection.ReflectionTypeLoadException)
                    {
                        ReflectionTypeLoadException typeLoadException = ex as ReflectionTypeLoadException;
                        StringBuilder sb = new StringBuilder();
                        foreach (Exception exSub in typeLoadException.LoaderExceptions)
                        {
                            sb.AppendLine(exSub.Message);
                            FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                            if (exFileNotFound != null)
                            {
                                if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                                {
                                    sb.AppendLine("Fusion Log:");
                                    sb.AppendLine(exFileNotFound.FusionLog);
                                }
                            }
                            sb.AppendLine();
                        }
                        string errorMessage = sb.ToString();
                        FileWriter.WriteToFile(errorMessage);
                    }
                }
                catch { }
            }
        }
    }

    public class EmployeeWorkHour
    {
        public string EmployeeWorkHourID { get; set; } //(char(36), not null)
        public string PersonID { get; set; } //(char(36), not null)
        public DateTime WorkHourDate { get; set; } //(datetime, not null)
        public DateTime ApplyDate { get; set; } //(datetime, not null)
        public DateTime CreationDate { get; set; } //(datetime, not null)
        public DateTime FromTime { get; set; } //(datetime, not null)
        public DateTime ToTime { get; set; } //(datetime, not null)
        public DateTime EquivalentHours { get; set; } //(datetime, not null)
        public string AttendanceTypeID { get; set; } //(char(36), not null)
        public int PolicyTypeID { get; set; } //(int, not null)
        public string ScheduleHeaderID { get; set; } //(char(36), not null)
        public string EmployeeWorkHourSourceID { get; set; } //(char(1), not null)
        public string SourceTransactionID { get; set; } //(char(36), null)
        public DateTime RealHours { get; set; } //(datetime, not null)
        public string EmployeeDayDutyID { get; set; } //(char(36), not null)
        public bool FirstAttendance { get; set; } //(bit, not null)
        public string ScheduleShiftSplitID { get; set; } //(char(36), null)
        public string BookingID { get; set; } //(nvarchar(36), null)
    }

}
