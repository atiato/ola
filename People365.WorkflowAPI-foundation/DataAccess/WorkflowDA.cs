using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace People365.FoundationAPI
{
    public class FoundationDA
    {
        string _connStr;

        public FoundationDA(string connStr)
        {
            _connStr = connStr;
        }
        
        public List<WorkflowTask> GetWorkflowTaskByStatusAndPersonMobile(string personId, string userid, int taskStatus, string workflowTypeID)
        {
            List<WorkflowTask> tasks = new List<WorkflowTask>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connStr))
                {
                    using (SqlCommand command = new SqlCommand(Constants.proc_GetAllTasksByStatusAndPersonMobile, conn))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (!string.IsNullOrEmpty(personId)) command.Parameters.AddWithValue(Constants.PersonID, personId); else command.Parameters.AddWithValue(Constants.PersonID, DBNull.Value);
                        command.Parameters.AddWithValue(Constants.TaskStatus, taskStatus);
                        command.Parameters.AddWithValue(Constants.Userid, userid);
                        command.Parameters.AddWithValue(Constants.WorkflowTypeId, workflowTypeID);
                        command.Parameters.AddWithValue("@CultureId", "en");

                        conn.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            WorkflowTask task = new WorkflowTask();
                            task.TaskId = reader["TaskId"].ToString();
                            task.InstanceId = reader["InstanceId"].ToString();
                            task.PersonId = reader["OwnerId"].ToString();
                            task.PersonJob = reader["PersonJob"].ToString();
                            task.UserId = userid;

                            string owner = reader["Owner"].ToString();
                            if (!string.IsNullOrEmpty(owner) && owner.Contains("-"))
                            {
                                task.PersonName = owner.Split('-')[1];
                            }
                            else
                            {
                                string firstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : string.Empty;
                                string lastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : string.Empty;

                                if ((!string.IsNullOrEmpty(firstName)) && (!string.IsNullOrEmpty(lastName)))
                                {
                                    task.PersonName = string.Concat(firstName, " ", lastName);
                                }
                            }

                            task.Notes = reader["Memo"].ToString();
                            task.TaskStatus = (WorkflowTaskStatus)Enum.Parse(typeof(WorkflowTaskStatus), reader["TaskStatus"].ToString());
                            bool isRequiredAuthorization = reader["isRequiredAuthorization"].ToString() == "1" ? true : false;

                            int authorizationStatus = 0;
                            if (reader["AuthorizationStatus"] != DBNull.Value)
                            {
                                authorizationStatus = int.Parse(reader["AuthorizationStatus"].ToString());
                            }
                            task.IsApproved = isRequiredAuthorization ? authorizationStatus == 1 ? true : false : false;
                            task.ImageExists = reader["ImageExists"] != DBNull.Value ? reader["ImageExists"].ToString() == "1" ? true : false : false;
                            task.LeaveTypeId = reader["LeaveTypeID"].ToString();
                            task.LeaveType = reader["LeaveType"].ToString();
                            task.IsFullDay = bool.Parse(reader["IsFullDay"].ToString());

                            if (!task.IsFullDay)
                            {
                                task.FromTime = reader["TimeFrom"] != DBNull.Value ? DateTime.Parse(reader["TimeFrom"].ToString()) : DateTime.MinValue;
                                task.ToTime = reader["TimeTo"] != DBNull.Value ? DateTime.Parse(reader["TimeTo"].ToString()) : DateTime.MinValue;
                            }

                            task.FromDate = reader["ctpDate1"] != DBNull.Value ? DateTime.Parse(reader["ctpDate1"].ToString()) : DateTime.MinValue;
                            task.ToDate = reader["ctpDate2"] != DBNull.Value ? DateTime.Parse(reader["ctpDate2"].ToString()) : DateTime.MinValue;

                            tasks.Add(task);
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                FileWriter.LogException(ex);
            }

            return tasks;
        }

        public WorkflowTask GetRequestDetails(string instanceId, string taskId)
        {
            //CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            //culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            //Thread.CurrentThread.CurrentCulture = culture;

            //if (string.IsNullOrEmpty(dataBaseName))
            //    throw new ArgumentNullException("dataBaseName");

            if (string.IsNullOrEmpty(instanceId))
                throw new ArgumentNullException("instanceId");

            WorkflowTask task = new WorkflowTask();

            try
            {
                //WorkflowBL.EngineBusinessLogic engineBusinessLogic = new WorkflowBL.EngineBusinessLogic(dataBaseName);

               // WorkflowBL.WorkflowTaskLogic workflowTaskLogic = new WorkflowTaskLogic(dataBaseName);
                task = GetWorkflowTaskDetail(taskId);

                //if (!string.IsNullOrEmpty(taskId))
                //{
                //    Dictionary<string, bool> taskSettings = engineBusinessLogic.GetWorkflowTaskSettings(new Guid(taskId));
                //    if (taskSettings.ContainsKey("IsNotification") && taskSettings["IsNotification"])
                //    {
                //        task.IsNotification = true;
                //    }
                //}

                ContentType contentType = GetWorkflowInstanceValues(new Guid(instanceId), false);

                task.FromDate = DateTime.Parse(contentType.Properties["FromDate"].Value);
                task.ToDate = DateTime.Parse(contentType.Properties["ToDate"].Value);
                //task.LeaveTypeId = contentType.Properties[WorkflowLeaveRequestConstants.LeaveType].Value;
                task.Notes = contentType.Properties["Remarks"].Value;
                task.TaskId = taskId;

                //AssignedLeaveBL assignLeaveLogic = new AssignedLeaveBL(dataBaseName);
                //List<string> leaveTypeIdList = new List<string>();
                //List<string> personIdList = new List<string>();
                //leaveTypeIdList.Add(task.LeaveTypeId);
                //personIdList.Add(task.PersonId);
                //task.AvailableBalance = assignLeaveLogic.GetEmployeeBalance(DateTime.Now, leaveTypeIdList, null, personIdList, false, null, 1).Select(x => x.Balance).FirstOrDefault();

                if (!string.IsNullOrEmpty(contentType.Properties["DayDuty"].Value))
                {
                    if (contentType.Properties["FullDay"].Value == "1")
                    {
                        task.IsFullDay = true;
                        task.Duration = Math.Round(decimal.Parse(contentType.Properties["DayDuty"].Value), 2);
                    }
                    else
                    {
                        task.IsFullDay = false;
                        DateTime dt = DateTime.Parse(contentType.Properties["EquivalentHours"].Value);
                        decimal duration = Math.Round(dt.Hour + (decimal)((decimal)dt.Minute / 60), 2);
                        task.Duration = duration;

                        task.FromTime = DateTime.Parse(contentType.Properties["TimeFrom"].Value);
                        task.ToTime = DateTime.Parse(contentType.Properties["TimeTo"].Value);
                    }
                }

                return task;
            }
            catch (Exception ex)
            {
                return null;
                //ManageException(ex, ShowMessage);
            }
        }

        private WorkflowTask GetWorkflowTaskDetail(string taskId)
        {
            WorkflowTask workflowTask = null;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand("proc_WorkflowTaskDetailLoadByTaskID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TaskID", taskId);

                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        workflowTask = new WorkflowTask();

                        workflowTask.InstanceId = reader["InstanceID"].ToString();
                        workflowTask.IsApproved = reader["IsApproved"] != DBNull.Value ? reader["IsApproved"].ToString() == "1" ? true : false : false;
                        workflowTask.UserId = reader["OwnerID"].ToString();
                        workflowTask.PersonId = reader["OwnerID"].ToString();
                        workflowTask.PersonName = reader["Owner"].ToString();
                        workflowTask.InstanceStatus = (WorkflowTaskStatus)Enum.Parse(typeof(WorkflowTaskStatus), reader["InstanceStatusId"].ToString());
                        workflowTask.TaskStatus = (WorkflowTaskStatus)Enum.Parse(typeof(WorkflowTaskStatus), reader["TaskStatusId"].ToString());
                        workflowTask.LeaveTypeId = reader["LeaveTypeID"] != DBNull.Value ? reader["LeaveTypeID"].ToString() : null;
                        workflowTask.LeaveType = reader["LeaveType"] != DBNull.Value ? reader["LeaveType"].ToString() : null;
                    }

                    conn.Close();
                }
            }

            return workflowTask;
        }

        private ContentType GetWorkflowInstanceValues(Guid instanceId, bool isStartForm)
        {
            FileWriter.WriteToFile("BusinessLayer.EngineBusinessLogic -> GetWorkflowInstanceValues(instanceId: " + instanceId + ", isStartForm: " + isStartForm + ")");

            ContentType retVal = new ContentType();
            //todo: set the id, name.
            try
            {
                SqlConnection connection = new SqlConnection(_connStr);

               // WorkflowDataAccess dataAccess = new WorkflowDataAccess(this.DatabaseName);
                SqlDataReader reader = GetWorkflowInstanceValues(connection, instanceId.ToString(), isStartForm);

                while (reader.Read())
                {
                    ContentTypeProperty property = new ContentTypeProperty();

                    property.PropertyId = new Guid(reader["PropertyID"].ToString());
                    property.ContentTypeId = new Guid(reader["ContentTypeId"].ToString());
                    property.Name = reader["Name"].ToString();
                    property.Type = (PropertyType)Enum.Parse(typeof(PropertyType), reader["Type"].ToString());
                    property.Value = reader["Value"].ToString();

                    retVal.Properties.Add(property);
                }

                connection.Close();

                if (retVal.Properties.Count > 0)
                {
                    retVal.ContentTypeId = retVal.Properties[0].ContentTypeId;
                }
            }
            catch (Exception ex)
            {
                FileWriter.LogException(ex);
            }
            return retVal;
        }

        private SqlDataReader GetWorkflowInstanceValues(SqlConnection connection, string instanceId, bool isStartForm)
        {
            SqlDataReader reader = null;

            try
            {
                //SqlConnection connection = new SqlConnection(ConnectionStringHelper.GetAdoConnectionString(_dbName));

                SqlCommand command = new SqlCommand(Constants.proc_WorkflowInstanceGetContentType, connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue(Constants.InstanceId, instanceId);
                command.Parameters.AddWithValue(Constants.IsStartForm, isStartForm);

                connection.Open();
                reader = command.ExecuteReader();
                //connection.Close();
            }
            catch (Exception ex)
            {
                //People365Logging.HandleException(ex);
            }

            return reader;
        }

    }

    public class FileWriter
    {
        public static void WriteToFile(string message)
        {
            try
            {
                Directory.CreateDirectory(string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"\Logging"));

                string loggingPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"\Logging\", DateTime.Now.ToShortDateString().Replace("/", "-"), ".txt");

                StreamWriter writer = new StreamWriter(loggingPath, true);
                writer.WriteLine(string.Concat("Time: ", DateTime.Now.TimeOfDay, "------ Message: ", message));
                writer.Close();
            }
            catch
            {

            }
        }

        public static void LogException(Exception ex)
        {
            try
            {
                Directory.CreateDirectory(string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"\Logging"));

                string loggingPath = string.Concat(AppDomain.CurrentDomain.BaseDirectory, @"\Logging\", DateTime.Now.ToShortDateString().Replace("/", "-"), ".txt");

                string message = "Time: {0} ------- Message: {1} ------- Inner Exception: {2} ---------- Stack Trace: {3}";

                StreamWriter writer = new StreamWriter(loggingPath, true);
                writer.WriteLine(string.Format(message, DateTime.Now.TimeOfDay, ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty, ex.StackTrace != null ? ex.StackTrace : string.Empty));
                writer.Close();
            }
            catch
            {
            }
        }
    }

    public class ContentType
    {
        public ContentType()
        {
            Properties = new ContentTypePropertyCollection();
        }

        public Guid ContentTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ContentTypePropertyCollection Properties { get; set; }
    }

    public class ContentTypeProperty
    {
        public Guid PropertyId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        public PropertyType Type { get; set; }

        public string DefaultValue { get; set; }

        public Guid ContentTypeId { get; set; }
    }

    public enum PropertyType
    {
        Date = 1,
        DateTime = 2,
        String = 3,
        Number = 4,
        Boolean = 5
    }

    public class ContentTypePropertyCollection : IEnumerable<ContentTypeProperty>
    {
        public ContentTypePropertyCollection()
        {
            Properties = new List<ContentTypeProperty>();
        }

        internal List<ContentTypeProperty> Properties;

        public ContentTypeProperty this[int i]
        {
            get
            {
                return Properties[i];
            }
        }

        public ContentTypeProperty this[string propertyName]
        {
            get
            {
                return this.Properties.Find(x => x.Name == propertyName);
            }
        }

        public void Add(ContentTypeProperty contentTypeProperty)
        {
            this.Properties.Add(contentTypeProperty);
        }

        public int Count
        {
            get { return Properties.Count; }
        }

        public bool Contains(string propertyName)
        {
            return this.Properties.Find(x => x.Name == propertyName) != null ? true : false;
        }

        #region IEnumerable<ContentTypeProperty> Members

        public IEnumerator<ContentTypeProperty> GetEnumerator()
        {
            return this.Properties.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    yield return this;
        //}

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }



    public enum WorkflowTaskStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
        Redirected = 4,
        PendingOverrule = 5,
        NotFound = 6,
        OverdueExpired = 7,
        Faulted = 8,
    }

    public class WorkflowTask
    {
        public string RequestedById { get; set; }
        public string RequestedDate { get; set; }
        public string TaskId { get; set; }
        public string InstanceId { get; set; }
        public string UserId { get; set; }
        public string PersonId { get; set; }
        public string PersonName { get; set; }
        public string Notes { get; set; }
        public WorkflowTaskStatus InstanceStatus { get; set; }
        public WorkflowTaskStatus TaskStatus { get; set; }
        public bool IsNotification { get; set; }
        public string PersonJob { get; set; }
        public bool IsApproved { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public string FromTimeDisplay
        {
            get
            {
                return FromTime.ToString("HH:mm");
            }
        }
        public string ToTimeDisplay
        {
            get
            {
                return ToTime.ToString("HH:mm");
            }
        }
        public decimal AvailableBalance { get; set; }
        public string DateRange
        {
            get
            {
                string value = string.Empty;
                if (FromDate.Month == ToDate.Month)
                {
                    if (FromDate.Day == ToDate.Day)
                    {
                        if (IsFullDay)
                        {
                            value = string.Concat(FromDate.ToString("MMMM"), " ", FromDate.Day);
                        }
                        else
                        {
                            value = string.Concat(FromDate.ToString("MMMM"), " ", FromDate.Day, " ", FromTime.ToString("HH:mm"), " - ", ToTime.ToString("HH:mm"));
                        }
                    }
                    else
                    {
                        value = string.Concat(FromDate.ToString("MMMM"), " ", FromDate.Day, " - ", ToDate.Day);
                    }
                }
                else
                {
                    value = string.Concat(FromDate.ToString("MMMM"), " ", FromDate.Day, " - ", ToDate.ToString("MMMM"), " ", ToDate.Day);
                }

                return value;
            }
        }
        public string LeaveTypeId { get; set; }
        public string LeaveType { get; set; }
        public decimal Duration { get; set; }
        public bool IsChecked { get; set; }
        public bool IsFullDay { get; set; }
        public string TaskStatusDisplay
        {
            get
            {
                if (TaskStatus == WorkflowTaskStatus.Completed)
                {
                    return IsApproved ? "Approved" : "Rejected";
                }
                else
                {
                    return TaskStatus.ToString();
                }
            }
        }

        public bool IsOnBehalf { get; set; }
        const string DateFormatDisplay = "{0} {1} {2} {3}";
        public string FromDateDisplay
        {
            get
            {
                return string.Format(DateFormatDisplay, FromDate.ToString("dddd"), FromDate.ToString("MMMM"), FromDate.Day, FromDate.Year);
            }
        }

        public string ToDateDisplay
        {
            get
            {
                return string.Format(DateFormatDisplay, ToDate.ToString("dddd"), ToDate.ToString("MMMM"), ToDate.Day, ToDate.Year);
            }
        }

        public bool ImageExists { get; set; }

        //public List<ContentTypeProperty> Properties { get; set; }
    }

    internal class Constants
    {
        #region Procedures

        internal const string proc_GetUserByPositionId = "proc_GetUserByPositionId";
        internal const string proc_GetUserByReplacementPositionId = "proc_GetUserByReplacementPositionId";
        internal const string proc_TaskPropertyValueInsert = "proc_TaskPropertyValueInsert";
        internal const string proc_TaskActivityPropertyValueInsert = "proc_TaskActivityPropertyValueInsert";
        internal const string proc_TaskActivityPropertyValueUpdate = "proc_TaskActivityPropertyValueUpdate";
        internal const string proc_WorkflowInstancePropertyValueInsert = "proc_WorkflowInstancePropertyValueInsert";
        internal const string proc_WorkflowTaskUpdateStatus = "proc_WorkflowTaskUpdateStatus";
        internal const string proc_WorkflowTaskUpdateStatusOnly = "proc_WorkflowTaskUpdateStatusOnly";
        internal const string proc_TaskActivityInsert = "proc_TaskActivityInsert";
        internal const string proc_WorkflowTaskInsert = "proc_WorkflowTaskInsert";
        internal const string proc_WorkflowTaskCancelUncompletedTasks = "proc_WorkflowTaskCancelUncompletedTasks";
        internal const string proc_WorkflowTaskCancelUncompletedTasksByInstanceId = "proc_WorkflowTaskCancelUncompletedTasksByInstanceId";
        internal const string proc_GetPersonSuperior = "proc_GetPersonSuperior";
        internal const string proc_GetPersonAdminSuperior = "proc_GetPersonAdminSuperior";
        internal const string proc_GetUserByJobId = "proc_GetUserByJobId";
        internal const string proc_GetUserByJobLevelId = "proc_GetUserByJobLevelId";
        internal const string proc_TaskActivityUpdateStatus = "proc_TaskActivityUpdateStatus";
        internal const string proc_WorkflowInstanceUpdateStatus = "proc_WorkflowInstanceUpdateStatus";
        internal const string proc_WorkflowInstanceUpdateStatuses = "proc_WorkflowInstanceUpdateStatuses";
        internal const string proc_WorkflowInstanceUpdateIsApproved = "proc_WorkflowInstanceUpdateIsApproved";
        internal const string proc_WorkflowTaskCheckIfRequireAuthorization = "proc_WorkflowTaskCheckIfRequireAuthorization";
        internal const string proc_GetWorkflowInstanceDTOByState = "proc_GetWorkflowInstanceDTOByState";
        internal const string proc_GetAllTaskByStatus = "proc_GetAllTaskByStatus";
        internal const string proc_GetAllTasksByStatusAndPerson = "proc_GetAllTasksByStatusAndPerson";
        internal const string proc_GetAllTasksByStatusAndPersonMobile = "proc_GetAllTasksByStatusAndPersonMobile";
        internal const string proc_GetAllTasksByStatusAndPersonCount = "proc_GetAllTasksByStatusAndPersonCount";
        internal const string proc_WorkflowTaskUpdateStatusById = "proc_WorkflowTaskUpdateStatusById";
        internal const string proc_WorkflowActivityGetFillActivity = "proc_WorkflowActivityGetFillActivity";
        internal const string proc_personGetWorkflowOwnerId = "proc_personGetWorkflowOwnerId";
        internal const string GetTemplateActivityInfoDTOByTaskActivityId = "GetTemplateActivityInfoDTOByTaskActivityId";
        internal const string proc_GetTaskOwnersByTemplateActivityId = "proc_GetTaskOwnersByTemplateActivityId";
        internal const string proc_GetTaskOwnersByTemplateTaskId = "proc_GetTaskOwnersByTemplateTaskId";
        internal const string proc_WorkflowEngineSettingsSelect = "proc_WorkflowEngineSettingsSelect";
        internal const string proc_WorkflowTemplateDefinitionDeleteByID = "proc_WorkflowTemplateDefinitionDeleteByID";
        internal const string WorkflowTemplateDefinitionCanBeDeleted = "WorkflowTemplateDefinitionCanBeDeleted";
        internal const string proc_WorkflowTemplateDefinitionGetActiveTasks = "[proc_WorkflowTemplateDefinitionGetActiveTasks]";
        internal const string proc_WorkFlowStatusSysLoadAllByCultureID = "proc_WorkFlowStatusSysLoadAllByCultureID";
        internal const string proc_WorkflowInstancePropertyValueDeleteByInstanceId = "proc_WorkflowInstancePropertyValueDeleteByInstanceId";
        internal const string proc_WorkflowInstanceGetContentType = "proc_WorkflowInstanceGetContentType";
        internal const string proc_WorkflowInstanceGetInstanceContentTypeByTaskId = "proc_WorkflowInstanceGetInstanceContentTypeByTaskId";
        internal const string proc_GetWorkflowUserControlName = "proc_GetWorkflowUserControlName";
        internal const string proc_IsUserHasPermission = "proc_IsUserHasPermission";
        internal const string proc_WorkflowTemplateDefinitionPublish = "proc_WorkflowTemplateDefinitionPublish";
        internal const string proc_WorkflowTemplateDefinitionIsScopeExists = "proc_WorkflowTemplateDefinitionIsScopeExists";
        internal const string proc_GetPersonLocation = "proc_GetPersonLocation";
        internal const string proc_WorkflowTaskCompleteOverruledTask = "proc_WorkflowTaskCompleteOverruledTask";
        internal const string proc_GetWorkflowTaskSettings = "proc_GetWorkflowTaskSettings";
        internal const string proc_TaskPropertyValueGetGetByTaskID = "proc_TaskPropertyValueGetGetByTaskID";
        internal const string proc_GetOrganizationUnitManager = "proc_GetOrganizationUnitManager";
        internal const string proc_GetPersonEmail = "proc_GetPersonEmail";
        internal const string WFNotificationEmail_Select = "WFNotificationEmail_Select";
        internal const string proc_WorkflowTemplateDefinitionUpdateTypeName = "proc_WorkflowTemplateDefinitionUpdateTypeName";
        internal const string proc_WorkflowInstancesLoadByStatusCount = "proc_WorkflowInstancesLoadByStatusCount";
        internal const string proc_IncrementWFTaskDeadlineByID = "proc_IncrementWFTaskDeadlineByID";
        internal const string proc_WorkflowInstanceGetPendingCount = "proc_WorkflowInstanceGetPendingCount";

        #endregion

        internal const string PositionId = "@PositionId";
        internal const string TaskID = "@TaskID";
        internal const string PropertyID = "@PropertyID";
        internal const string Value = "@Value";
        internal const string TaskActivityID = "@TaskActivityID";
        internal const string NewStatus = "@NewStatus";
        internal const string InstanceId = "@InstanceID";
        internal const string StartDate = "@StartDate";
        internal const string IsStartForm = "@IsStartForm";

        internal const string SequencePerson = "@SequencePerson";
        internal const string TaskType = "@TaskType";
        internal const string PersonID = "@PersonID";
        internal const string isRequiredAuthorization = "@isRequiredAuthorization";
        internal const string AuthorizationStatus = "@AuthorizationStatus";
        internal const string AuthorizationDate = "@AuthorizationDate";
        internal const string NotificationType = "@NotificationType";
        internal const string NotificationDate = "@NotificationDate";
        internal const string ReplacementTaskID = "@ReplacementTaskID";
        internal const string DeadLine = "@DeadLine";
        internal const string Priority = "@Priority";
        internal const string TaskStatus = "@TaskStatus";
        internal const string TaskStatusDate = "@TaskStatusDate";
        internal const string Memo = "@Memo";
        internal const string FormRequestID = "@FormRequestID";
        internal const string jobId = "@jobId";
        internal const string jobLevelId = "@jobLevelId";
        internal const string Status = "@Status";
        internal const string WorkflowTypeId = "@WorkflowTypeId";
        internal const string TemplateActivityId = "@TemplateActivityId";
        internal const string TemplateTaskId = "@TemplateTaskId";
        internal const string IsApproved = "@IsApproved";
        internal const string TemplateID = "@TemplateID";
        internal const string CultureID = "@CultureID";
        internal const string ActivityIdd = "@ActivityId";
        internal const string OuTypeID = "@OuTypeID";
        internal const string OuID = "@OuID";
        internal const string UpdateDate = "@UpdateDate";
        internal const string OrganizationUnitIdd = "@OrganizationUnitId";
        internal const string TypeName = "@TypeName";
        internal const string Version = "@Version";

        internal const string OccupiedBy = "OccupiedBy";
        internal const string WorkflowTemplateTaskId = "WorkflowTemplateTaskId";

        internal const string TemplateActivityIdd = "TemplateActivityId";
        internal const string ActivityName = "ActivityName";
        internal const string ActivityId = "ActivityId";

        internal const string TaskIDD = "TaskID";
        internal const string PersonIDD = "PersonId";
        internal const string WorkFlowStatusID = "WorkFlowStatusID";
        internal const string Description = "Description";

        internal const string Csproj = "Csproj";
        internal const string AssemblyInfo = "AssemblyInfo";
        internal const string Settings = "Settings";
        internal const string Settings2 = "Settings2";
        internal const string Class = "Class";
        internal const string BatchFile = "BatchFile";

        internal const string OrganizationUnitID = "OrganizationUnit";
        internal const string jobIdd = "Job";
        internal const string jobLevelIdd = "JobLevel";
        internal const string OrganizationUnitTypeId = "OrganizationUnitType";

        internal const string HasDecision = "HasDecision";
        internal const string IsRequiredAuthorization = "IsRequiredAuthorization";
        internal const string IsNotification = "IsNotification";
        internal const string ActionTypeID = "ActionTypeID";

        internal const string FirstEmail = "FirstEmail";
        internal const string PositionIdd = "PositionID";

        internal const string NotificationEmail = "NotificationEmail";
        internal const string FollowUpEmail = "FollowUpEmail";
        internal const string CancellationEmail = "CancellationEmail";
        internal const string OverdueEmail = "OverdueEmail";
        internal const string OveruleEmail = "OveruleEmail";
        internal const string FinalNotificationEmail = "FinalNotificationEmail";
        internal const string FaultedEmail = "FaultedEmail";
        internal const string AttachEmail = "AttachEmail";
        internal const string FaultedAdminEmail = "FaultedAdminEmail";
        internal const string BackwardTaskOwnersEmail = "BackwardTaskOwnersEmail";
        internal const string RewindedEmail = "RewindedEmail";
        internal const string RequesterRewindEmail = "RequesterRewindEmail";

        internal const string _AuthorizationStatus = "AuthorizationStatus";

        internal const string People365 = "People365";
        internal const string LoggingFolder = @"\Logging\";
        internal const string Userid = "UserID";
    }
}
