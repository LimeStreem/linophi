﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class MyPageViewModel
    {
        public SearchResultArticle[] articles;
        public bool IsMyPage { get; set; }
    }
}