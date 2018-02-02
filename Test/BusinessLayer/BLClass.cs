using DataLayer;
using System;
using System.Collections.Generic;

namespace BusinessLayer
{
    public class BLClass
    {
        public List<string> TestBL()
        {
            DAClass dAClass = new DAClass();
            return dAClass.TestDa();
        }
    }
}
