#if NETCOREAPP
using Microsoft.AspNetCore.Mvc.Filters;

namespace XiaoLi.NET.Mvc.Filters
{
    
    /// <summary>
    /// 在这里定义你的结果格式
    /// </summary>
    public interface IUnifiedResultHandler
    {
        void HandleActionResult(ResultExecutingContext context);
    }
}
#endif