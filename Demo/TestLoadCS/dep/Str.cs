using System;
using System.Collections;
using System.Collections.Generic;

namespace Test
{
    /// <summary>
    /// 结构体类型的string
    /// </summary>    
    public struct Str : IEquatable<Str>
    {
        public static Str Empty = new Str(string.Empty, 0, 0);
        public static char[] S_TRIM_CHARS = new char[] { ' ', '\n', '\r' };

        public int _len;
        public int _offset;
        public string _str;

        public Str(string v)
        {
            _str = v;
            _offset = 0;
            if (_str == null)
                _len = 0;
            else
                _len = _str.Length;
        }

        public Str(string v, int offset, int len)
        {
            _str = v;
            _offset = offset;
            _len = len;
        }

        public Str Substr(int start_index)
        {
            return Substr(start_index, _len - start_index);
        }

        public Str Substr(int start_index, int length)
        {
            if (start_index < 0 || (start_index + length) > Length)
                return string.Empty;
            start_index += _offset;
            return new Str(_str, start_index, length);
        }

        public Str this[int start, int len]
        {
            get
            {
                return Substr(start, len);
            }
        }

        public void ToStr(System.Text.StringBuilder sb)
        {
            if (_str == null)
                return;
            if (_offset == 0 && _len == _str.Length)
            {
                sb.Append(_str);
                return;
            }

            for (int i = _offset; i < _offset + _len; ++i)
            {
                sb.Append(_str[i]);
            }
        }

        public override string ToString()
        {
            if (_str == null) return _str;
            if (_offset == 0 && _len == _str.Length) return _str;
            return _str.Substring(_offset, _len);
        }

