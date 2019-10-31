using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetMan.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnippetMan.Classes.Tests
{
    [TestClass()]
    public class SnippetCodeTest
    {
        [TestMethod()]
        public void CopySnippetTest()
        {
            SnippetCode sc = new SnippetCode(0, "import", "code");
            sc.CopySnippet();

            Assert.AreEqual("import"+ Environment.NewLine +"code", System.Windows.Clipboard.GetText());
        }

        [TestMethod()]
        public void WithEmptyLinesCopySnippetTest()
        {
            SnippetCode sc = new SnippetCode(0, "import" + Environment.NewLine, "code");
            sc.CopySnippet();

            Assert.AreEqual("import" + Environment.NewLine  + Environment.NewLine + "code", System.Windows.Clipboard.GetText());
        }
    }
}