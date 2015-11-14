﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cafe.Commands.Tab;
using Cafe.Web.ActionFilters;

namespace Cafe.Web.Controllers
{
    [IncludeLayoutData]
    public class ChefController : Controller
    {
        public ActionResult Index()
        {
            return View(Domain.ChefTodoListQueries.GetTodoList());
        }

        public ActionResult MarkPrepared(Guid id, FormCollection form)
        {
            Domain.Dispatcher.SendCommand(new MarkFoodPrepared
            {
                Id = id,
                MenuNumbers = (from entry in form.Keys.Cast<string>()
                               where form[entry] != "false"
                               let m = Regex.Match(entry, @"prepared_\d+_(\d+)")
                               where m.Success
                               select int.Parse(m.Groups[1].Value)
                              ).ToList()
            });

            return RedirectToAction("Index");
        }
    }
}
