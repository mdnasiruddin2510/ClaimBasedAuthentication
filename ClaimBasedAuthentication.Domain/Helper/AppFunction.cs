using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaimBasedAuthentication.Domain.Helper
{
    public static class AppFunction
    {
        public static string FileUpload(this IWebHostEnvironment _webHostEnvironment, IFormFile file, string path = "", bool isFileName = false)
        {
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Upload");
            if (path != null)
            {
                uploadPath = Path.Combine(uploadPath, path);
            }
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            var filePath = "";
            if (isFileName)
            {
                filePath = Guid.NewGuid().ToString("N") + "_" + file.FileName;
            }
            else
            {
                filePath = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            }
            filePath = Path.Combine(uploadPath, filePath);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            var contentRootPath = _webHostEnvironment.ContentRootPath;
            var relativePath = filePath.Replace(contentRootPath, "");
            return relativePath.Replace(@"\wwwroot", "").Replace(@"\", "/");
        }
    }
}
