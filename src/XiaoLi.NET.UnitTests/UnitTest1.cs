using System.Threading.Channels;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace XiaoLi.NET.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            string str = "sadkhkajsfhjer";
            for (int i = 0; i < str.Length; i++)
            {
                var item = str[i];
                ConsoleOutput.Instance.Write(item.ToString(),OutputLevel.Information);
                item = 'a';
            }
        }
    }
}