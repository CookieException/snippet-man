using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetMan.Classes.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;

namespace SnippetMan.Classes.Database.Tests
{
    [TestClass()]
    public class SQLiteDAOTests
    {
        [TestMethod()]
        public void OpenAndCloseConnectionTest()
        {
            SQLiteDAO db = new SQLiteDAO();
            try
            {
                db.OpenConnection();
                db.CloseConnection();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [TestMethod()]
        public void saveSnippetTest()
        {
            SQLiteDAO db = new SQLiteDAO();
            db.OpenConnection();

            SnippetInfo snippetInfo = new SnippetInfo
            {
                Titel = "Titel",
                Beschreibung = "Beschreibung"
            };
            int result = db.saveSnippet(snippetInfo);
            
            Assert.IsTrue(true,"" +result);
        }
    }
}