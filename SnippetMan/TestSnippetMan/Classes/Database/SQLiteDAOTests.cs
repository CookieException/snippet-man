using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetMan.Classes.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SQLite;
using SnippetMan.Classes.Snippets;

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
        public void SaveCompleteSnippetTest()
        {
            SQLiteDAO db = new SQLiteDAO();
            db.OpenConnection();

            Tag progTag = new Tag
            {
                Title = "Progsprache",
                Type = TagType.TAG_PROGRAMMING_LANGUAGE
            };

            Tag tag = new Tag
            {
                Title = "Tag",
                Type = TagType.TAG_WITHOUT_TYPE
            };

            List<Tag> tags = new List<Tag> { progTag, tag };

            SnippetCode snippetCode = new SnippetCode
            {
                Imports = "Oh schau, ein Import!",
                Code = "Ein Stück Code\nSogar mit Absatz!!"
            };

            SnippetInfo snippetInfo = new SnippetInfo
            {
                Titel = "Test Snippet",
                Beschreibung = "Na ein Test Snippet halt,\nwie immer ohne viel Inhalt!",
                Tags = tags,
                SnippetCode = snippetCode
            };

            db.saveSnippet(snippetInfo);

            db.CloseConnection();

            Assert.Fail("Manueller Check! Schon verknüpfte Tags werden nochmal verknüpft");
        }

        //[TestMethod()]
        //public void saveSnippetTest()
        //{
        //    SQLiteDAO db = new SQLiteDAO();
        //    db.OpenConnection();

        //    SnippetInfo snippetInfo = new SnippetInfo
        //    {
        //        Titel = "Titel",
        //        Beschreibung = "Beschreibung"
        //    };
        //    int result = db.saveSnippet(snippetInfo);

        //    SnippetInfo dbSnippetInfo = db.GetSnippetMetaById(result);
        //    //Assert.AreSame(snippetInfo,db.GetSnippetMetaById(result));
        //    Assert.IsNotNull(dbSnippetInfo);
        //    Assert.AreEqual(snippetInfo.Titel, dbSnippetInfo.Titel);
        //    Assert.AreEqual(snippetInfo.Beschreibung, dbSnippetInfo.Beschreibung);
        //    Assert.IsNotNull(dbSnippetInfo.Id);
        //    Assert.IsNotNull(dbSnippetInfo.CreationDate);
        //    Assert.IsNotNull(dbSnippetInfo.LastEditDate);
        //}

        //[TestMethod()]
        //public void GetSnippetMetaByIdAndSaveSnippetTest()
        //{
        //    SQLiteDAO db = new SQLiteDAO();
        //    db.OpenConnection();

        //    SnippetInfo snippetInfo = new SnippetInfo
        //    {
        //        Titel = "Titel",
        //        Beschreibung = "Beschreibung"
        //    };
        //    int result = db.saveSnippet(snippetInfo);

        //    SnippetInfo dbSnippetInfo = db.GetSnippetMetaById(result);
        //    //Assert.AreSame(snippetInfo,db.GetSnippetMetaById(result));
        //    Assert.IsNotNull(dbSnippetInfo);
        //    Assert.AreEqual(snippetInfo.Titel, dbSnippetInfo.Titel);
        //    Assert.AreEqual(snippetInfo.Beschreibung, dbSnippetInfo.Beschreibung);
        //    Assert.IsNotNull(dbSnippetInfo.Id);
        //    Assert.IsNotNull(dbSnippetInfo.CreationDate);
        //    Assert.IsNotNull(dbSnippetInfo.LastEditDate);
        //}

        //[TestMethod()]
        //public void GetTagTest()
        //{
        //    SQLiteDAO db = new SQLiteDAO();
        //    db.OpenConnection();

        //    Tag tag = new Tag
        //    {
        //        Title = "TestTag",
        //        Type = TagType.TAG_WITHOUT_TYPE
        //    };
        //    int result = db.saveTag(tag);

        //    Tag dbTag = db.GetTagById(result);

        //    Assert.IsNotNull(dbTag);
        //    Assert.AreEqual(tag.Title, dbTag.Title);
        //    Assert.AreEqual(tag.Type, dbTag.Type);
        //    Assert.IsNotNull(dbTag.Id);
        //}

        [TestMethod()]
        public void GetTagsTest()
        {
            SQLiteDAO db = SQLiteDAO.Instance;

            Tag tag = new Tag { Title = "TestTag", Type = TagType.TAG_WITHOUT_TYPE };

            SnippetCode snippetCode = new SnippetCode { };
            List<Tag> tags = new List<Tag>
            {
                tag
            };

            SnippetInfo snippetInfo = new SnippetInfo { Tags = tags, SnippetCode = snippetCode };

            db.saveSnippet(snippetInfo);

            Tag dbTag = db.GetTags("TestTag", TagType.TAG_WITHOUT_TYPE).First();

            Assert.IsNotNull(dbTag);
            Assert.AreEqual(tag.Title, dbTag.Title);
            Assert.AreEqual(tag.Type, dbTag.Type);
            Assert.IsNotNull(dbTag.Id);
        }
    }
}