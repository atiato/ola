using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace People365.ComputeWokingDays.Controllers
{
    [Produces("application/json")]
    [Route("api/WorkingDays")]
    public class WorkingDaysController : Controller
    {
        [HttpGet]
        [Route("workingDay")]
        public PersonWorkingDay GetWorkingDays(string id)
        {
            int days = ComputeWorkingDays(id);

            PersonWorkingDay pwd = new PersonWorkingDay { PersonId = id, WorkingDays = days };

            return pwd;
        }

        private int ComputeWorkingDays(string id)
        {
            Random r = new Random(31);
            int x = r.Next();

            return x;
        }
    }

    public class PersonWorkingDay
    {
        public string PersonId { get; set; }
        public int WorkingDays { get; set; }
    }
}