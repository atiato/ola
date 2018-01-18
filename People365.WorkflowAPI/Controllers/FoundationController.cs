using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using People365.FoundationAPI.DataAccess;

namespace People365.FoundationAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Foundation")]
    public class FoundationController : Controller
    {
        string _connStr;

        IConfiguration _iconfiguration;
        public FoundationController(IConfiguration iconfiguration)
        {
            _iconfiguration = iconfiguration;
            _connStr = iconfiguration.GetSection("Data").GetSection("ConnectionString").Value;
        }

        [HttpGet]
        [Route("GetSiteMap")]
        public List<SiteMap> GetSiteMap()
        {
            SiteMapDA siteMapDA = new SiteMapDA(_connStr);
            return siteMapDA.GetSiteMapByCultureAndRoles();
        }

        [HttpGet]
        [Route("GetUserInfo")]
        public UserInfo GetUserInfo(string username)
        {
            SiteMapDA siteMapDA = new SiteMapDA(_connStr);
            return siteMapDA.GetUserInfo(username);
        }

        [HttpGet]
        [Route("GetWorkflowTask/{personId}/{userid}")]
        public List<WorkflowTask> GetWorkflowTask(string personId, string userid)
        {
            FoundationDA foundationDA = new FoundationDA(_connStr);
            return foundationDA.GetWorkflowTaskByStatusAndPersonMobile(personId, userid, 1, "848ECD54-BF0B-4400-B842-7CA11559C5D8");
        }

        [HttpGet]
        [Route("GetRequestDetails/{instanceId}/{taskId}")]
        public WorkflowTask GetRequestDetails(string instanceId, string taskId)
        {
            FoundationDA foundationDA = new FoundationDA(_connStr);
            return foundationDA.GetRequestDetails(instanceId, taskId);
        }
    }
}