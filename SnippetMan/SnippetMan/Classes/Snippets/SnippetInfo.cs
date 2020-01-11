using SnippetMan.Classes.Database;
using SnippetMan.Classes.Snippets;
using System;
using System.Collections.Generic;

namespace SnippetMan.Classes
{
    public class SnippetInfo
    {
        public int? Id { get; set; }
        public string Titel { get; set; }
        public string Beschreibung { get; set; }
        public string ProgrammingLanguage { get { return Tags.Find(t => t.Type == TagType.TAG_PROGRAMMING_LANGUAGE)?.Title ?? ""; } }
        public List<Tag> Tags { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public bool Favorite { get; set; }
        public SnippetCode SnippetCode { get; set; }

        public SnippetInfo withSnippetCodeUpdate()
        {
            SnippetCode = SQLiteDAO.Instance.GetSnippetCode(this);

            return this;
        }

        public override bool Equals(object obj)
        {
            return this.Id == ((SnippetInfo)obj).Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public static bool operator ==(SnippetInfo lhs, SnippetInfo rhs)
        {
            if (Object.ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            else if ((lhs is null) || (rhs is null))
            {
                // if both are null, reference equals already returns true
                return false;
            }
            else
            {
                return lhs.Equals(rhs);
            }
        }

        public static bool operator !=(SnippetInfo lhs, SnippetInfo rhs) => !(lhs == rhs);
    }
}