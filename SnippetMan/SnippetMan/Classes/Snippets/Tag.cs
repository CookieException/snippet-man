﻿using System;

namespace SnippetMan.Classes.Snippets
{
    public enum TagType
    {
        TAG_WITHOUT_TYPE,
        TAG_PROGRAMMING_LANGUAGE
    }

    public class Tag
    {
        public int? Id { get; set; }
        public TagType Type { get; set; }
        public string Title { get; set; }
        public int? usedXTimes { get; set; }

        public bool IsEmpty { get => String.IsNullOrEmpty(Title); }

        public override string ToString()
        {
            return Title;
        }
        public override bool Equals(object obj)
        {
            return this.Id == (obj as Tag)?.Id;
        }
        public override int GetHashCode()
        {
            return this.Id?.GetHashCode() ?? 0;
        }
    }
}
