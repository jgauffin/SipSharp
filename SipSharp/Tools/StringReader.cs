using System;
using System.Collections.Specialized;

namespace SipSharp.Tools
{
    /// <summary>
    /// Reads stuff from a string
    /// </summary>
    public class StringReader
    {
        private readonly string _text;
        private int _pos;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringReader"/> class.
        /// </summary>
        /// <param name="text">Text string to read from.</param>
        public StringReader(string text)
        {
            _text = text;
            _pos = 0;
            SavedPosition = -1;
        }

        /// <summary>
        /// Current character.
        /// </summary>
        /// <remarks>
        /// <see cref="char.MinValue"/> if EOF.
        /// </remarks>
        public char Current
        {
            get { return _pos < _text.Length ? _text[_pos] : char.MinValue; }
        }

        /// <summary>
        /// Previous character.
        /// </summary>
        /// <remarks>
        /// <see cref="char.MinValue"/> if BOF.
        /// </remarks>
        public char Previous
        {
            get { return _pos > 0 ? _text[_pos - 1] : char.MinValue; }
        }

        private int SavedPosition { get; set; }

        /// <summary>
        /// Scans forward ignoring white spaces to see if the specified char is the next one.
        /// </summary>
        /// <param name="token">separator to look for.</param>
        /// <remarks>
        /// It stays on the found token if it was found.
        /// </remarks>
        /// <returns>Position if the token is next; otherwise -1.</returns>
        public bool IsNext(char token)
        {
            int savedPos = _pos;
            while (_pos < _text.Length)
            {
                if (char.IsWhiteSpace(Current))
                    ++_pos;

                if (Current == token)
                    return true;
                break;
            }

            _pos = savedPos;
            return false;
        }

        private bool IsNext(string delimiters)
        {
            int savedPos = _pos;
            while (_pos < _text.Length)
            {
                if (char.IsWhiteSpace(Current))
                    ++_pos;

                if (delimiters.IndexOf(Current) != -1)
                    return true;
                break;
            }

            _pos = savedPos;
            return false;
        }

        /// <summary>
        /// Move forward one character.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (_pos < _text.Length)
            {
                ++_pos;
                return true;
            }

            return false;
        }

        public string Read(char delimiter)
        {
            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                MoveNext();

            SavePosition();
            while (_pos < _text.Length)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiter))
                        return ReturnSaved(pos);
                }

                if (_text[_pos] == delimiter)
                    return ReturnSaved(_pos);
                ++_pos;
            }

            RestorePosition();
            return null;
        }

        public string Read(string delimiters)
        {
            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                MoveNext();

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiters))
                        return ReturnSaved(pos);
                }

                if (delimiters.IndexOf(Current) != -1)
                    return ReturnSaved(_pos);

                MoveNext();
            }

            RestorePosition();
            return null;
        }

        public string Read(char delimiter, char abortCharacter)
        {
            if (IsEOF)
                return null;

            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                MoveNext();

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiter))
                        return ReturnSaved(pos);
                }

                if (Current == abortCharacter)
                {
                    RestorePosition();
                    return null;
                }

                if (Current == delimiter)
                    return ReturnSaved(_pos);

                MoveNext();
            }

            RestorePosition();
            return null;
        }

        public string ReadToEnd(char delimiter)
        {
            if (IsEOF)
                return null;

            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                ++_pos;

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiter))
                        return ReturnSaved(pos);
                }

                if (Current == delimiter)
                    return ReturnSaved(_pos);
                MoveNext();
            }

            return ReturnSaved(_text.Length);
        }

        public string ReadToEnd(char delimiter, bool ignoreQuotedStrings)
        {
            if (IsEOF)
                return null;

            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                ++_pos;

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiter))
                        return ReturnSaved(pos);
                }
                if (inQuote && !ignoreQuotedStrings)
                {
                    MoveNext();
                    continue;
                }

                if (Current == delimiter)
                    return ReturnSaved(_pos);
                MoveNext();
            }

            return ReturnSaved(_text.Length);
        }

        public string ReadToEnd(string delimiters)
        {
            if (IsEOF)
                return null;

            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                ++_pos;

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    if (IsNext(delimiters))
                        return ReturnSaved(pos);
                }

                if (delimiters.IndexOf(Current) != -1)
                    return ReturnSaved(_pos);
                MoveNext();
            }

            return ReturnSaved(_text.Length);
        }

        public string ReadWord()
        {
            if (IsEOF)
                return null;

            bool inQuote = Current == '"' && Previous != '\\';
            if (inQuote)
                ++_pos;

            SavePosition();
            while (!IsEOF)
            {
                if (Current == '"' && Previous != '\\')
                {
                    int pos = _pos;
                    MoveNext();
                    return ReturnSaved(pos);
                }

                MoveNext();
            }

            RestorePosition();
            return null;
        }


        /// <summary>
        /// Parse all semicolon separated parameters.
        /// </summary>
        /// <param name="parameters"></param>
        public void ParseParameters(NameValueCollection parameters)
        {
            SkipWhiteSpaces();
            while (Current == ';')
            {
                if (!MoveNext())
                    return;

                string name = ReadToEnd("= \t;");
                if (name == null)
                    break;

                SkipWhiteSpaces();
                if (Current == '=' && !MoveNext())
                {
                    parameters.Add(name, string.Empty);
                    break;
                }

                SkipWhiteSpaces();
                string value = ReadToEnd(" ;\t");
                if (value == null)
                {
                    parameters.Add(name, string.Empty);
                    break;
                }
                parameters.Add(name, value);
                SkipWhiteSpaces();
            }
        }

        /// <summary>
        /// Restore a saved position.
        /// </summary>
        /// <exception cref="InvalidOperationException">Position have not been saved.</exception>
        private void RestorePosition()
        {
            if (SavedPosition == -1)
                throw new InvalidOperationException("Position have not been saved.");
            _pos = SavedPosition;
            SavedPosition = -1;
        }

        /// <summary>
        /// Return text from saved position to specified position.
        /// </summary>
        /// <param name="endPos"></param>
        /// <returns></returns>
        private string ReturnSaved(int endPos)
        {
            int pos = SavedPosition;
            SavedPosition = -1;
            return _text.Substring(pos, endPos - pos);
        }

        /// <summary>
        /// Save current position, can only be done once.
        /// </summary>
        /// <exception cref="InvalidOperationException">Position have already been saved.</exception>
        private void SavePosition()
        {
            if (SavedPosition != -1)
                throw new InvalidOperationException("Position have already been saved.");

            SavedPosition = _pos;
        }

        /// <summary>
        /// Ignore all white spaces.
        /// </summary>
        /// <returns>false if EOF; otherwise true.</returns>
        public bool SkipWhiteSpaces()
        {
            while (!IsEOF && char.IsWhiteSpace(Current))
                ++_pos;
            return !IsEOF;
        }

        public bool SkipWhiteSpaces(char extra)
        {
            while (!IsEOF && (char.IsWhiteSpace(Current) || Current == extra))
                ++_pos;
            return !IsEOF;
        }

        public bool IsEOF
        {
            get { return _pos >= _text.Length; }
        }

    }
}