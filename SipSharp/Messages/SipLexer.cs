using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SipSharp.Parser
{
    	class SipLexer
	{
		public bool Match(char token, string characters)
		{
			return true;
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

	}
}
