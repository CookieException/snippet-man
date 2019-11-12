﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnippetMan.Classes;

namespace SnippetMan
{
    interface IDatabaseDAO
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
        /// <returns>ID of database entry</returns>
        int saveSnippet(SnippetInfo infoToSave);
        /// <summary>
        /// Saves the code a given snippet
        /// </summary>
        /// <param name="infoToSave">Information of the snippet as <see cref="SnippetInfo"/></param>
        /// <returns>ID of parent meta info database entry</returns>
        int saveSnippetCode(SnippetCode infoToSave);

    }
}