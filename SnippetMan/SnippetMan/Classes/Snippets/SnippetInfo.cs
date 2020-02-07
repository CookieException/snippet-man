using SnippetMan.Classes.Database;
using SnippetMan.Classes.Snippets;
using SnippetMan.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnippetMan.Classes
{
    public class SnippetInfo
    {
        public int? Id { get; set; }
        public string Titel { get; set; }
        public string Beschreibung { get; set; }

        /// <summary>
        /// Returns all tags used by this snippet.
        /// Use <see cref="RangeObservableCollection{T}.AddRange(IEnumerable{T}, bool)"/> to override the list completely
        /// </summary>
        public RangeObservableCollection<Tag> Tags { get; } 
            = new RangeObservableCollection<Tag>();

        /// <summary>
        /// Returns the currently set language tag.
        /// If no tag is marked as language tag yet, an empty tag will be returned
        /// </summary>
        public Tag LanguageTag
        {
            // if there is no language tag yet, return an empty one
            get => Tags.Where(t => t.Type == TagType.TAG_PROGRAMMING_LANGUAGE).FirstOrDefault() ?? new Tag() { Type = TagType.TAG_PROGRAMMING_LANGUAGE };
            set
            {
                // Find the language tag in all tags ..
                Tag lang = Tags.Where(t => t.Type == TagType.TAG_PROGRAMMING_LANGUAGE).FirstOrDefault();

                // .. remove it and ..
                if (lang != null)
                    Tags.Remove(lang);

                // .. add the new one
                Tags.Add(value);
            }
        }

        public DateTime CreationDate { get; set; }
        public DateTime LastEditDate { get; set; }
        public bool Favorite { get; set; }
        public SnippetCode SnippetCode { get; set; }

        public SnippetInfo save() => App.DatabaseInstance.saveSnippet(this);

        public void delete() => App.DatabaseInstance.deleteSnippet(this);

        public SnippetInfo withSnippetCodeUpdate()
        {
            SnippetCode = App.DatabaseInstance.GetSnippetCode(this);

            return this;
        }

        #region comparison methods
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
        #endregion
    }
}