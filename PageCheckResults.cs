using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSPA.upd8r
{
    public class PageCheckResults
    {
        [JsonIgnore()]
        public bool newPageExists { get; set; }

        public PageInformation page { get; set; }
        public int numberOfPages { get; set; }
    }
}
