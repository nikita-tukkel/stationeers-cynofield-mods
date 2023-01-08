using System;
using System.Collections;
using System.Collections.Generic;

namespace cynofield.mods.utils
{
    public class TextAnimator : IEnumerator<string>
    {
        internal string text;
        private string _current = "";
        internal int textIndex = -1;
        public TextAnimator(string text)
        {
            this.text = text;
        }

        public bool MoveNext()
        {
            if (string.IsNullOrEmpty(text))
                return false;

            textIndex++;
            if (textIndex >= text.Length)
                return false;

            while (textIndex < text.Length && text[textIndex] == ' ') textIndex++;

            // take whole rich text tag
            var c = text[textIndex];
            if (c == '<')
            {
                while (textIndex < text.Length && text[textIndex] != '>') textIndex++;
                textIndex = Math.Clamp(textIndex + 1, 0, text.Length - 1); // also take next char after tag
            }

            _current = text.Substring(0, textIndex + 1);
            return true;
        }

        public string Current => _current;
        object IEnumerator.Current => _current;

        public void Reset() { textIndex = -1; _current = ""; }
        public void Dispose() { }
    }
}
