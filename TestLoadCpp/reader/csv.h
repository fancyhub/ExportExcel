#include <string>
#include <vector>

namespace Test
{
	enum class ECsvToken
	{
		word = 0, //后面跟着的是 ,
		word_with_new_line = 1, //后面跟着的是 换行符
		word_with_end = 2, //后面跟着的结束符
		end = 3,
		error = 4,
	};


	class CsvToken
	{
	private:
		const static char C_NEW_LINE = '\n';
		const static char C_RETURN = '\r';
		const static char C_COMMAS = ',';
		const static char C_QUOTES = '"';

	public:

		std::string _buf;
		int _offset;

		CsvToken(const char* buff, int len)
		{
			std::string text = "";
			if (buff != nullptr)
			{
				if (len >= 3 && buff[0] == (char)0xef && buff[1] == (char)0xbb && buff[2] == (char)0xbf)
				{
					text = buff + 3;
				}
				else
					text = buff;
			}
			_buf = text;
			_offset = 0;
		}

		CsvToken(const std::string& buf)
		{
			_buf = buf;
			_offset = 0;
		}

		bool IsEnd() { return _offset >= _buf.length(); }

		ECsvToken Next(std::string& word)
		{
			//1. 检查是否已经到了结尾
			word ="";

			int buf_len = _buf.length();
			if (_offset >= buf_len)
				return ECsvToken::end;

			//2. 读取第一个字符
			char first_char = _buf[_offset];
			switch (first_char)
			{
			case C_QUOTES: // " 碰到了这个
			{
				bool contain_double;
				int end_index = _index_of_next_quotes(_buf, _offset + 1,&contain_double);
				if (end_index == -1)
				{
					_offset = buf_len;
					return ECsvToken::error;
				}

				int start = _offset + 1;
				int count = end_index - start;
				word = _buf.substr(start, count);

				if (contain_double)
					_format_str(word);

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
					return ECsvToken::error;
				}

				int start = _offset;
				int count = end_index - start;
				_offset = end_index;
				word = _buf.substr(start, count);
				return _advance_split_symb();
			}
			}
		}

	private:
		ECsvToken _advance_split_symb()
		{
			if (_offset >= _buf.length())
				return ECsvToken::word_with_end;

			char c = _buf[_offset];
			switch (c)
			{
			case C_COMMAS: // ,
			{
				_offset++;
				return ECsvToken::word;
			}
			case C_NEW_LINE:// \n
			{
				_offset++;
				if (_offset >= _buf.length())
					return ECsvToken::word_with_new_line;

				if (_buf[_offset] == C_RETURN) // \n\r                
					_offset++;

				return ECsvToken::word_with_new_line;
			}
			case C_RETURN: // \r
			{
				_offset++;
				if (_offset >= _buf.length())
					return ECsvToken::word_with_new_line;
				if (_buf[_offset] == C_NEW_LINE) // \r\n
					_offset++;
				return ECsvToken::word_with_new_line;
			}

			default:
				return ECsvToken::error;
			}
		}

		void _format_str(std::string& str)
		{
			for (;;)
			{
				auto pos = str.find("\"\"");
				if (pos <0)
					return;

				str.replace(pos, 2, "\"");
			}
		}

		int _index_of_str_end(const std::string& buf, int index)
		{
			for (int i = index; i < buf.length(); i++)
			{
				char c = buf[i];
				if (c == C_COMMAS || c == C_NEW_LINE || c == C_RETURN)
					return i;
			}
			return -1;
		}

		int _index_of_next_quotes(const std::string& buf, int index, bool* contain_double)
		{
			*contain_double = false;
			for (int i = index; i < buf.length() - 1; i++)
			{
				char c = buf[i];
				if (c == C_QUOTES)
				{
					if (buf[i + 1] == C_QUOTES)
					{
						*contain_double = true;
						i++;
					}
					else
						return i;
				}
			}
			return -1;
		}
	};

	class CsvReader
	{
	private:
		CsvToken Token;
	public:
		CsvReader(const char* buff, int count)
			:Token(buff, count)
		{
		}

		CsvReader(const std::string& buf)
			:Token(buf)
		{
		}

		bool IsEnd() { return Token.IsEnd(); }

		bool ReadRow(std::vector<std::string>& out_list, bool clear_list = true)
		{
			if (clear_list)
				out_list.clear();

			if (IsEnd())
				return false;
			for (; ; )
			{
				std::string word;
				auto r = Token.Next(word);
				switch (r)
				{
				case ECsvToken::word:
					out_list.push_back(word);
					break;

				case ECsvToken::word_with_end:
				case ECsvToken::word_with_new_line:
					out_list.push_back(word);
					return true;
				case ECsvToken::error:
					return false;
				case ECsvToken::end:
					return false;
				default:
					break;
				}
			}
		}

		bool ReadWord(std::string& word)
		{
			word = "";
			if (IsEnd())
				return false;
			for (; ; )
			{
				auto r = Token.Next(word);
				switch (r)
				{
				case ECsvToken::word:
				case ECsvToken::word_with_end:
				case ECsvToken::word_with_new_line:
					return true;
				case ECsvToken::end:
					return false;
				case ECsvToken::error:
					return false;
				default:
					break;
				}
			}
		}
	};
}
