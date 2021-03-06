﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;


namespace BasicPlugin
{
    public class CreateContactTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService)); // Enables writing to the tracing log

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)     // provides access to the context for the event that executes Plugin
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.  
                Entity entity = (Entity)context.InputParameters["Target"];

                // Obtain the organization service reference which you will need for  
                // web service calls.  
                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    // Plug-in business logic goes here.  
                    // Create a task activity to follow up with the account customer in 7 days. 
                    Entity contactCreate = new Entity("task");  // Activity type

                    contactCreate["subject"] = "Send e-mail to update password in 6 months.";
                    contactCreate["description"] =
                        "Follow up and ensure password has been updated.";
                    contactCreate["scheduledstart"] = DateTime.Now.AddMonths(6);
                    contactCreate["scheduledend"] = DateTime.Now.AddMonths(6);
                    contactCreate["category"] = context.PrimaryEntityName;

                    // Refer to the account in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = "contact";

                        contactCreate["regardingobjectid"] =
                        new EntityReference(regardingobjectidType, regardingobjectid);
                    }

                    // Create the task in Microsoft Dynamics CRM.
                    tracingService.Trace("ContactCreatePlugin: Creating and Updating Contact entity.");
                    service.Create(contactCreate);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in ContactCreateUpdatePlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("ContactCreateUpdatePlugin: {0}", ex.ToString());
                    throw;
                }
            }
            //  throw new NotImplementedException();
        }
    }
}
