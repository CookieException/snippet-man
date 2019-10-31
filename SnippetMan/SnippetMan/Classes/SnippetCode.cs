namespace SnippetMan.Classes
{
    public class SnippetCode
    {
        public int Id { get; set; }
        public string Imports { get; set; }
        public string Code { get; set; }

        public SnippetCode(int id)
        {
            Id = id;
        }

        public SnippetCode(int id, string imports, string code)
        {
            Id = id;
            Imports = imports;
            Code = code;
        }

        public void CopySnippet()
        {
            System.Windows.Clipboard.SetText(Imports + System.Environment.NewLine + Code);
        }
    }
}