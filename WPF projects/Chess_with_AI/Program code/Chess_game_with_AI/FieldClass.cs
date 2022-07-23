using System;

namespace Chess_game_with_AI
{
    public class FieldClass
    {
        public int row { get; private set; }
        public int column { get; private set; }

        public FieldClass()
        {
            this.row = 0;
            this.column = 0;
        }

        public FieldClass(int column, int row)
        {
            this.row = row;
            this.column = column;
        }

        public FieldClass(FieldClass fieldClass)
        {
            this.row = fieldClass.row;
            this.column = fieldClass.column;
        }

        public override bool Equals(object obj)
        {
            FieldClass obj2 = obj as FieldClass;

            if (obj2 == null)
                return false;

            return this.row == obj2.row && this.column == obj2.column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(row, column);
        }
    }
}
