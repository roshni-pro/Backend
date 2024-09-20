using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularJSAuthentication.DataContracts.APIParams
{
    public class MurliPager
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public string Keyword { get; set; }
    }

    public class MurliList<T>
    {
        public List<T> StoryList { get; set; }
        public int Count { get; set; }
    }
}
