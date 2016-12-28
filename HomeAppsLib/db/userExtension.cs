using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib.db
{
    public partial class user
    {
        public int? Age
        {
            get
            {
                if (!this.birthday.HasValue)
                    return null;
                else                
                {
                    int years = DateTime.Now.Year - this.birthday.Value.Year;
                    if (DateTime.Now.Month < this.birthday.Value.Month && DateTime.Now.Day < this.birthday.Value.Day)
                        years--;

                    return years;
                }
            }
 
        }
        public bool IsKid
        {
            get
            {
                if (this == null)
                    return false;

                return Age < 10;
            }
        }
    }
}
