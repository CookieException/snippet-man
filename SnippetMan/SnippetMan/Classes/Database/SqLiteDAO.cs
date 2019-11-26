﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using SnippetMan.Classes.Snippets;

namespace SnippetMan.Classes.Database
{
    public class SQLiteDAO : IDatabaseDAO
    {
        private SQLiteConnection m_dbConnection;
        public bool OpenConnection()
        {
            SQLiteConnection.CreateFile("MyDatabase.SQLite");
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.SQLite;Version=3;");
            m_dbConnection.Open();

            execute(
                "create table if not exists snippetCode " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, imports LONGTEXT, code LONGTEXT)"
            );
            execute(
                "create table if not exists snippetInfo " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, titel VARCHAR(128), beschreibung VARCHAR(1024)" +
                ", creationDate DATETIME, lastEditDate DATETIME, snippetCodeId INTEGER" +
                ", FOREIGN KEY('snippetCodeId') REFERENCES 'snippetCode' ('id'))"
            );
            execute(
                "create table if not exists tag " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, title VARCHAR(128), type INTEGER)"
            );
            execute(
                "create table if not exists tag_snippetInfo " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, snippetInfoId INTEGER, tagId INTEGER" +
                ", FOREIGN KEY('snippetInfoId') REFERENCES 'snippetInfo' ('id')" +
                ", FOREIGN KEY('tagId') REFERENCES 'tag' ('id'))"
            );

