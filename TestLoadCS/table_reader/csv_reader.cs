using System;
using System.Collections;
using System.Collections.Generic;
using Test;


namespace TestLoadCs.table_reader
{
    public enum E_CSV_TOKEN
    {
        word, //后面跟着的是 ,
        word_with_new_line, //后面跟着的是 换行符
        word_with_end, //后面跟着的结束符
        end,
        error,
    }

    public class CsvToken : IEnumerable<KeyValuePair<E_CSV_TOKEN, Str>>
    {
        public const char C_NEW_LINE = '\n';
        public const char C_RETURN = '\r';
        public const char C_COMMAS = ',';
        public const char C_QUOTES = '"';

        public string _buf;
        public int _offset;

        public CsvToken(byte[] buff)
        {
            string text = string.Empty;
            if (buff != null)
            {
                if (buff.Length >= 3 && buff[0] == 0xef && buff[1] == 0xbb && buff[2] == 0xbf)
                {
                    text = System.Text.Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
                }
                else
                    text = System.Text.Encoding.UTF8.GetString(buff);
            }
            _buf = text;
            _offset = 0;
        }


        public CsvToken(string buf)
        {
            _buf = buf;
            _offset = 0;
        }

        public bool IsEnd { get { return _offset >= _buf.Length; } }

        public E_CSV_TOKEN Next(out Str word)
        {
            //1. 检查是否已经到了结尾
            word = Str.Empty;

            int buf_len = _buf.Length;
            if (_offset >= buf_len)
                return E_CSV_TOKEN.end;

            //2. 读取第一个字符
            char first_char = _buf[_offset];
            switch (first_char)
            {
                case C_QUOTES: // " 碰到了这个
                    {
                        int end_index = _index_of_next_quotes(_buf, _offset + 1, out bool contain_double);
                        if (end_index == -1)
                        {
                            _offset = buf_len;
                            return E_CSV_TOKEN.error;
                        }

                        int start = _offset + 1;
                        int count = end_index - start;
                        word = new Str(_buf, start, count);

                        if (contain_double)
                            word = _format_str(word);

                        _offset = end_index + 1;

                        return _advance_split_symb();
                    }

                case C_NEW_LINE: // 换行符号
                case C_RETURN: //换行符号
                case C_COMMAS: //直接就是逗号
                    {
                        return _advance_split_symb();
                    }

                default: //普通的字符
                    {
                        int end_index = _index_of_str_end(_buf, _offset);
                        if (end_index == -1)
                        {
                            _offset = buf_len;
                            return E_CSV_TOKEN.error;
                        }

                        int start = _offset;
                        int count = end_index - start;
                        _offset = end_index;
                        word = new Str(_buf, start, count);
                        return _advance_split_symb();
                    }
            }
        }

