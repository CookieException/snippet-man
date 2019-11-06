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
            // TODO Assert ob snippet gespeichert
        }

        [TestMethod()]
        public void GetSnippetMetaByIdTest()
        {
            SQLiteDAO db = new SQLiteDAO();
            db.OpenConnection();

            SnippetInfo snippetInfo = new SnippetInfo
            {
                Titel = "Titel",
                Beschreibung = "Beschreibung"
            };
            int result = db.saveSnippet(snippetInfo);

            SnippetInfo dbSnippetInfo = db.GetSnippetMetaById(result);
            //Assert.AreSame(snippetInfo,db.GetSnippetMetaById(result));
            Assert.IsNotNull(dbSnippetInfo);
            Assert.AreEqual(snippetInfo.Titel,dbSnippetInfo.Titel);
            Assert.AreEqual(snippetInfo.Beschreibung,dbSnippetInfo.Beschreibung);
            Assert.IsNotNull(dbSnippetInfo.Id);
            Assert.IsNotNull(dbSnippetInfo.CreationDate);
            Assert.IsNotNull(dbSnippetInfo.LastEditDate);
        }
    }
}