            return m_dbConnection.State == System.Data.ConnectionState.Open;
        }
        public bool CloseConnection()
        {
            m_dbConnection.Close();
            return m_dbConnection.State == System.Data.ConnectionState.Closed;
        }

        public SnippetCode GetSnippetCode(SnippetInfo parentInfo)
        {
            List<SnippetCode> result = selectSnippetCode("select * from snippetCode where id=:id",
                new Dictionary<string, object> { { "id", parentInfo.SnippetCode.Id } });

            if (result.Count > 0)
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public SnippetInfo GetSnippetMetaById(int id)
        {
            List<SnippetInfo> result = selectSnippetInfo("select * from snippetInfo where id=:id",
                new Dictionary<string, object> { { "id", id } });

            if (result.Count > 0)
            {
                return result.First();
            }
            else
            {
                return null;
            }
        }

        public List<SnippetInfo> GetSnippetMetaByTag(List<string> seachTags)
        {
            throw new NotImplementedException();
        }

        public List<SnippetInfo> GetSnippetMetaByText(string searchText)
        {
            throw new NotImplementedException();
        }

        public List<SnippetInfo> GetSnippetMetaList()
        {
            return selectSnippetInfo("select * from snippetInfo");
        }

        public SnippetInfo saveSnippet(SnippetInfo infoToSave)
        {
            infoToSave.Tags = saveTags(infoToSave.Tags);
            infoToSave.SnippetCode = saveSnippetCode(infoToSave.SnippetCode);

            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "id", infoToSave.Id },
                { "snippetCodeId", infoToSave.SnippetCode.Id },
                { "titel", infoToSave.Titel },
                { "beschreibung", infoToSave.Beschreibung },
                { "lastEditDate", DateTime.Now }
            };

            if (infoToSave.Id.HasValue)
            {//Update
                dict.Add("creationDate", infoToSave.CreationDate);
                execute("update snippetInfo set titel = :titel" +
                        ", beschreibung = :beschreibung" +
                        ", creationDate = :creationDate" +
                        ", lastEditDate = :lastEditDate" +
                        ", snippetCodeId = :snippetCodeId" +
                        "where id = :id"
                    , dict);
            }
            else
            {//Insert
                dict.Add("creationDate", DateTime.Now);
                execute("insert into snippetInfo " +
                        "(titel, beschreibung, creationDate, lastEditDate, snippetCodeId)" +
                        " values " +
                        "(:titel, :beschreibung, :creationDate, :lastEditDate, :snippetCodeId)"
                    , dict);

                infoToSave.Id = (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
            }

            saveTagsToSnippetInfo(infoToSave.Tags, infoToSave);

            return infoToSave;
        }

        public List<Tag> GetTags(string searchText, TagType tagType)
        {
            string sql = "select * from tag where title like :searchText AND type = :tagType";

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "searchText", searchText },
                { "tagType", tagType }
            };
            return selectTag(sql, dict);
        }

        public Tag GetTagById(int id)
        {
            return selectTag("select * from tag where id = :id", new Dictionary<string, object> { { "id", id } }).First();
        }

        private void saveTagsToSnippetInfo(List<Tag> tags, SnippetInfo snippetInfo) // TODO dont save if link already exists
        {
            execute("delete from tag_snippetInfo where snippetInfoId = :snippetInfoId",new Dictionary<string, object> {
                {":snippetInfoId", snippetInfo.Id }
            });

            foreach (Tag tag in tags)
            {
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    {":snippetInfoId", snippetInfo.Id },
                    {":tagId", tag.Id }
                };

                execute("insert into tag_snippetInfo (snippetInfoId, tagId) values (:snippetInfoId, :tagId)", dict);
            }
        }

        private List<Tag> saveTags(List<Tag> tags)
        {
            foreach (Tag tag in tags) // TODO prüfen welche Tags wirklich neu sind
            {
                tag.Id = saveTag(tag);
            }

            return tags;
        }

        private int saveTag(Tag tag)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"id", tag.Id },
                {"title", tag.Title },
                {"type",tag.Type }
            };

            if (tag.Id.HasValue)
            { // Update
                execute("update tag set" +
                    " title = :title," +
                    " type = :type" +
                    " where id = :id", dict);
                return tag.Id.Value;
            }
            else
            { // Insert
                execute("insert into tag (title,type) values (:title, :type)", dict);
                return (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
            }
        }

        private SnippetCode saveSnippetCode(SnippetCode infoToSave)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"infoId", infoToSave.Id },
                {"imports", infoToSave.Imports },
                {"code",infoToSave.Code }
            };

            if (infoToSave.Id.HasValue)
            {// Update
                execute("update snippetCode set" +
                    " imports = :imports" +
                    ", code = :code" +
                    " where id = :id", dict);

                return infoToSave;
            }
            else
            {// Insert
                execute("insert into snippetCode" +
                    " (imports, code)" +
                    " values" +
                    " (:imports, :code)", dict);

                infoToSave.Id = (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
                return infoToSave;
            }
        }

        private void execute(string sql) => execute(sql, new Dictionary<string, object>());

        private void execute(string sql, Dictionary<string, object> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            foreach (KeyValuePair<string, object> kvp in parameters)
                command.Parameters.Add(new SQLiteParameter(kvp.Key, kvp.Value));
            Console.WriteLine("Ausgeführter Befehl: " + command.CommandText);
            command.ExecuteNonQuery();
        }

        private List<SnippetInfo> selectSnippetInfo(string sql) => selectSnippetInfo(sql, new Dictionary<string, object>());
        private List<SnippetInfo> selectSnippetInfo(string sql, Dictionary<string, object> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            foreach (KeyValuePair<string, object> kvp in parameters)
                command.Parameters.Add(new SQLiteParameter(kvp.Key, kvp.Value));

            SQLiteDataReader dr = command.ExecuteReader();

            List<SnippetInfo> snippetInfoList = new List<SnippetInfo>();

            while (dr.Read())
            {
                SnippetInfo snippetInfo = new SnippetInfo
                {
                    Id = (int)dr.GetInt64(0),
                    Titel = dr.GetString(1),
                    Beschreibung = dr.GetString(2),
                    CreationDate = dr.GetDateTime(3),
                    LastEditDate = dr.GetDateTime(4)
                };

                snippetInfoList.Add(snippetInfo);
            }

            return snippetInfoList;
        }

        private List<SnippetCode> selectSnippetCode(string sql) => selectSnippetCode(sql, new Dictionary<string, object>());
        private List<SnippetCode> selectSnippetCode(string sql, Dictionary<string, object> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            foreach (KeyValuePair<string, object> kvp in parameters)
                command.Parameters.Add(new SQLiteParameter(kvp.Key, kvp.Value));

            SQLiteDataReader dr = command.ExecuteReader();

            List<SnippetCode> snippetCodeList = new List<SnippetCode>();

            while (dr.Read())
            {
                SnippetCode snippetCode = new SnippetCode
                {
                    Id = (int)dr.GetInt64(0),
                    Imports = dr.GetString(1),
                    Code = dr.GetString(2)
                };

                snippetCodeList.Add(snippetCode);
            }

            return snippetCodeList;
        }

        private List<Tag> selectTag(string sql) => selectTag(sql, new Dictionary<string, object>());
        private List<Tag> selectTag(string sql, Dictionary<string, object> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            foreach (KeyValuePair<string, object> kvp in parameters)
                command.Parameters.Add(new SQLiteParameter(kvp.Key, kvp.Value));

            SQLiteDataReader dr = command.ExecuteReader();

            List<Tag> tagList = new List<Tag>();

            while (dr.Read())
            {
                Tag tag = new Tag
                {
                    Id = (int)dr.GetInt64(0),
                    Title = dr.GetString(1),
                    Type = (TagType)dr.GetInt32(2)
                };

                tagList.Add(tag);
            }

            return tagList;
        }

    }
}
