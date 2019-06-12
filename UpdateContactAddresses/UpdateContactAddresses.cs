using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace UpdateContactAddresses
{
    public class UpdateContactAddressses : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Create a tracing instance to log progress of this plugin.
            ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Obtain the organization service reference.
                IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = factory.CreateOrganizationService(null);

                if (pluginExecutionContext.InputParameters.Contains("Target") && pluginExecutionContext.InputParameters["Target"] is Entity) // Checking that it contains 'Target' and that it is an entity
                {
                    // Obtain the target entity from the input parameters.
                    Entity account = (pluginExecutionContext.InputParameters["Target"] as Entity);

                    // Verify that the target entity represents an account. 
                    if (account.LogicalName != "account")
                    {
                        tracing.Trace("This entity is not an Account entity. It is likely that this plug-in was not registered correctly (was an incorrect \"Primary Entity\" selected? It should be an Account entity).");
                        return;
                    }
                    var query = new QueryExpression("contact"); // Getting contacts
                    query.Criteria.AddCondition("accountid", ConditionOperator.Equal, pluginExecutionContext.PrimaryEntityId); // Ensure contact is related to account

                    var result = service.RetrieveMultiple(query);
                    tracing.Trace("The QueryExpression found " + result.TotalRecordCount.ToString() + " associated contacts.");

                    foreach (Entity contact in result.Entities)
                    {
                        tracing.Trace("Updating contact addresses " + contact.ToString() + " address...");
                        contact["address1_line1"] = account.GetAttributeValue<string>("address1_line1");
                        contact["address1_line2"] = account.GetAttributeValue<string>("address1_line2");
                        contact["address1_line3"] = account.GetAttributeValue<string>("address1_line3");
                        contact["address1_city"] = account.GetAttributeValue<string>("address1_city");
                        contact["address1_county"] = account.GetAttributeValue<string>("address1_county");
                        contact["address1_postalcode"] = account.GetAttributeValue<string>("address1_postalcode");
                        contact["address1_country"] = account.GetAttributeValue<string>("address1_country");
                        service.Update(contact);
                        tracing.Trace("Contact " + contact.ToString() + " address updated.");
                    }
                }
                tracing.Trace("Completed execution of plugin " + this.GetType().Name + ".");
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in plugin " + this.GetType().Name + ".", ex);
            }
            catch (Exception ex)
            {
                tracing.Trace("An error occurred executing plugin " + this.GetType().Name + ".");
                tracing.Trace("\t\tError: " + ex.Message);
                throw ex;
            }
        }
    }
}
