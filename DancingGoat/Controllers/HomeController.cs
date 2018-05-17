﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DancingGoat.Areas.Admin;
using DancingGoat.Models;

using KenticoCloud.Personalization;
using KenticoCloud.Personalization.MVC;
using KenticoCloud.Delivery;

namespace DancingGoat.Controllers
{
    public class HomeController : ControllerBase
    {
        private const string VISITOR_NOT_FOUND_CODE = "CR404.2";
        private readonly PersonalizationClient personalizationClient;

        public HomeController()
        {
            // Disable personalization when PersonalizationToken is not set
            var personalizationToken = ConfigurationManager.AppSettings["PersonalizationToken"];

            if (!string.IsNullOrWhiteSpace(personalizationToken) && AppSettingProvider.ProjectId.HasValue)
            {
                personalizationClient = new PersonalizationClient(personalizationToken, AppSettingProvider.ProjectId.Value);
            }
        }

        public async Task<ActionResult> Index()
        {
            var response = await client.GetItemAsync<Home>("home");

            var viewModel = new HomeViewModel
            {
                ContentItem = response.Item,
            };

            // Show promotion banner by default
            var showPromotion = true;

            if (personalizationClient != null)
            {
                // Get User ID of the current visitor
                var visitorUid = Request.GetCurrentPersonalizationUid();
                if (!string.IsNullOrEmpty(visitorUid))
                {
                    // Determine whether the visitor submitted a form
                    try
                    {
                        var visitorSegments = await personalizationClient.GetVisitorSegmentsAsync(visitorUid);
                        
                        showPromotion = !visitorSegments.Segments.Any(
                            s => string.Equals(s.Codename, "Customers_Who_Requested_a_Coffee_Sample",
                                StringComparison.OrdinalIgnoreCase)
                        );
                    }
                    catch (PersonalizationException e)
                    {
                        if (e.Code != VISITOR_NOT_FOUND_CODE)
                        {
                            throw;                            
                        }
                    }
                }
            }
            var codeName = showPromotion ? "home_page_promotion" : "home_page_hero_unit";
            viewModel.Header = response.Item.HeroUnit.Cast<HeroUnit>().FirstOrDefault(x => x.System.Codename == codeName);

            return View(viewModel);
        }


        [ChildActionOnly]
        public ActionResult CompanyAddress()
        {
            var contact = Task.Run(() => client.GetItemAsync<Home>("home", new ElementsParameter("contact"))).Result.Item.Contact;

            return PartialView("CompanyAddress", contact);
        }
    }
}
