using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using SnippetMan.Classes.Snippets;
using System.IO;

namespace SnippetMan.Classes.Database
{
    public class SQLiteDAO : IDatabaseDAO
    {
        private SQLiteConnection m_dbConnection;

        public static SQLiteDAO Instance { get; set; } = new SQLiteDAO();

        public bool OpenConnection()
        {
            string dbPath = "MyDatabase.SQLite";
            if (!File.Exists(dbPath))
                SQLiteConnection.CreateFile(dbPath);
            m_dbConnection = new SQLiteConnection("Data Source=" + dbPath + ";Version=3;");
            m_dbConnection.Open();
            execute("PRAGMA foreign_keys = ON;");

            execute(
                "create table if not exists snippetCode " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, imports LONGTEXT, code LONGTEXT)"
            );
            execute(
                "create table if not exists snippetInfo " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, titel VARCHAR(128), beschreibung VARCHAR(1024)" +
                ", creationDate DATETIME, lastEditDate DATETIME, favorite BOOLEAN, snippetCodeId INTEGER" +
                ", FOREIGN KEY('snippetCodeId') REFERENCES 'snippetCode' ('id') ON DELETE CASCADE)"
            );
            execute(
                "create table if not exists tag " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, title VARCHAR(128), type INTEGER NOT NULL )"
            );
            execute(
                "create table if not exists tag_snippetInfo " +
                "(id INTEGER PRIMARY KEY AUTOINCREMENT, snippetInfoId INTEGER NOT NULL, tagId INTEGER NOT NULL" +
                ", FOREIGN KEY('snippetInfoId') REFERENCES 'snippetInfo' ('id') ON DELETE CASCADE" +
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
            List<SnippetCode> result = selectSnippetCode("select * from snippetCode c inner join snippetInfo i on i.id = c.id where i.id=:id",
                new Dictionary<string, object> { { "id", parentInfo.Id } });

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
            List<SnippetInfo> result = new List<SnippetInfo>();
            foreach (string tag in seachTags)
            {
                result.AddRange(selectSnippetInfo(
                    "SELECT snippetInfo.* from tag " +
                    "join tag_snippetInfo on tag.id = tag_snippetInfo.tagId " +
                    "join snippetInfo on tag_snippetInfo.snippetInfoId = snippetInfo.id " +
                    "where tag.title like '%:tag%';",
                    new Dictionary<string, object> {
                        {"tag",tag }
                    }));
            }

            return result;
        }

        public List<SnippetInfo> GetSnippetMetaByText(string searchText)
        {
            return selectSnippetInfo("select * from snippetInfo where titel like '%:searchText%' or beschreibung like '%:searchText%'", new Dictionary<string, object> {
                { "searchText", searchText }
            });
        }

        public List<SnippetInfo> GetSnippetMetaList()
        {
            return selectSnippetInfo("select * from snippetInfo").Select((x) => { x.Tags = GetTagsFromSnippetInfo(x); return x; }).ToList();
        }

