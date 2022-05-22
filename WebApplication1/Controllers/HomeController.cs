using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using WebApplication1.DAO;
using WebApplication1.Service;
using Microsoft.Extensions.Configuration;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;
       
        private static IWebHostEnvironment _webHostEnvironment;
        private readonly Plate _plate;
        public int a=0;

        
        

        public HomeController(ILogger<HomeController> logger,IMemoryCache memoryCache,IWebHostEnvironment webHostEnvironment,Plate plate)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _webHostEnvironment = webHostEnvironment;
            _plate = plate;
            
        }

       

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Index()
        {
            
            return View();
        }
        
        public IActionResult UploadString(String str)
        {
            String number;
            CacheData _data;
            
            lock (_plate)
            {
                if (_memoryCache.TryGetValue(_plate.ToString(), out _data))
                {
                    return View("NoSpace");
                }
                _plate.Number=_plate.Number+1;
                if (_plate.Number==1000)
                {
                    _plate.Number = 000;
                }

                number = _plate.Number.ToString();
            }

            number = number.PadLeft(3, '0');
            ViewData["plate"] = number;

            _memoryCache.Set(number.ToString(), new CacheData()
            {
                type = "string",
                value = str
            }, TimeSpan.FromSeconds(30));
            ViewData["plate"] = number;
            return View();
        }

        public async Task<IActionResult> UploadImage(List<IFormFile> files)
        {
           
            if (files.Count==0||files.Count>1)
            {
                ViewData["message"] = "NotOnlyFile";
                return View("NoFile");
            }
            
            
            String number;
            CacheData _data;
            lock (_plate)
            {
                if (_memoryCache.TryGetValue(_plate.ToString(), out _data))
                {
                    return View("NoSpace");
                }
                _plate.Number=_plate.Number+1;
                if (_plate.Number==1000)
                {
                    _plate.Number = 0;
                }

                number = _plate.Number.ToString();
            }
            number = number.PadLeft(3, '0');

            ViewData["plate"] = number;
            
            
            var file = files.First();
            var fileType = file.ContentType.Split("/");
            var fileExt = file.FileName.Split(".")[1];
            
            var webRootPath = _webHostEnvironment.WebRootPath;
            var dirPath = webRootPath + "/image/";
            var path = $@"{dirPath}{number}.{fileExt}";
            
            if(System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            
            if (file.Length> 10000000)
            {
                ViewData["message"] = "FileTooBig";
                return View("NoFile");
            }
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            _memoryCache.Set(number.ToString(), new CacheData()
            {
                type = fileType[0],
                fileContentType = files.First().ContentType,
                value = "/image/"+number+"."+fileExt,
            }, TimeSpan.FromSeconds(30));

            FileExtension.DeleteFile(path);
            return View();
        }

        public  IActionResult GetCache(String plate)
        {
            CacheData data;
            if (!_memoryCache.TryGetValue(plate, out data))
            {
                ViewData["IsValue"] = false;
                return View("NotValue");
            }

            if (data.type == "string")
            {
                ViewData["value"] = data.value;
                return View();
            }
            
            //type is file
            return File(data.value, data.fileContentType);
        }
        
    }
}