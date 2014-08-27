﻿using System.Web.Mvc;
using Web.Models;
using Web.Storage;

namespace Web.Controllers
{
    public class EditController : Controller
    {
        
        // GET: Edit
        public ActionResult Index()
        {
//            EditViewModel vm = new EditViewModel();
//            vm.title = "Title";
//            vm.parapraphs = new ParagraphViewModel[]
//            {
//                new ParagraphViewModel()
//                {
//                    id = "idididid",
//                    nextParagraph = "nextidididid",
//                    prevParagraph = "previdididid",
//                    paragraphIndex = 10,
//                    rawText = "rawrawraw"
//                }, new ParagraphViewModel()
//                {
//                    id = "idididid2",
//                    nextParagraph = "nextidididid2",
//                    prevParagraph = "previdididid2",
//                    paragraphIndex = 11,
//                    rawText = "rawrawraw2"
//                },
//                new ParagraphViewModel()
//                {
//                    id = "idididid3",
//                    nextParagraph = "nextidididid3",
//                    prevParagraph = "previdididid3",
//                    paragraphIndex = 10,
//                    rawText = "rawrawraw3"
//                }
//            };
//            return Json(vm,JsonRequestBehavior.AllowGet);
            return View();
        }
        [HttpPost]
        public ActionResult Index(EditViewModel vm)
        {
            ApplicationDbContext context=new ApplicationDbContext();
            var article = ArticleModel.GenerateArticle(vm.Title, null);
            context.Articles.Add(article);
            context.SaveChanges();
            var data=System.Web.Helpers.Json.Decode<ParagraphDataModel[]>(vm.Context);
            ParagraphTableManager ptm=new ParagraphTableManager(new TableStorageConnection());
            foreach (var paragraphDataModel in data)
            {
                ptm.AddParagraph(article.ArticleId,0,paragraphDataModel);
            }
            return null;
        }
    }
}