        public SnippetInfo saveSnippet(SnippetInfo infoToSave)
        {
            // if we save a whole snippet with a code linked to it, we assume they belong together
            infoToSave.Tags = saveTags(infoToSave.Tags);

            Dictionary<string, object> dict;
            if (infoToSave.SnippetCode != null)
            {
                infoToSave.SnippetCode.Id = infoToSave.Id;
                infoToSave.SnippetCode = saveSnippetCode(infoToSave.SnippetCode);
                dict = new Dictionary<string, object>
                {
                    { "id", infoToSave.Id },
                    { "snippetCodeId", infoToSave.SnippetCode.Id },
                    { "titel", infoToSave.Titel },
                    { "beschreibung", infoToSave.Beschreibung },
                    { "lastEditDate", DateTime.Now },
                    { "favorite", infoToSave.Favorite }
                };
            }
            else
                dict = new Dictionary<string, object>
                {
                    { "id", infoToSave.Id },
                    { "titel", infoToSave.Titel },
                    { "beschreibung", infoToSave.Beschreibung },
                    { "lastEditDate", DateTime.Now },
                    { "favorite", infoToSave.Favorite }
                };

            if (infoToSave.Id.HasValue)
            {//Update
                dict.Add("creationDate", infoToSave.CreationDate);
                if (infoToSave.SnippetCode != null)
                    execute("update snippetInfo set titel = :titel" +
                            ", beschreibung = :beschreibung" +
                            ", creationDate = :creationDate" +
                            ", lastEditDate = :lastEditDate" +
                            ", favorite = :favorite" +
                            ", snippetCodeId = :snippetCodeId" +
                            " where id = :id"
                        , dict);
                else
                    execute("update snippetInfo set titel = :titel" +
                            ", beschreibung = :beschreibung" +
                            ", creationDate = :creationDate" +
                            ", lastEditDate = :lastEditDate" +
                            ", favorite = :favorite" +
                            " where id = :id"
                        , dict);
            }
            else
            {//Insert
                dict.Add("creationDate", DateTime.Now);
                execute("insert into snippetInfo " +
                        "(titel, beschreibung, creationDate, lastEditDate, favorite, snippetCodeId)" +
                        " values " +
                        "(:titel, :beschreibung, :creationDate, :lastEditDate, :favorite, :snippetCodeId)"
                    , dict);

                infoToSave.Id = (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
            }

            saveTagsToSnippetInfo(infoToSave.Tags, infoToSave);

            return infoToSave;
        }

        public void deleteSnippet(SnippetInfo infoToDelete)
        {
            SnippetCode dbSnippetCodeToDelete = GetSnippetCode(infoToDelete);
            if (dbSnippetCodeToDelete != null)
                execute("DELETE FROM snippetCode where snippetCode.id = :snippetCodeId", new Dictionary<string, object> { { "snippetCodeId", dbSnippetCodeToDelete.Id } });
        }

        public List<Tag> GetTags(string searchText, TagType tagType)
        {
            string sql = "";
            if (searchText == "")
                sql = "select * from tag where type = :tagType";
            else
                sql = "select * from tag where title like :searchText AND type = :tagType";

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "searchText", searchText },
                { "tagType", tagType }
            };
            return selectTag(sql, dict);
        }

        public List<Tag> GetTagsFromSnippetInfo(SnippetInfo snippetInfo)
        {
            string sql = "select tag.id, tag.title, tag.type from tag_snippetInfo left join tag on tag_snippetInfo.tagId = tag.id where tag_snippetInfo.snippetInfoId = :snippetId";

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "snippetId", snippetInfo.Id }
            };

            return selectTag(sql, dict);
        }

        public Tag GetTagById(int id)
        {
            return selectTag("select * from tag where id = :id", new Dictionary<string, object> { { "id", id } }).First();
        }

        private void saveTagsToSnippetInfo(List<Tag> tags, SnippetInfo snippetInfo) // TODO dont save if link already exists
        {
            // Delete the existing Tag <-> snippetInfo connections
            execute("delete from tag_snippetInfo where snippetInfoId = :snippetInfoId", new Dictionary<string, object> {
                {":snippetInfoId", snippetInfo.Id }
            });

            execute("BEGIN TRANSACTION");

            // Save connection between tags and snippetInfo
            foreach (Tag tag in tags)
            {
                if (tag.Title == "" || !tag.Id.HasValue)
                    continue;

                Dictionary<string, object> dict;

                if (!tag.Id.HasValue)
                {
                    Tag dbTag = doesTagTitleExist(tag);

                    if (dbTag != null)
                    {
                        dict = new Dictionary<string, object>
                        {
                            {":snippetInfoId", snippetInfo.Id.Value },
                            {":tagId", dbTag.Id.Value }
                        };
                    }
                    else continue;
                }
                else
                {
                    dict = new Dictionary<string, object>
                    {
                        {":snippetInfoId", snippetInfo.Id.Value },
                        {":tagId", tag.Id.Value }
                    };
                }

                execute("insert into tag_snippetInfo (snippetInfoId, tagId) values (:snippetInfoId, :tagId)", dict);
            }

            execute("COMMIT");
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
            if (tag.Title == "")
                return -1;


            Tag dbTag;
            if (/*tag.Id.HasValue || */(dbTag = doesTagTitleExist(tag)) != null)
            { // Update
                //Dictionary<string, object> dict = new Dictionary<string, object>
                //{
                //    {"id", dbTag.Id.Value },
                //    {"title", tag.Title },
                //    {"type",tag.Type }
                //};

                //execute("update tag set" +
                //    " title = :title," +
                //    " type = :type" +
                //    " where id = :id", dict);
                return dbTag.Id.Value;
            }
            else
            { // Insert
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    {"id", tag.Id },
                    {"title", tag.Title },
                    {"type",tag.Type }
                };

                execute("insert into tag (title,type) values (:title, :type)", dict);
                return (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
            }
        }

        private Tag doesTagTitleExist(Tag tag)
        {
            string sql = "select * from tag where title = :searchText AND type = :tagType";

            Dictionary<string, object> dict = new Dictionary<string, object> {
                { "searchText", tag.Title },
                { "tagType", tag.Type }
            };

            List<Tag> tags = selectTag(sql, dict);

            if (tags.Count > 0)
                return tags.First();
            else
                return null;
        }

        private SnippetCode saveSnippetCode(SnippetCode infoToSave)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"id", infoToSave.Id },
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
                    LastEditDate = dr.GetDateTime(4),
                    Favorite = dr.GetBoolean(5),
                    //Tags = new List<Tag>() { new Tag() { Title = "C#", Type = TagType.TAG_PROGRAMMING_LANGUAGE } } //TODO: fill with real values //TODO wird das überhaupt gebraucht?
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