        private E_CSV_TOKEN _advance_split_symb()
        {
            if (_offset >= _buf.Length)
                return E_CSV_TOKEN.word_with_end;

            char c = _buf[_offset];
            switch (c)
            {
                case C_COMMAS: // ,
                    {
                        _offset++;
                        return E_CSV_TOKEN.word;
                    }
                case C_NEW_LINE:// \n
                    {
                        _offset++;
                        if (_offset >= _buf.Length)
                            return E_CSV_TOKEN.word_with_new_line;

                        if (_buf[_offset] == C_RETURN) // \n\r                
                            _offset++;

                        return E_CSV_TOKEN.word_with_new_line;
                    }
                case C_RETURN: // \r
                    {
                        _offset++;
                        if (_offset >= _buf.Length)
                            return E_CSV_TOKEN.word_with_new_line;
                        if (_buf[_offset] == C_NEW_LINE) // \r\n
                            _offset++;
                        return E_CSV_TOKEN.word_with_new_line;
                    }

                default:
                    return E_CSV_TOKEN.error;
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<E_CSV_TOKEN, Str>>
        {
            public CsvToken _token;
            public KeyValuePair<E_CSV_TOKEN, Str> _cur;
            public Enumerator(CsvToken reader)
            {
                _token = reader;
                _cur = default;
            }

            public KeyValuePair<E_CSV_TOKEN, Str> Current => _cur;

            object IEnumerator.Current => _cur;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var r = _token.Next(out Str v);
                if (r == E_CSV_TOKEN.end)
                    return false;
                _cur = new KeyValuePair<E_CSV_TOKEN, Str>(r, v);
                return true;
            }

            public void Reset()
            {
                _token._offset = 0;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        private Str _format_str(Str str)
        {
            return str.ToString().Replace("\"\"", "\"");
        }

        private int _index_of_str_end(string buf, int index)
        {
            for (int i = index; i < buf.Length; i++)
            {
                char c = buf[i];
                if (c == C_COMMAS || c == C_NEW_LINE || c == C_RETURN)
                    return i;
            }
            return -1;
        }

        private int _index_of_next_quotes(string buf, int index, out bool contain_double)
        {
            contain_double = false;
            for (int i = index; i < buf.Length - 1; i++)
            {
                char c = buf[i];
                if (c == C_QUOTES)
                {
                    if (buf[i + 1] == C_QUOTES)
                    {
                        contain_double = true;
                        i++;
                    }
                    else
                        return i;
                }
            }
            return -1;
        }

        IEnumerator<KeyValuePair<E_CSV_TOKEN, Str>> IEnumerable<KeyValuePair<E_CSV_TOKEN, Str>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }

    public class CsvReader
    {
        public CsvToken Token;

        public CsvReader(byte[] buff)
        {
            Token = new CsvToken(buff);
        }

        public CsvReader(string buf)
        {
            Token = new CsvToken(buf);
        }

        public bool IsEnd => Token.IsEnd;

        public bool ReadRow(List<Str> out_list, bool clear_list = true)
        {
            if (clear_list)
                out_list.Clear();

            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = Token.Next(out Str word);
                switch (r)
                {
                    case E_CSV_TOKEN.word:
                        out_list.Add(word);
                        break;

                    case E_CSV_TOKEN.word_with_end:
                    case E_CSV_TOKEN.word_with_new_line:
                        out_list.Add(word);
                        return true;
                    case E_CSV_TOKEN.error:
                        return false;
                    case E_CSV_TOKEN.end:
                        return false;
                    default:
                        break;
                }
            }
        }

        public bool ReadWord(out Str word)
        {
            word = Str.Empty;
            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = Token.Next(out word);
                switch (r)
                {
                    case E_CSV_TOKEN.word:
                    case E_CSV_TOKEN.word_with_end:
                    case E_CSV_TOKEN.word_with_new_line:
                        return true;
                    case E_CSV_TOKEN.end:
                        return false;
                    case E_CSV_TOKEN.error:
                        return false;
                    default:
                        break;
                }
            }
        }

    }

    public class CsvWriter : IDisposable
    {
        public TextWriter _writer;
        public bool _first = true;
        public CsvWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public CsvWriter(string file_path)
            : this(file_path, System.Text.Encoding.UTF8)
        {
        }

        public CsvWriter(string file_path, System.Text.Encoding encoding)
        {
            _writer = new StreamWriter(file_path, false, encoding);
        }

        public void WriteWord(string word)
        {
            if (!_first)
                _writer.Write(',');
            _writer.Write(FormatCsvStr(word));
            _first = false;
        }

        public void WriteWord(params string[] words)
        {
            foreach (var p in words)
                WriteWord(p);
        }

        public void WriteWord<T>(IList<T> words, bool end_line)
        {
            foreach (var p in words)
                WriteWord(p.ToString());
            if (end_line)
                WriteLine();
        }

        public void WriteWord(IEnumerable e, bool end_line)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_line)
                WriteLine();
        }

        public void WriteWord<T>(IEnumerable<T> e, bool end_line)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_line)
                WriteLine();
        }

        public void WriteLine()
        {
            _writer.WriteLine();
            _first = true;
        }

        public void Close()
        {
            _writer?.Close();
            _writer = null;
        }

        public string FormatCsvStr(string s)
        {
            if (s == null)
                return string.Empty;

            bool contain_qutos = s.Contains('\"');
            if (!contain_qutos && !s.Contains('\n') && !s.Contains('\r') && !s.Contains(','))
                return s;

            if (contain_qutos)
                s = s.Replace("\"", "\"\"");

            return string.Concat("\"", s, "\"");
        }

        public void Dispose()
        {
            Close();
        }
    }
}
