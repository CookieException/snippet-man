using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

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
                ", creationDate DATETIME, lastEditDate DATETIME, snippetCodeId INTEGER " +
                ", FOREIGN KEY('snippetCodeId') REFERENCES 'snippetCode' ('id'))"
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
                new Dictionary<string, object> {{"id", parentInfo.Id}});

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
                new Dictionary<string, object> {{"id", id}});

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

        public int saveSnippet(SnippetInfo infoToSave)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "id", infoToSave.Id },
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
                        ", lastEditDate = :lastEditDate " +
                        "where id = :id"
                    , dict);
            }
            else
            {//Insert
                dict.Add("creationDate", DateTime.Now);
                execute("insert into snippetInfo " +
                        "(titel, beschreibung, creationDate, lastEditDate)" +
                        " values " +
                        "(:titel, :beschreibung, :creationDate, :lastEditDate)"
                    , dict);

            }
            return (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
        }

        public int saveSnippetCode(SnippetCode infoToSave) // TODO woher soll ich wissen bei welchem snippetInfo ich das speichern soll?
        {
            // TODO Snippet code Speichern
            //TODO Update snippetInfo Fremdschlüssel
            throw new NotImplementedException();
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
                    Imports = dr.GetString(1),
                    Code = dr.GetString(2)
                };

                snippetCodeList.Add(snippetCode);
            }

            return snippetCodeList;
        }
    }
}
