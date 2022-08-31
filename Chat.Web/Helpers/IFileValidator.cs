using Microsoft.AspNetCore.Http;

namespace Chat.Web.Helpers
{
    public interface IFileValidator
    {
        bool IsValid(IFormFile file);
    }
}
