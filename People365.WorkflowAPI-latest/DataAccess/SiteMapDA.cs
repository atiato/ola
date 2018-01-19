using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace People365.FoundationAPI.DataAccess
{
    public class SiteMapDA
    {
        string _connStr;

        public SiteMapDA(string connStr)
        {
            _connStr = connStr;
        }

        public List<SiteMap> GetSiteMapByCultureAndRoles()
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                using (SqlCommand cmd = new SqlCommand("proc_GetSiteMapByCultureIdAndDetailedRoles", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CultureID", "en");
                    cmd.Parameters.AddWithValue("@TimeFlag", 1);
                    cmd.Parameters.AddWithValue("@PayrollFlag", 1);
                    cmd.Parameters.AddWithValue("@HumanFlag", 1);
                    cmd.Parameters.AddWithValue("@StockFlag", 0);
                    cmd.Parameters.AddWithValue("@GeneralFlag", 0);
                    cmd.Parameters.AddWithValue("@FixedFlag", 1);
                    cmd.Parameters.AddWithValue("@ManufacturingFlag", 1);
                    cmd.Parameters.AddWithValue("@Role", "Super User");
                    cmd.Parameters.AddWithValue("@MenuTypeID", 1);
                    cmd.Parameters.AddWithValue("@PaySecurity", "B");
                    cmd.Parameters.AddWithValue("@TimeSecurity", "B");
                    cmd.Parameters.AddWithValue("@HumanSecurity", "B");
                    cmd.Parameters.AddWithValue("@MedicalSecurity", "B");
                    cmd.Parameters.AddWithValue("@MedicalFlag", 0);

                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<SiteMap> siteMaps = new List<SiteMap>();

                    while (reader.Read())
                    {
                        SiteMap siteMap = new SiteMap();
                        siteMap.Description = reader["Description"].ToString();
                        siteMap.SiteMapId = int.Parse(reader["ID"].ToString());
                        siteMap.Url = reader["Url"].ToString();
                        siteMap.ParentId = reader["parent"] != DBNull.Value ? int.Parse(reader["parent"].ToString()) : 0;

                        siteMaps.Add(siteMap);
                    }

                    conn.Close();

                    return siteMaps;
                }
            }
        }

        public UserInfo GetUserInfo(string username)
        {
            try
            {
                UserInfo user = new UserInfo();
                using (SqlConnection connection = new SqlConnection(_connStr))
                {
                    SqlCommand cmd = new SqlCommand("proc_getuserinfo", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    string roleIds = "";
                    string ParantRoles = "";
                    while (reader.Read())
                    {
                        user.UserName = reader["UserName"].ToString();
                        user.UserID = reader["UserId"].ToString();
                        user.PersonID = reader["PersonId"].ToString();
                        user.FullName = reader["FullName"].ToString();
                        roleIds = reader["RoleIds"].ToString();
                        ParantRoles = reader["ParentRoles"].ToString();
                    }
                    user.RoleIDs = roleIds.Split(',').ToList();
                    user.ParentRoles = ParantRoles.Split(',').ToList();
                }
                return user;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<SiteMap> GetSiteMap()
        {
            List<SiteMap> allSiteMap = GetSiteMapByCultureAndRoles();

            List<SiteMap> lstParentNodes = allSiteMap.FindAll(x => x.ParentId == 1);

            foreach (SiteMap siteMap in lstParentNodes)
            {
                List<SiteMap> childNodes = GetChildNodes(siteMap, allSiteMap);

                if (childNodes != null)
                {
                    siteMap.InnerNodes = new List<SiteMap>();
                    siteMap.InnerNodes.AddRange(childNodes);
                }
            }

            return lstParentNodes;
        }

        private List<SiteMap> GetChildNodes(SiteMap parentSideMap, List<SiteMap> lst)
        {
            List<SiteMap> retVal = new List<SiteMap>();

            List<SiteMap> childNodes = lst.FindAll(x => x.ParentId == parentSideMap.SiteMapId);

            foreach (SiteMap childNode in childNodes)
            {
                SiteMap childNodeNew = childNode;

                List<SiteMap> childNodesNew = GetChildNodes(childNodeNew, lst);
                if (childNodesNew != null && childNodesNew.Count > 0)
                {
                    childNodeNew.InnerNodes = new List<SiteMap>();
                    childNodeNew.InnerNodes.AddRange(childNodesNew);
                }

                retVal.Add(childNodeNew);
            }

            return retVal;
        }

    }

    public class UserInfo
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PersonID { get; set; }
        public string UserID { get; set; }
        public List<string> RoleIDs { get; set; }
        public List<string> ParentRoles { get; set; }
    }

    public class SiteMap
    {
        public int SiteMapId { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int ParentId { get; set; }
        public int SortingOrder { get; set; }
        public string Roles { get; set; }

        public List<SiteMap> InnerNodes { get; set; }
    }

    public class SiteMenuSettings
    {
        public bool Time { get; set; }
        public bool Payroll { get; set; }
        public bool Human { get; set; }
        public bool Stock { get; set; }
        public bool GeneralLedger { get; set; }
        public bool Fixed { get; set; }
        public bool Manufacturing { get; set; }

        public string MedicalSecurity { get; set; }
        public int MenuTypeId { get; set; }

        public string PayrollBasicAdvanced { get; set; }
        public string TimeBasicAdvanced { get; set; }
        public string HRModules { get; set; }
        public bool Medical { get; set; }
        //selectedRole, 
    }
}
