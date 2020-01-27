namespace SnippetMan.Classes
{
    public class SnippetCode
    {
        public int? Id { get; set; }
        public string Imports { get; set; }
        public string Code { get; set; }
        
        public SnippetCode(int? id = null, string imports = null, string code = null)
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