﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace N2ImageAgent.AzureBlob
{
    public class indexModel : PageModel
    {
        public void OnGet()
        {
            Response.Redirect("/index.html");
        }
    }
}