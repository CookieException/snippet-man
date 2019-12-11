using SnippetMan.Classes.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnippetMan.Classes
{
    public class SnippetInfo
    {
        public int? Id { get; set; }
        public string Titel { get; set; }
        public string Beschreibung { get; set; }
        public List<Snippets.Tag> Tags { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public SnippetCode SnippetCode { get; set; }

        public SnippetInfo withSnippetCodeUpdate()
        {
            IDatabaseDAO database = new SQLiteDAO();
            database.OpenConnection();
            SnippetCode = database.GetSnippetCode(this);
            database.CloseConnection();

            return this;
        }
    }
}