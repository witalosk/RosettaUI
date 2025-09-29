// ReSharper disable FieldCanBeMadeReadOnly.Global

using System;

namespace RosettaUI
{
    public struct ListViewOption
    {
        public static ListViewOption Default =>  new (true);

        public bool reorderable;
        public bool fixedSize;
        public bool header;
        public bool suppressAutoIndent;
        public Func<object> newItemFactory;

        public ListViewOption(bool reorderable, bool fixedSize = false, bool header = true, bool suppressAutoIndent = false, Func<object> newItemFactory = null) : this()
        {
            this.reorderable = reorderable;
            this.fixedSize = fixedSize;
            this.header = header;
            this.suppressAutoIndent = suppressAutoIndent;
            this.newItemFactory = newItemFactory;
        }
    }
}