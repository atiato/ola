using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace People365.EmployeeService.Controllers
{
    [Produces("application/json")]
    [Route("api/Employee")]
    public class EmployeeController : Controller
    {
        // GET api/values
        [HttpGet]
        [Route("items")]
        public List<EmployeeFilter> Get()
        {
            DataAccess da = new DataAccess();
            return da.GetFilteredEmployeesWithTerminatee();
        }
    }

    public class Person
    {
        public int PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class DataAccess
    {
        public List<EmployeeFilter> GetFilteredEmployeesWithTerminatee()
        {
            return this.GetFilteredEmployeesWithTerminate(new List<string>(), new List<string>(), new List<string>(), new List<string>(), 
                "15BFFEE0-4D01-44D5-B82F-84AB8695BF67", null, null, null, null, null, null, null, null, null, null, null, null, "", "", false);
        }

        private List<EmployeeFilter> GetFilteredEmployeesWithTerminate(List<string> OrganizationUnitIDs, List<string> JobLevelIDs,
           List<string> JobCategoryIDs, List<string> JobTitleIDs, string userID, DateTime? fromHireDate, DateTime? ToHireDate,
           DateTime? fromTerminationDate, DateTime? ToTerminationDate, DateTime? FromDate, DateTime? ToDate,
           List<string> EmploymentTypeIDs, List<string> NationalityIDs, List<string> GenderIDs, string CostCenterID, List<string> GradeIDs,
           List<string> GroupIDs, string OUTypeIDs, string ShowJoinDate, bool userSelectedOUsToFilter)
        {
            List<EmployeeFilter> _lstEmp = new List<EmployeeFilter>();
            string commmandCostCenterFrom = string.Empty;
            string commmandCostCenterWhere = string.Empty;

            if (!string.IsNullOrEmpty(CostCenterID))
            {
                //RitaGeha: fromDate and toDate are used to get the query related to the CostCenter part, dependent 
                //on the from and to dates in the employee filter control
                string fromDate = string.Empty;
                string toDate = string.Empty;
                if (ToDate != null)
                {
                    toDate = new CommonDTO().HijriToGreg(ToDate.Value, "yyyyMMdd");
                }
                else
                {
                    toDate = new CommonDTO().HijriToGreg(DateTime.Now.Date, "yyyyMMdd");
                }

                if (FromDate != null)
                {
                    fromDate = new CommonDTO().HijriToGreg(FromDate.Value, "yyyyMMdd");
                }
                else
                {
                    fromDate = new CommonDTO().HijriToGreg(DateTime.Now.Date, "yyyyMMdd");
                }

                commmandCostCenterFrom = " INNER JOIN dbo.EmployeeCostCenter ec1 ON p.PersonID = ec1.PersonID ";
                commmandCostCenterWhere = " AND ec1.CostCenterID = '" + CostCenterID +
                "' AND (((ec1.StartingDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "') AND (ec1.TillDate IS NULL)) OR ((ec1.StartingDate BETWEEN '" + fromDate + "' and '" + toDate +
                "') AND (ec1.TillDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "')) or (ec1.StartingDate <= '" + fromDate + "' AND ec1.TillDate IS NULL) OR ((ec1.StartingDate <= '" + fromDate +
                "') AND ( ec1.TillDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "')) or ((ec1.StartingDate <= '" + fromDate + "') AND (ec1.TillDate >= '" + toDate +
                "')) or (( ec1.StartingDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "') AND (ec1.TillDate >= '" + toDate +
                "'))) And StartingDate = (SELECT MAX(ec2.StartingDate) FROM dbo.EmployeeCostCenter ec2 WHERE ec1.PersonID = ec2.PersonID AND (((ec1.StartingDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "') AND (ec1.TillDate IS NULL)) OR ((ec1.StartingDate BETWEEN '" + fromDate + "' and '" + toDate +
                "') AND (ec1.TillDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "')) or (ec1.StartingDate <= '" + fromDate + "' AND ec1.TillDate IS NULL) OR ((ec1.StartingDate <= '" + fromDate +
                "') AND ( ec1.TillDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "')) or ((ec1.StartingDate <= '" + fromDate + "') AND (ec1.TillDate >= '" + toDate +
                "')) or (( ec1.StartingDate BETWEEN '" + fromDate + "' AND '" + toDate +
                "') AND (ec1.TillDate >= '" + toDate +
                "'))))";
            }

            string OUByType = (!string.IsNullOrEmpty(OUTypeIDs)) ? "OUDOU{0}.Description AS OU{0} " : "NULL AS OU{0}";

            string OUByTypeSinceDate = (!string.IsNullOrEmpty(OUTypeIDs) && Convert.ToBoolean(ShowJoinDate)) ? "OU{0}.SinceDate AS OU{0}SinceDate " : "NULL AS OU{0}SinceDate ";

            string OUByTypeJoins = (!string.IsNullOrEmpty(OUTypeIDs)) ? " LEFT OUTER JOIN dbo.PersonOUStructure OU{0} ON p.personID = OU{0}.personid AND OU{0}.OrganizationUnitTypeID = ( SELECT val FROM dbo.CustomSPLIT( '{1}', ',') WHERE id = {0}) " +
                " LEFT OUTER JOIN dbo.OrganizationUnit OU{0}OU ON OU{0}OU.OrganizationUnitID = OU{0}.OrganizationUnitID " +
                " LEFT OUTER JOIN  dbo.OrganizationUnitDefinition OUDOU{0} ON OUDOU{0}.OrganizationUnitDefinitionID = OU{0}OU.OrganizationUnitDefinitionID  " : string.Empty;

            string command = "select";

            command += ((NationalityIDs != null && NationalityIDs.Count > 0) ? " distinct" : string.Empty);

            command += " p.PersonID ,e.EmployeeID,p.FirstName,p.LastName,e.HireDate,e.TerminationDate, j.JobTitle,oud.description as OrganizationUnit,p.BirthDate, ET.Name AS EmploymentType,e.IsNSSFCovered, " +
                        string.Format(OUByType, 1) + " , " +
                        string.Format(OUByTypeSinceDate, 1) + " , " +
                        string.Format(OUByType, 2) + " , " +
                        string.Format(OUByTypeSinceDate, 2) + " , " +
                        string.Format(OUByType, 3) + " , " +
                        string.Format(OUByTypeSinceDate, 3) +
                        " , p.FatherName " +
                        " from Employee e Inner Join  person p" +
                        " on p.PersonID = e.PersonID" +
                        " INNER JOIN EmploymentTypeHistory ETH ON e.PersonID = ETH.PersonID" +
                        " INNER JOIN EmploymentType ET ON ETH.EmploymentTypeID = ET.EmploymentTypeID";

            if (!string.IsNullOrEmpty(CostCenterID))
            {
                command += commmandCostCenterFrom;
            }
            if (!string.IsNullOrEmpty(OUByTypeJoins))
            {
                command += string.Format(OUByTypeJoins, 1, OUTypeIDs);
                command += string.Format(OUByTypeJoins, 2, OUTypeIDs);
                command += string.Format(OUByTypeJoins, 3, OUTypeIDs);
            }

            command += " Inner Join GetSecuredEmployeesByUserID('" + userID + "') AS t ON p.[PersonID] = t.PersonID";

            command += ((NationalityIDs != null && NationalityIDs.Count > 0) ? " LEFT OUTER JOIN PersonNationality pn ON p.PersonID = pn.PersonID" : null);

            command += "  left outer join   PositionHistory ph" + " on  p.PersonID = ph.OccupiedBy" +
                " left outer join  position ps" + " on ph.PositionID = ps.PositionID" +
                "  left outer join  job j" + " on  ps.JobID = j.JobID" +
                ((OrganizationUnitIDs.Count == 0 || !userSelectedOUsToFilter) ? "  left outer join  OrganizationUnit ou on " : " INNER JOIN OrganizationUnit ou on  ou.OrganizationUnitID in ( select * from fn_splitguids('" + string.Join(",", OrganizationUnitIDs) + "')) and ") + "  ou.OrganizationUnitID = ps.OrganizationUnitID " +
                "  left outer join  OrganizationUnitDefinition oud" + " on ou.OrganizationUnitDefinitionID = oud.OrganizationUnitDefinitionID" +
                " LEFT OUTER JOIN dbo.EmployeeGrade EG ON EG.PersonID = p.PersonID" +
                " LEFT OUTER JOIN dbo.Grade G ON G.GradeID=EG.GradeID" +
                " where ";

            if (GroupIDs != null && GroupIDs.Count > 0)
            {

                command += " p.PersonID IN  (SELECT EGr.PersonID FROM dbo.EmployeeGroup EGr, Person p,GroupList GL " +
                           "WHERE EGr.PersonID = p.PersonID AND GL.GroupID = EGr.GroupID  " +
                             " and (GL.GroupID in ('" + string.Join("','", GroupIDs.ToArray()) + "'))";

                if (FromDate != ToDate)
                {
                    if (ToDate != null)
                    {
                        command += " and (EGr.EffectiveDate  <= '" + new CommonDTO().HijriToGreg(ToDate.Value, "yyyyMMdd") + "' ) ";
                    }
                    if (FromDate != null)
                    {
                        command += " AND ('" + new CommonDTO().HijriToGreg(FromDate.Value, "yyyyMMdd") + "' <= ISNULL(EGr.EndDate, '9999-12-31'))";
                    }
                }
                if (FromDate == ToDate)
                {
                    if (FromDate != null && ToDate != null)
                    {
                        command += " AND ('" + new CommonDTO().HijriToGreg(ToDate.Value, "yyyyMMdd") + "' BETWEEN EGr.EffectiveDate AND ISNULL(EGr.EndDate,'9999-12-31')) ";
                    }

                }
                command += " ) AND ";
            }
            command += "   ETH.IsCurrent=1" + " AND (EG.IsCurrent IS NULL OR EG.IsCurrent = 1  OR (e.TerminationDate IS NOT NULL AND EG.IsCurrent = 0 and EG.EffectiveDate=(select max(EG1.EffectiveDate) from EmployeeGrade EG1 where EG1.PersonID = p.PersonID))) " +
            " AND ph.IsPrimary = 1 " +
            " AND CONVERT(DATE,ph.SinceDate) = (Select  MAX(CONVERT(DATE, ph1.SinceDate)) from PositionHistory ph1 where  (ph1.OccupiedBy = ph.OccupiedBy)" +
            " and ph1.IsPrimary = 1" + (ToDate != null ? " and CONVERT(DATE,ph1.SinceDate)<= '" + new CommonDTO().HijriToGreg(ToDate.Value.Date, "yyyyMMdd") + "' " : null) + ")" +
            " AND ph.OccupiedBy IS NOT NULL " + " AND ph.VacancyStatusID = 3 ";

            command += (GenderIDs != null && GenderIDs.Count > 0) ? " AND p.GenderID IN ('" + string.Join("','", GenderIDs.ToArray()) + "' )" : null;

            if (!string.IsNullOrEmpty(CostCenterID))
            {
                command += commmandCostCenterWhere;
            }
            command +=
            (ToDate != null ? " and e.HireDate<= '" + new CommonDTO().HijriToGreg(ToDate.Value.Date, "yyyyMMdd") + "'" : null) +
            (FromDate != null ? " and ( e.TerminationDate is null or e.TerminationDate >= '" + new CommonDTO().HijriToGreg(FromDate.Value.Date, "yyyyMMdd") + "')" : null) +
            (ToDate != null ? " and ( e.TerminationDate is null or e.TerminationDate <= '" + new CommonDTO().HijriToGreg(ToDate.Value.Date, "yyyyMMdd") + "')" : null) +
            ((JobTitleIDs.Count > 0) ? " and (j.JobID in ('" + string.Join("','", JobTitleIDs.ToArray()) + "'))" : null) +
            ((JobCategoryIDs.Count > 0) ? " and (j.jobcategoryid in ('" + string.Join("','", JobCategoryIDs.ToArray()) + "'))" : null) +
            ((JobLevelIDs.Count > 0) ? " and (j.JobLevelID in ('" + string.Join("','", JobLevelIDs.ToArray()) + "'))" : null) +
            ((EmploymentTypeIDs != null && EmploymentTypeIDs.Count > 0) ? " and (ET.EmploymentTypeID in ('" + string.Join("','", EmploymentTypeIDs.ToArray()) + "'))" : null) +
            ((GradeIDs != null && GradeIDs.Count > 0) ? " and (G.GradeID in ('" + string.Join("','", GradeIDs.ToArray()) + "'))" : null) +
            ((NationalityIDs != null && NationalityIDs.Count > 0) ? " and (pn.NationalityID in ('" + string.Join("','", NationalityIDs.ToArray()) + "'))" : null) +
            ((fromHireDate != null && ToHireDate != null) ? " and e.HireDate between '" + new CommonDTO().HijriToGreg(fromHireDate.Value, "yyyyMMdd") + "' and '" + new CommonDTO().HijriToGreg(ToHireDate.Value, "yyyyMMdd") + "'" : null) +
            ((fromTerminationDate != null && ToTerminationDate != null) ? " and e.TerminationDate between '" + new CommonDTO().HijriToGreg(fromTerminationDate.Value, "yyyyMMdd") + "' and '" + new CommonDTO().HijriToGreg(ToTerminationDate.Value, "yyyyMMdd") + "'" : null);

            command += " order by e.EmployeeID,p.FirstName";

            string strConnection = CommonDTO.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(strConnection))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(command, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _lstEmp.Add(new EmployeeFilter()
                    {
                        personID = ((reader.IsDBNull(0)) ? null : (String)reader.GetString(0)),
                        employeeID = ((reader.IsDBNull(1)) ? null : (String)reader.GetString(1)),
                        firstName = ((reader.IsDBNull(2)) ? null : (String)reader.GetString(2)),
                        lastName = ((reader.IsDBNull(3)) ? null : (String)reader.GetString(3)),
                        hireDate = reader.GetDateTime(4),
                        terminationDate = ((reader.IsDBNull(5)) ? null : (DateTime?)reader.GetDateTime(5)),
                        jobTitle = ((reader.IsDBNull(6)) ? null : (String)reader.GetString(6)),
                        organizationUnit = ((reader.IsDBNull(7)) ? null : (String)reader.GetString(7)),
                        birthDate = ((reader.IsDBNull(8)) ? null : (DateTime?)reader.GetDateTime(8)),
                        EmploymentType = ((reader.IsDBNull(9)) ? null : (String)reader.GetString(9)),
                        IsNSSFCovered = ((reader.IsDBNull(10)) ? false : (bool)reader[10]),
                        OU1 = ((reader.IsDBNull(11)) ? null : (String)reader.GetString(11)),
                        OU1SinceDate = ((reader.IsDBNull(12)) ? null : (DateTime?)reader.GetDateTime(12)),
                        OU2 = ((reader.IsDBNull(13)) ? null : (String)reader.GetString(13)),
                        OU2SinceDate = ((reader.IsDBNull(14)) ? null : (DateTime?)reader.GetDateTime(14)),
                        OU3 = ((reader.IsDBNull(15)) ? null : (String)reader.GetString(15)),
                        OU3SinceDate = ((reader.IsDBNull(16)) ? null : (DateTime?)reader.GetDateTime(16)),
                        fatherName = ((reader.IsDBNull(17)) ? null : (String)reader.GetString(17)),
                    });

                }
                conn.Close();

            }
            return _lstEmp;
        }
    }

    public class CommonDTO
    {
        public string HijriToGreg(DateTime DateTimeValue, string FormatString)
        {
            CultureInfo arCul = null;
            CultureInfo enCul = null;
            UmAlQuraCalendar calUmAlQura = null;

            string[] allFormats ={"yyyy/MM/dd","yyyy/M/d",
            "dd/MM/yyyy","d/M/yyyy",
            "dd/M/yyyy","d/MM/yyyy","yyyy-MM-dd",
            "yyyy-M-d","dd-MM-yyyy","d-M-yyyy",
            "dd-M-yyyy","d-MM-yyyy","yyyy MM dd",
            "yyyy M d","dd MM yyyy","d M yyyy",
            "dd M yyyy","d MM yyyy","yyyyMMdd",
            "yyyyMMdd HH:mm:ss","yyyyMMdd hh:mm:ss tt",
             "HH:mm","ddMMyyyy","s","yyyy-MM-dd HH:mm:ss.fff","yyyy-MM-dd HH:mm","yyyy-MM-dd","yyyy-MM-dd HH:mm:ss","dd-MMM-yyyy","dd/MMM/yyyy"};

            try
            {
                string strDateTime = DateTimeValue.ToString(FormatString);
                if (strDateTime.Length <= 0)
                {
                    return "";
                }
                //arCul = new CultureInfo("ar-SA");
                //enCul = new CultureInfo("en-US");
                //arCul.DateTimeFormat.Calendar = new UmAlQuraCalendar(); // To be sure
                //DateTimeStyles styles = DateTimeStyles.None;


                if (System.Globalization.CultureInfo.CurrentCulture.Name == "ar-SA")
                {
                    DateTime temp;
                    if (arCul == null)
                    {
                        arCul = new CultureInfo("ar-SA");
                    }
                    if (enCul == null)
                    {
                        enCul = new CultureInfo("en-US");
                    }
                    if (calUmAlQura == null)
                    {
                        calUmAlQura = new UmAlQuraCalendar(); // To be sure
                    }
                    arCul.DateTimeFormat.Calendar = calUmAlQura;
                    DateTimeStyles styles = DateTimeStyles.None;
                    if (DateTime.TryParse(DateTimeValue.ToString(), arCul, styles, out temp))
                    {
                        DateTime tempDate = DateTime.ParseExact(strDateTime, allFormats, arCul.DateTimeFormat, DateTimeStyles.AllowWhiteSpaces);
                        return tempDate.ToString(FormatString, enCul.DateTimeFormat);
                    }
                    else
                    {
                        return strDateTime;
                    }
                }
                else
                {
                    return strDateTime;
                }
            }
            catch (Exception ex)
            {
                return DateTimeValue.ToString();
            }
        }

        public static string GetConnectionString()
        {
            return "server=localhost;Password=sasql1$$;Persist Security Info=True;User ID=sa;Initial Catalog=Adir";
            //  return "Password=sasql;Persist Security Info=True;User ID=sa;Initial Catalog=MEA;Data Source=.";
        }
    }

    public class EmployeeFilter
    {
        public int DependancyCount { get; set; }

        public int TotalRecords { get; set; }

        public string personID { get; set; }

        public string employeeID { get; set; }

        public DateTime hireDate { get; set; }

        public string jobTitle { get; set; }

        public string firstName { get; set; }

        public string fatherName { get; set; }

        public string lastName { get; set; }

        public string organizationUnit { get; set; }

        public DateTime? terminationDate { get; set; }

        public DateTime? birthDate { get; set; }

        public string PositionID { get; set; }

        public string FullName { get { return firstName + " " + lastName; } }

        public string FullNameWithID { get { return employeeID + " " + firstName + " " + lastName; } }

        public string EmploymentType { get; set; }

        public string GradeName { get; set; }

        public string SupervisorName { get; set; }

        private int missingPic { get; set; }

        public bool MissingPic
        {
            get
            {
                return (missingPic == 1);
            }
        }

        public bool IsNSSFCovered { get; set; }

        public bool IsHusbandInBank { get; set; }

        public string OU1 { get; set; }

        public DateTime? OU1SinceDate { get; set; }

        public string OU2 { get; set; }

        public DateTime? OU2SinceDate { get; set; }

        public string OU3 { get; set; }

        public DateTime? OU3SinceDate { get; set; }
    }
}
