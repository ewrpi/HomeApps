using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomeAppsLib.db
{
    public partial class NFL_forum
    {
        public string TopicDescription 
        {
            get
            {
                if (!this.ref_id.HasValue)
                    return this.title;
                else
                    return LibCommon.DBModel().NFL_forums.First(x => x.id == this.ref_id.Value).TopicDescription;
            }
        }
        public string DisplayComment
        {
            get
            {
                return this.comment.Trim().Replace(Environment.NewLine, "<br/>");
            }
        }
    }
}
