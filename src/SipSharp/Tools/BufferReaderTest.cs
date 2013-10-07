using System.Text;
using Xunit;

namespace SipSharp.Tools
{
    public class BufferReaderTest
    {
        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly BufferReader _reader;

        public BufferReaderTest()
        {
            _reader = new BufferReader(_encoding);
        }

        [Fact]
        private void TestGetString()
        {
            byte[] buffer = _encoding.GetBytes("0123456789");
            int startIndex = 3;
            int endIndex = 5;
            Assert.Equal("345", _encoding.GetString(buffer, startIndex, endIndex - startIndex + 1));
        }


        [Fact]
        private void TestReadLine()
        {
            byte[] bytes = _encoding.GetBytes("Hej\r\npå\r\ndig\r\n\t multi.\r\n");
            _reader.Assign(bytes, 0, bytes.Length);
            Assert.Equal("Hej", _reader.ReadLine());
            Assert.Equal("på", _reader.ReadLine());
            string temp = _reader.ReadLine();
            Assert.Equal("dig multi.", temp);
        }

        [Fact]
        private void TestReadUntilOneHeader()
        {
            byte[] bytes = _encoding.GetBytes("Welcome :  Here\r\n");
            _reader.Assign(bytes, 0, bytes.Length);

            Assert.Equal("Welcome", _reader.ReadUntil(':'));
            _reader.Consume(); // eat delimiter
            _reader.Consume('\t', ' ');
            Assert.Equal("Here", _reader.ReadLine());
        }

        [Fact]
        private void TestReadUntilTwoHeaders()
        {
            byte[] bytes = _encoding.GetBytes("Welcome :  Here\r\nNext:header\r\n");
            _reader.Assign(bytes, 0, bytes.Length);
            _reader.ReadUntil(':');
            _reader.Consume(); // eat delimiter
            _reader.Consume('\t', ' ');
            _reader.ReadLine();
            Assert.Equal("Next", _reader.ReadUntil(':'));
            _reader.Consume(); // eat delimiter
            _reader.Consume('\t', ' ');
            Assert.Equal("header", _reader.ReadLine());
        }
    }
}