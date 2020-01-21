using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnippetMan.Classes;
using SnippetMan.Classes.Snippets;

namespace SnippetMan
{
    public interface IDatabaseDAO
    {
        /// <summary>
        /// Open database connection
        /// </summary>
        /// <returns>True if connection was successfully opened</returns>
        bool OpenConnection();
        /// <summary>
        /// Close database connection
        /// </summary>
        /// <returns>True if connection was successfully closed</returns>
        bool CloseConnection();

        /// <summary>
        /// Get list of the meta information of all snippets
        /// </summary>
        /// <returns>List of all snippets as <see cref="SnippetInfo"/></returns>
        List<SnippetInfo> GetSnippetMetaList();

        /// <summary>
        /// Searches for a snippet by id and returns its meta data
        /// </summary>
        /// <param name="id">ID of the database entry</param>
        /// <returns><see cref="SnippetInfo"/> instance of the matching entry</returns>
        SnippetInfo GetSnippetMetaById(int id);

        /// <summary>
        /// Searches for one or multiple snippets by text and returns its meta data
        /// </summary>
        /// <param name="searchText">Text that has to occur in the snippet to get listed</param>
        /// <returns>List of <see cref="SnippetInfo"/> instances</returns>
        List<SnippetInfo> GetSnippetMetaByText(string searchText);

        /// <summary>
        /// Searches for one or multiple snippets by one or multiple tags and returns their meta data
        /// </summary>
        /// <param name="searchText">Text that has to occur in the snippet tag list to get listed</param>
        /// <returns>List of <see cref="SnippetInfo"/> instances</returns>
        List<SnippetInfo> GetSnippetMetaByTag(List<string> seachTags);

        /// <summary>
        /// Gets the code of a snippet
        /// </summary>
        /// <param name="parentInfo">Parent meta data object of class <see cref="SnippetInfo"/></param>
        /// <returns><see cref="SnippetCode"/> instance</returns>
        SnippetCode GetSnippetCode(SnippetInfo parentInfo);

        /// <summary>
        /// Saves a complete given snippet
        /// </summary>
        /// <param name="infoToSave">Information of the snippet as <see cref="SnippetInfo"/></param>
        /// <returns>SnippetInfo with all ids set</returns>
        SnippetInfo saveSnippet(SnippetInfo infoToSave);

        /// <summary>
        /// Deletes a given Snippet, only the ID has to be filled
        /// </summary>
        /// <param name="infoToDelete">Information of the snippet as <see cref="SnippetInfo"/></param>
        /// <returns>void</returns>
        void deleteSnippet(SnippetInfo infoToDelete);

        /// <summary>
        /// Returns already used/ saved Tags for Autocompletion when adding Tags to a Snippet
        /// </summary>
        /// <param name="searchText">Searchtext to match</param>
        /// <param name="tagType">Tag Type to Filter for example for Programming Languages</param>
        /// <returns>All matching Tags</returns>
        List<Tag> GetTags(string searchText, TagType tagType);

        /// <summary>
        /// Returns a Tag with that Id
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Tag GetTagById(int id);
    }
}
