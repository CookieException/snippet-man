using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnippetMan.Classes.Snippets
{
    enum TagType
    {
        TAG_WITHOUT_TYPE,
        TAG_PROGRAMMING_LANGUAGE
    }

    class Tag
    {
        public int Id { get; set; }
        public TagType Type { get; set; }
        public string Title { get; set; }
        public int? usedXTimes { get; set; }
    }
}
