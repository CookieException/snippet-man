namespace SnippetMan.Classes
{
    public class SnippetCode
    {
        public int? Id { get; set; }
        public string Imports { get; set; }
        public string Code { get; set; }

        public SnippetCode()
        {
        }

        public SnippetCode(string imports, string code)
        {
            Imports = imports;
            Code = code;
        }

        public void CopySnippet()
        {
            System.Windows.Clipboard.SetText(Imports + System.Environment.NewLine + Code);
        }
    }
}