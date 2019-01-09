﻿using System;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Trees;
using Analytics.Models;
using Analytics.Controllers;

namespace Analytics
{
    public class UmbracoStartup : ApplicationEventHandler
    {
        /// <summary>
        /// Register Install & Uninstall Events
        /// </summary>
        /// <param name="umbracoApplication"></param>
        /// <param name="applicationContext"></param>
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            //Add Tree Node Rendering Event - Used to check if user is admin to display settings node in tree
            TreeControllerBase.TreeNodesRendering += TreeControllerBase_TreeNodesRendering;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TreeControllerBase_TreeNodesRendering(TreeControllerBase sender, TreeNodesRenderingEventArgs e)
        {
            //Get Current User
            var currentUser = User.GetCurrent();

            //This will only run on the analyticsTree & if the user is NOT admin
            if (sender.TreeAlias == "analyticsTree" && !currentUser.IsAdmin())
            {
                //setting node to remove
                var settingNode = e.Nodes.SingleOrDefault(x => x.Id.ToString() == "settings");

                //Ensure we found the node
                if (settingNode != null)
                {
                    //Remove the settings node from the collection
                    e.Nodes.Remove(settingNode);
                }
            }

            //This will only run on the analyticsTree
            if (sender.TreeAlias == "analyticsTree")
            {
                AnalyticsApiController gaApi = new AnalyticsApiController();

                Profile profile = AnalyticsHelpers.GetProfile();

                if (profile != null)
                {
                    var ecommerceNode = e.Nodes.SingleOrDefault(x => x.Id.ToString() == "ecommerce");
                    if (ecommerceNode != null)
                    {
                        try
                        {
                            // check if profile has any ecommerce data for last 3 years
                            StatsApiResult transactions = gaApi.GetTransactions(profile.Id, DateTime.Now.AddYears(-3), DateTime.Now);
                            if (!transactions.ApiResult.Rows.Any())
                            {
                                //Remove the ecommerce node from the collection
                                //If no Rows returned from API - then remove the node from the tree
                                e.Nodes.Remove(ecommerceNode);
                            }
                        }
                        catch
                        {
                            //Remove the ecommerce node from the collection
                            //If the API throws an ex then remove the node from the tree as well
                            e.Nodes.Remove(ecommerceNode);
                        }
                    }
                }
            }
        }

    }
}
