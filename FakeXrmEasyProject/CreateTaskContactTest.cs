//  Welcome to your First Test with Fake Xrm Easy!
//
//  We are going to create a new simple test to get up to speed with the basic concepts.
// 
//  First, let's add a code file to your project, where we'll add our first test.
//
//  We're going to add a couple of using statements....
//  
//  
using System;

using Xunit;
using FakeItEasy;
using FakeXrmEasy;

using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System.Reflection;

//  Now...
//
//  Let's declare a class where we'll have our tests...
//
namespace MyFirstXrmUnitTestProject
{
    public class MyTests
    {

        //"Fact" is how we tell Xunit that this 
        // is a test so that it will be discovered by the test runner.... 
        // if you need more info, just go to the Xunit project page

        [Fact]
        public void MyFirstTest()
        {
            //Ok, this is going to be our test method body

            //But before doing anything...

            //  FakeXrmEasy is based on the state-based testing paradigm, 
            //  which is made of, roughly, 3 easy steps:

            //1) We define the initial state of our test.

            //2) Then, we execute the piece of logic which we want to test, 
            //   which will produce a new state, the final state.

            //3) Finally, we verify that the final state is the expected state (assertions).

            //Let's implement those now

            // 1) Define the initial state
            // -----------------------------------------------------------------------

            //  Our initial state is going to be stored in what we call a faked context:

            XrmFakedContext context = new XrmFakedContext();

            //You can think of a context like an Organisation database which stores entities In Memory.

            //We can also use TypedEntities but we need to tell the context where to look for them, 
            //this could be done, easily, like this:

            // context.ProxyTypesAssembly = Assembly.GetAssembly(typeof(Account));
            var account = new Entity();
            account.LogicalName = "account";
            account.Attributes["id"] = Guid.NewGuid();
            account.Attributes["name"] = "My First Faked Account yeah!";
            //We have to define our initial state now, 
            //by calling the Initialize method, which expects a list of entities.

            //var account = new Account() { Id = Guid.NewGuid(), Name = "My First Faked Account yeah!" };

            context.Initialize(new List<Entity>() {
                account
            });

            //With the above example, we initialized our context with a single account record

            // 2) Execute our logic
            // -----------------------------------------------------------------------
            //
            // We need to get a faked organization service first, by calling this method:

            var service = context.GetOrganizationService();

            // That line is the most powerful functionality of FakeXrmEasy
            // That method has returned a reference to an OrganizationService 
            // which you could pass to your plugins, codeactivities, etc, 
            // and, from now on, every create, update, delete, even queries, etc
            // will be reflected in our In Memory context

            // In a nutshell, everything is already mocked for you... cool, isn't it?

            // Now... 

            // To illustrate this...

            // Let's say we have a super simple piece of logic which updates an account's name

            // Let's do it!

            var accountToUpdate = new Entity();
            accountToUpdate.LogicalName = "account";
            accountToUpdate.Id = account.Id;
            accountToUpdate.LogicalName = "A new faked name!";

            //var accountToUpdate = new Account() { Id = account.Id }; -- had to intitialise entity above first, then set its values
            //accountToUpdate.Name = "A new faked name!";
            service.Update(accountToUpdate);
            // Done!
            //We have successfully executed the code we want to test..
            // Now...
            // The final step is...
            // 3) Verify final state is the expected state
            // -----------------------------------------------------------------------
            //
            //We are going to use Xunit assertions.
            var updateAccountName = new Entity();
            updateAccountName.LogicalName = "account";

            XrmFakedContext fcntx = new XrmFakedContext();
            var updatedAccountName = context.CreateQuery("account")
                                    .Where(e => e.Id == account.Id)
                                    .Select(a => a.LogicalName)
                                    .FirstOrDefault();
            //And finally, validate the account has the expected name
            Assert.Equal("A new faked name!", updatedAccountName);
            // And we are DONE!
            // We have successfully implemented our first test!
        }
    }
}
