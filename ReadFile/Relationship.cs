using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadFile
{
    public class Relationship
    {
        public string ChildNo { get; set; }
        public string ParentNo { get; set; }
        public RelationshipType RelType { get; set; }
    }
}
