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
            throw new NotImplementedException();
        }

        public SnippetInfo GetSnippetMetaById(int id)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public int saveSnippet(SnippetInfo infoToSave)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "id", infoToSave.Id },
                { "titel", infoToSave.Titel },
                { "beschreibung", infoToSave.Beschreibung },
                { "creationDate", DateTime.Now },
                { "lastEditDate", DateTime.Now }
            };
            if (infoToSave.Id.HasValue)
            {//Update
                execute("update snippetInfo set titel = :titel" +
                        ", beschreibung = :beschreibung" +
                        ", creationDate = :creationDate" +
                        ", lastEditDate = :lastEditDate " +
                        "where id = :id"
                    , dict);
            }
            else
            {//Insert
                execute("insert into snippetInfo " +
                        "(titel, beschreibung, creationDate, lastEditDate)" +
                        " values " +
                        "(:titel, :beschreibung, :creationDate, :lastEditDate)"
                    , dict);

            }
            return (int)m_dbConnection.LastInsertRowId; // TODO return ist int64
        }

        public int saveSnippetCode(SnippetCode infoToSave)
        {
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

        private object select(string sql, Dictionary<string, object> parameters)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            foreach (KeyValuePair<string, object> kvp in parameters)
                command.Parameters.Add(new SQLiteParameter(kvp.Key, kvp.Value));
            return command.ExecuteScalar();
        }
    }
}