        public Str TrimEnd()
        {
            int offset = _offset;
            int len = _len;
            for (int i = offset + len - 1; i >= offset; --i)
            {
                if (!char.IsWhiteSpace(_str[i]))
                    break;
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimEnd(char[] trim_chars)
        {
            int offset = _offset;
            int len = _len;
            for (int i = offset + len - 1; i >= offset; --i)
            {
                if (!_has(trim_chars, _str[i]))
                    break;
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimStart(char[] trim_chars)
        {
            int offset = _offset;
            int len = _len;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                if (!_has(trim_chars, _str[i]))
                {
                    offset = i;
                    break;
                }
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimStart()
        {
            int offset = _offset;
            int len = _len;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                if (!char.IsWhiteSpace(_str[i]))
                {
                    offset = i;
                    break;
                }
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str Trim(char[] trim_chars)
        {
            Str ret = TrimEnd(trim_chars);
            return ret.TrimStart(trim_chars);
        }

        public Str Trim()
        {
            Str ret = TrimEnd();
            return ret.TrimStart();
        }

        public StrEnumerable Split(char split_char, bool remove_empty)
        {
            return new StrEnumerable(this, split_char, remove_empty);
        }
        public StrEnumerable Split(char split_char)
        {
            return new StrEnumerable(this, split_char, false);
        }

        public void Split(char split_char, List<Str> out_list)
        {
            Split(split_char, out_list, false);
        }

        public void Split(char split_char, List<Str> out_list, bool remove_empty)
        {
            out_list.Clear();
            if (_str == null)
                return;
            if (_len == 0)
            {
                if (!remove_empty)
                    out_list.Add(this);
                return;
            }

            int start = _offset;
            int count = _len;
            for (; ; )
            {
                int index = _str.IndexOf(split_char, start, count);
                if (index < 0)
                {
                    Str f1 = new Str(_str, start, count);
                    if (remove_empty && f1.Length == 0)
                        break;

                    out_list.Add(f1);
                    break;
                }

                int count2 = index - start;
                Str f2 = new Str(_str, start, count2);
                if (!(remove_empty && f2.Length == 0))
                    out_list.Add(f2);

                count = count - count2 - 1;
                start = index + 1;
            }
        }

        public int LastIndexOf(char c)
        {
            return LastIndexOf(c, _len - 1);
        }

        public int LastIndexOf(char c, int offset)
        {
            if (_str == null)
                return -1;
            if (offset < 0)
                return -1;
            if (offset >= _len)
                return -1;

            int ret = _str.LastIndexOf(c, offset + _offset, offset + 1);
            if (ret < 0)
                return -1;
            return ret - _offset;
        }

        public int IndexOf(char c, int offset)
        {
            if (_str == null)
                return -1;
            if (offset < 0)
                return -1;
            if (offset >= _len)
                return -1;

            int ret = _str.IndexOf(c, offset + _offset, _len - offset);
            if (ret < 0)
                return -1;
            return ret - _offset;
        }

        public bool IsEmpty()
        {
            return _len == 0;
        }

        public int Length { get { return _len; } }
        public char this[int idx]
        {
            get
            {
                return _str[idx + _offset];
            }
        }

        public bool Equals(Str other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is string str_val)
            {
                Str str = new Str(str_val);
                return this == str;
            }
            else if (obj is Str str)
            {
                return this == str;
            }
            return false;
        }

        public int ParseInt32()
        {
            return (int)ParseInt64();
        }

        public uint ParseUInt32()
        {
            return (uint)ParseUInt64();
        }

        public long ParseInt64()
        {
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;
            long ret = 0;
            if (end_offset == start)
                return ret;
            bool is_neg = false;
            if (_str[start] == '-') //记录数字正负  
            {
                is_neg = true;
                start++;
            }

            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + (b - '0');
                else
                    break;
            }
            if (is_neg)
                return -ret;
            return ret;
        }

        public ulong ParseUInt64()
        {
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;
            ulong ret = 0;
            if (end_offset == start)
                return ret;

            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + b - '0';
                else
                    break;
            }
            return ret;
        }

        public double ParseDouble()
        {
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;

            double s = 0.0;
            if (end_offset == start)
                return s;

            double d = 10.0;
            bool is_neg = false;
            if (_str[start] == '-') //记录数字正负  
            {
                is_neg = true;
                start++;
            }

            bool is_dig = true; //是整数部分
            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                {
                    if (is_dig)
                        s = s * 10.0 + b - '0';
                    else
                    {
                        s = s + (b - '0') / d;
                        d *= 10.0;
                    }
                }
                else if (b == '.')
                {
                    if (is_dig)
                        is_dig = false;
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
            double v = s * (is_neg ? -1.0 : 1.0);
            if (end_offset == start)
                return v;

            if (_str[start] == 'E' || _str[start] == 'e')
            {
                start++;
                Str sub = Substr(start);
                int exp = sub.ParseInt32();
                return Math.Pow(10, exp) * v;
            }
            else
                return v;
        }

        public float ParseFloat()
        {
            return (float)ParseDouble();
        }

        public static bool operator ==(Str self, Str other)
        {
            if (self._len != other._len)
                return false;
            if (self._len == 0)
                return true;

            if (self._len == self._str.Length
                    && other._len == other._str.Length)
                return self._str == other._str;

            for (int i = 0, i_1 = self._offset, i_2 = other._offset
                        ; i < self._len
                        ; ++i, ++i_1, ++i_2)
            {
                if (self._str[i_1] != other._str[i_2])
                    return false;
            }
            return true;
        }

        public static bool operator !=(Str self, Str other)
        {
            if (self._len != other._len)
                return true;
            if (self._len == 0)
                return false;

            if (self._len == self._str.Length
              && other._len == other._str.Length)
                return self._str != other._str;

            for (int i = 0, i_1 = self._offset, i_2 = other._offset
                    ; i < self._len
                    ; ++i, ++i_1, ++i_2)
            {
                if (self._str[i_1] != other._str[i_2])
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash1 = 5381;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                hash1 = ((hash1 << 5) + hash1) ^ _str[i];
            }
            return hash1 * 1566083941;
        }

        public bool _has(char[] trim_chars, char c)
        {
            for (int i = 0; i < trim_chars.Length; ++i)
            {
                if (trim_chars[i] == c)
                    return true;
            }
            return false;
        }

        public static implicit operator Str(string v) { return new Str(v); }

        public static implicit operator string(Str v)
        {
            return v.ToString();
        }
    }

    public struct StrEnumerator : IEnumerator<Str>
    {
        public Str _val;
        public char _split;
        public int _start_index;
        public int _length;
        public bool _remove_empty;

        public StrEnumerator(ref Str str, char split, bool remove_empty)
        {
            _val = str;
            _split = split;
            _start_index = 0;
            _length = -1;
            _remove_empty = remove_empty;
        }

        public Str Current
        {
            get
            {
                if (_length == -1)
                    return string.Empty;

                return _val.Substr(_start_index, _length);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                Str ret = Current;
                return ret;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            for (; ; )
            {
                _start_index = _start_index + _length + 1;
                _length = -1;

                if (_start_index > _val.Length)
                    return false;

                int index = _val.IndexOf(_split, _start_index);
                if (index < 0)
                {
                    _length = _val.Length - _start_index;
                }
                else
                    _length = index - _start_index;

                if (_remove_empty && _length == 0)
                    continue;

                return true;
            }
        }

        public void Reset()
        {
            _start_index = 0;
            _length = -1;
        }
    }

    public struct StrEnumerable : IEnumerable<Str>
    {
        public char _split;
        public Str _str;
        public bool _remove_empty;
        public StrEnumerable(Str str, char split, bool remove_empty)
        {
            _str = str;
            _split = split;
            _remove_empty = remove_empty;
        }
        public StrEnumerator GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }
        IEnumerator<Str> IEnumerable<Str>.GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }
    }

    public static class StringExt
    {
        public static StrEnumerable SplitExt(this string self, char split)
        {
            return new StrEnumerable(self, split, false);
        }
    }


    public class StrComparer : IEqualityComparer<Str>
    {
        public static StrComparer _ = new StrComparer();

        public bool Equals(Str x, Str y)
        {
            return x == y;
        }
        public int GetHashCode(Str obj)
        {
            return obj.GetHashCode();
        }
    }
}
