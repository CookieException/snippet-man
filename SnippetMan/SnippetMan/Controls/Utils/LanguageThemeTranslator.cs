using HL.Manager;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnippetMan.Controls.Utils
{
    public class LanguageThemeTranslator
    {
        public static IHighlightingDefinition GetHighlighterByLanguageName(IHLTheme theme, string langName)
        {
            // at first, try the real name - but that does only work if the case is exactly matching
            IHighlightingDefinition def = theme.GetDefinition(langName);

            if (def != null)
                return def;

            langName = langName.ToLower().Replace("-", "").Replace("_", "");

            // then try it by extension
            def = theme.GetDefinitionByExtension("." + langName.Replace(".", ""));

            if (def != null)
                return def;


            string customName = "";
            switch (langName)
            {
                case "xmldoc":
                    customName = "XmlDoc";
                    break;
                case "c#":
                case "csharp":
                    customName = "C#";
                    break;
                case "javascript":
                    customName = "JavaScript";
                    break;
                case "xhtml":
                case "asp":
                    customName = "ASP/XHTML";
                    break;
                case "boo":
                    customName = "Boo";
                    break;
                case "coco":
                    customName = "coco";
                    break;
                case "css":
                    customName = "css";
                    break;
                case "cpp":
                case "c++":
                    customName = "C++";
                    break;
                case "ps":
                case "powershell":
                    customName = "PowerShell";
                    break;
                case "python":
                    customName = "Python";
                    break;
                case "tex":
                    customName = "TeX";
                    break;
                case "tsql":
                case "mssql":
                    customName = "TSQL";
                    break;
                case "vb":
                case "visualbasic":
                    customName = "VB";
                    break;

                // additional ones
                case "actionscript":
                case "as3":
                case "actionscript3":
                    customName = "ActionScript3";
                    break;
                case "f#":
                case "fsharp":
                    customName = "F#";
                    break;
                case "hlsl":
                    customName = "HLSL";
                    break;
                case "pascal":
                case "pasc":
                    customName = "Pascal";
                    break;
                case "ruby":
                case "rb":
                    customName = "Ruby";
                    break;
                case "scheme":
                    customName = "Scheme";
                    break;
                case "squirrel":
                    customName = "Squirrel";
                    break;
            }

            return theme.GetDefinition(customName);
        }
    }
}
