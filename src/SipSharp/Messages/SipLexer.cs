namespace SipSharp.Parser
{
    internal class SipLexer
    {
        public bool IsDelimiter(char token)
        {
            switch (token)
            {
                case '(':
                case ')':
                case '<':
                case '>':
                case '@':
                case ',':
                case ';':
                case ':':
                case '\\':
                case '"':
                case '/':
                case '[':
                case ']':
                case '?':
                case '=':
                case '{':
                case '}':
                case ' ':
                case '\t':
                    return true;
            }
            return false;
        }

        public bool IsToken(char token)
        {
            if (char.IsLetterOrDigit(token))
                return true;
            switch (token)
            {
                case '-':
                case '.':
                case '!':
                case '%':
                case '*':
                case '_':
                case '+':
                case '`':
                case '\'':
                case '~':
                    return true;
            }
            return false;
        }

        public bool IsWord(char token)
        {
            if (char.IsLetterOrDigit(token))
                return true;

            switch (token)
            {
                case '-':
                case '.':
                case '!':
                case '%':
                case '*':
                case '_':
                case '+':
                case '`':
                case '\'':
                case '~':
                case '(':
                case ')':
                case '<':
                case '>':
                case ':':
                case '\\':
                case '"':
                case '/':
                case '[':
                case ']':
                case '?':
                case '{':
                case '}':
                    return true;
            }
            return false;
        }

        public bool Match(char token, string characters)
        {
            return true;
        }
    }
}