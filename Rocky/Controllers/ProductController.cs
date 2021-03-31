using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rocky_DataAccess;
using Rocky_Models;
using Rocky_Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rocky_Utility;
using Microsoft.EntityFrameworkCore;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    [Authorize(Roles =MC.AdminRole)]
    public class ProductController : Controller
    {

        private readonly IProductRepository _prodRepo;

        private readonly IWebHostEnvironment _webHostEnviroment;


        public ProductController(IProductRepository prodRepo, IWebHostEnvironment webHostEnvironment)
        {

            _prodRepo = prodRepo;
            _webHostEnviroment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            IEnumerable<Product> obList = _prodRepo.GetAll(includeProperties:"Category,ApplicationType");

            return View(obList);
        }



        [HttpGet]
        public IActionResult Upsert(int? id)
        {



            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _prodRepo.GetAllDropDown(MC.Category),
                ApplicationTypeSelectList = _prodRepo.GetAllDropDown(MC.Application)
            };

            if (id == null)
            {


                return View(productVM);


            }
            else
            {
                productVM.Product = _prodRepo.Find(id.GetValueOrDefault());
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                return View(productVM);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;

                string webRootPath = _webHostEnviroment.WebRootPath;

                if (productVM.Product.Id == 0)
                {

                    //creating


                    string upload = webRootPath + MC.ImagePath;

                    string filename = Guid.NewGuid().ToString();

                    string extension = Path.GetExtension(files[0].FileName);


                    using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))

                    {
                        files[0].CopyTo(fileStream);
                    }



                    productVM.Product.Image = filename + extension;

                   _prodRepo.Add(productVM.Product);

                }

                else
                {
                    var objFromDb = _prodRepo.FirstOrDefault(u => u.Id == productVM.Product.Id,isTracking:false);

                    if (files.Count > 0)
                    {

                        string upload = webRootPath + MC.ImagePath;

                        string filename = Guid.NewGuid().ToString();

                        string extension = Path.GetExtension(files[0].FileName);

                        var oldFile = Path.Combine(upload, objFromDb.Image);

                        if (System.IO.File.Exists(oldFile))
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        using (var fileStream = new FileStream(Path.Combine(upload, filename + extension), FileMode.Create))

                        {
                            files[0].CopyTo(fileStream);
                        }

                        productVM.Product.Image = filename + extension;
                    }
                    else
                    {
                        productVM.Product.Image = objFromDb.Image;
                    }

                   _prodRepo.Update(productVM.Product);
                }

                _prodRepo.Save();
                return RedirectToAction("Index");
            }

            productVM.CategorySelectList = _prodRepo.GetAllDropDown(MC.Category);
            productVM.ApplicationTypeSelectList = _prodRepo.GetAllDropDown(MC.Category);

            return View(productVM);

        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {

            if (id == null || id == 0)
            {
                return NotFound();
            }
            var obj = _prodRepo.Find(id.GetValueOrDefault());

            if (obj == null)
            {
                return NotFound();
            }
            return View(obj);


        }

        public IActionResult Delete(int? id)
        {


            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product product = _prodRepo.FirstOrDefault(u=>u.Id == id, includeProperties: "Category,ApplicationType");

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }




        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {

            var obj = _prodRepo.Find(id.GetValueOrDefault());

            string upload = _webHostEnviroment.WebRootPath + MC.ImagePath;



            var oldFile = Path.Combine(upload, obj.Image);

            if (System.IO.File.Exists(oldFile))
            {
                System.IO.File.Delete(oldFile);

                _prodRepo.Remove(obj);
                _prodRepo.Save();
                return RedirectToAction("Index");
            }

            else
            {
                return NotFound();
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _prodRepo.Update(obj);
                _prodRepo.Save();
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
